using JT808.Enums;
using JT808.Extensions;
using JT808.Models;

namespace JT808.Serializer
{
    /// <summary>
    /// JT808 protocol serializer.
    /// Handles message framing: escape encoding, checksum, flags.
    /// 
    /// Escape rules:
    ///   0x7E -> 0x7D 0x02
    ///   0x7D -> 0x7D 0x01
    /// </summary>
    public static class JT808Serializer
    {
        private const byte Flag = 0x7E;
        private const byte EscapeFlag = 0x7D;
        private const byte EscapeReplacement7E = 0x02;
        private const byte EscapeReplacement7D = 0x01;

        /// <summary>
        /// Serialize a JT808 package to a byte array (including start/end flags).
        /// </summary>
        public static byte[] Serialize(JT808Package package)
        {
            // Build raw content (header + body) before escaping
            byte[] rawContent = BuildRawContent(package);

            // Calculate checksum (XOR of all raw bytes)
            byte checksum = CalculateChecksum(rawContent);

            // Escape raw content + checksum
            byte[] escaped = Escape(Append(rawContent, checksum));

            // Wrap with flags
            var result = new byte[escaped.Length + 2];
            result[0] = Flag;
            Array.Copy(escaped, 0, result, 1, escaped.Length);
            result[result.Length - 1] = Flag;
            return result;
        }

        /// <summary>
        /// Deserialize a byte array into a JT808Package.
        /// The input must start and end with 0x7E.
        /// </summary>
        public static JT808Package Deserialize(byte[] data)
        {
            if (data == null || data.Length < 2)
                throw new ArgumentException("Data too short", nameof(data));
            if (data[0] != Flag || data[data.Length - 1] != Flag)
                throw new ArgumentException("Missing start/end flags", nameof(data));

            // Remove flags and unescape
            byte[] inner = Unescape(data[1..(data.Length - 1)]);

            // Verify checksum (last byte of inner is checksum)
            byte receivedChecksum = inner[inner.Length - 1];
            byte[] rawContent = inner[0..(inner.Length - 1)];
            byte expectedChecksum = CalculateChecksum(rawContent);
            if (receivedChecksum != expectedChecksum)
                throw new InvalidDataException(
                    $"Checksum mismatch: expected 0x{expectedChecksum:X2}, got 0x{receivedChecksum:X2}");

            // Parse header and body from rawContent
            return ParsePackage(rawContent);
        }

        private static byte[] BuildRawContent(JT808Package package)
        {
            var header = package.Header;
            bool isSubPackage = header.IsSubPackage;
            int headerSize = isSubPackage ? 16 : 12; // 12 bytes base, +4 for sub-package info

            byte[] phoneBytes = JT808BcdExtension.EncodeBcd(header.PhoneNumber, 6);
            int bodyLength = package.Bodies.Length;

            // Update body length in properties
            ushort properties = (ushort)((header.MsgBodyProperties & 0xFC00) | (bodyLength & 0x03FF));
            if (isSubPackage)
                properties |= 0x2000;

            var raw = new byte[headerSize + bodyLength];
            int offset = 0;

            raw[offset++] = (byte)(header.MsgId >> 8);
            raw[offset++] = (byte)(header.MsgId & 0xFF);
            raw[offset++] = (byte)(properties >> 8);
            raw[offset++] = (byte)(properties & 0xFF);
            Array.Copy(phoneBytes, 0, raw, offset, 6); offset += 6;
            raw[offset++] = (byte)(header.MsgSerialNumber >> 8);
            raw[offset++] = (byte)(header.MsgSerialNumber & 0xFF);

            if (isSubPackage)
            {
                ushort total = header.TotalSubPackets ?? 0;
                ushort idx = header.SubPacketIndex ?? 0;
                raw[offset++] = (byte)(total >> 8);
                raw[offset++] = (byte)(total & 0xFF);
                raw[offset++] = (byte)(idx >> 8);
                raw[offset++] = (byte)(idx & 0xFF);
            }

            Array.Copy(package.Bodies, 0, raw, offset, bodyLength);
            return raw;
        }

        private static JT808Package ParsePackage(byte[] rawContent)
        {
            int offset = 0;
            ushort msgId = (ushort)((rawContent[offset++] << 8) | rawContent[offset++]);
            ushort properties = (ushort)((rawContent[offset++] << 8) | rawContent[offset++]);
            bool isSubPackage = (properties & 0x2000) != 0;

            byte[] phoneBytes = rawContent[offset..(offset + 6)]; offset += 6;
            string phone = JT808BcdExtension.DecodeBcd(phoneBytes);
            ushort serial = (ushort)((rawContent[offset++] << 8) | rawContent[offset++]);

            ushort? totalSub = null;
            ushort? subIdx = null;
            if (isSubPackage)
            {
                totalSub = (ushort)((rawContent[offset++] << 8) | rawContent[offset++]);
                subIdx = (ushort)((rawContent[offset++] << 8) | rawContent[offset++]);
            }

            int bodyLength = properties & 0x03FF;
            byte[] body = bodyLength > 0
                ? rawContent[offset..(offset + bodyLength)]
                : Array.Empty<byte>();

            return new JT808Package
            {
                Header = new Models.JT808Header
                {
                    MsgId = msgId,
                    MsgBodyProperties = properties,
                    PhoneNumber = phone,
                    MsgSerialNumber = serial,
                    TotalSubPackets = totalSub,
                    SubPacketIndex = subIdx
                },
                Bodies = body,
                CheckCode = CalculateChecksum(rawContent)
            };
        }

        /// <summary>Compute XOR checksum of all bytes.</summary>
        public static byte CalculateChecksum(byte[] data)
        {
            byte checksum = 0;
            foreach (byte b in data)
                checksum ^= b;
            return checksum;
        }

        /// <summary>Escape 0x7D and 0x7E bytes in data.</summary>
        public static byte[] Escape(byte[] data)
        {
            var result = new List<byte>(data.Length);
            foreach (byte b in data)
            {
                if (b == EscapeFlag)
                {
                    result.Add(EscapeFlag);
                    result.Add(EscapeReplacement7D);
                }
                else if (b == Flag)
                {
                    result.Add(EscapeFlag);
                    result.Add(EscapeReplacement7E);
                }
                else
                {
                    result.Add(b);
                }
            }
            return result.ToArray();
        }

        /// <summary>Unescape previously escaped data.</summary>
        public static byte[] Unescape(byte[] data)
        {
            var result = new List<byte>(data.Length);
            int i = 0;
            while (i < data.Length)
            {
                if (data[i] == EscapeFlag && i + 1 < data.Length)
                {
                    if (data[i + 1] == EscapeReplacement7E)
                    {
                        result.Add(Flag);
                        i += 2;
                    }
                    else if (data[i + 1] == EscapeReplacement7D)
                    {
                        result.Add(EscapeFlag);
                        i += 2;
                    }
                    else
                    {
                        result.Add(data[i++]);
                    }
                }
                else
                {
                    result.Add(data[i++]);
                }
            }
            return result.ToArray();
        }

        private static byte[] Append(byte[] data, byte b)
        {
            var result = new byte[data.Length + 1];
            Array.Copy(data, result, data.Length);
            result[data.Length] = b;
            return result;
        }
    }
}

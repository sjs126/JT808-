using System.Text;
using JT808.Enums;

namespace JT808.Messages
{
    /// <summary>
    /// 0x0100 Terminal registration (终端注册)
    /// Supports both JT/T 808-2011 and JT/T 808-2013.
    /// 
    /// 2011 field layout (fixed-length fields, GBK strings):
    ///   Province ID    (2 bytes) WORD
    ///   City ID        (2 bytes) WORD
    ///   Manufacturer   (5 bytes)
    ///   Terminal Model (8 bytes)
    ///   Terminal ID    (7 bytes)
    ///   License Plate Color (1 byte) BYTE
    ///   License Plate  (variable, GBK)
    ///
    /// 2013 field layout:
    ///   Province ID    (2 bytes) WORD
    ///   City ID        (2 bytes) WORD
    ///   Manufacturer   (5 bytes)
    ///   Terminal Model (20 bytes) -- expanded from 8 bytes
    ///   Terminal ID    (7 bytes)
    ///   License Plate Color (1 byte) BYTE
    ///   License Plate  (variable, GBK)
    /// </summary>
    public class JT808_0x0100 : IJT808MessageBody
    {
        public JT808MsgId MsgId => JT808MsgId.TerminalRegister;

        /// <summary>Province ID (省域ID)</summary>
        public ushort ProvinceId { get; set; }

        /// <summary>City/County ID (市县域ID)</summary>
        public ushort CityId { get; set; }

        /// <summary>Manufacturer ID (制造商ID, 5 bytes)</summary>
        public string ManufacturerId { get; set; } = string.Empty;

        /// <summary>Terminal model (终端型号, 8 bytes for 2011, 20 bytes for 2013)</summary>
        public string TerminalModel { get; set; } = string.Empty;

        /// <summary>Terminal ID (终端ID, 7 bytes)</summary>
        public string TerminalId { get; set; } = string.Empty;

        /// <summary>
        /// License plate color (车牌颜色):
        /// 0=no plate, 1=blue, 2=yellow, 3=black, 4=white, 9=other
        /// </summary>
        public byte LicensePlateColor { get; set; }

        /// <summary>License plate number (车辆标识, GBK)</summary>
        public string LicensePlate { get; set; } = string.Empty;

        /// <summary>Protocol version used for encoding/decoding</summary>
        public JT808Version Version { get; set; } = JT808Version.JTT2013;

        private int TerminalModelLength => Version == JT808Version.JTT2011 ? 8 : 20;

        public byte[] Encode()
        {
            var encoding = Encoding.GetEncoding("GBK");
            byte[] manufacturerBytes = PadOrTruncate(encoding.GetBytes(ManufacturerId), 5);
            byte[] modelBytes = PadOrTruncate(encoding.GetBytes(TerminalModel), TerminalModelLength);
            byte[] terminalIdBytes = PadOrTruncate(encoding.GetBytes(TerminalId), 7);
            byte[] plateBytes = encoding.GetBytes(LicensePlate);

            int totalLength = 2 + 2 + 5 + TerminalModelLength + 7 + 1 + plateBytes.Length;
            var buffer = new byte[totalLength];
            int offset = 0;

            buffer[offset++] = (byte)(ProvinceId >> 8);
            buffer[offset++] = (byte)(ProvinceId & 0xFF);
            buffer[offset++] = (byte)(CityId >> 8);
            buffer[offset++] = (byte)(CityId & 0xFF);
            Array.Copy(manufacturerBytes, 0, buffer, offset, 5); offset += 5;
            Array.Copy(modelBytes, 0, buffer, offset, TerminalModelLength); offset += TerminalModelLength;
            Array.Copy(terminalIdBytes, 0, buffer, offset, 7); offset += 7;
            buffer[offset++] = LicensePlateColor;
            Array.Copy(plateBytes, 0, buffer, offset, plateBytes.Length);

            return buffer;
        }

        public static JT808_0x0100 Decode(byte[] body, JT808Version version = JT808Version.JTT2013)
        {
            var encoding = Encoding.GetEncoding("GBK");
            int modelLen = version == JT808Version.JTT2011 ? 8 : 20;
            int minLength = 2 + 2 + 5 + modelLen + 7 + 1;
            if (body.Length < minLength)
                throw new ArgumentException($"Body too short for 0x0100 ({version})", nameof(body));

            int offset = 0;
            ushort provinceId = (ushort)((body[offset++] << 8) | body[offset++]);
            ushort cityId = (ushort)((body[offset++] << 8) | body[offset++]);
            string manufacturer = encoding.GetString(body, offset, 5).TrimEnd('\0'); offset += 5;
            string model = encoding.GetString(body, offset, modelLen).TrimEnd('\0'); offset += modelLen;
            string terminalId = encoding.GetString(body, offset, 7).TrimEnd('\0'); offset += 7;
            byte plateColor = body[offset++];
            string plate = encoding.GetString(body, offset, body.Length - offset);

            return new JT808_0x0100
            {
                ProvinceId = provinceId,
                CityId = cityId,
                ManufacturerId = manufacturer,
                TerminalModel = model,
                TerminalId = terminalId,
                LicensePlateColor = plateColor,
                LicensePlate = plate,
                Version = version
            };
        }

        private static byte[] PadOrTruncate(byte[] source, int length)
        {
            if (source.Length == length) return source;
            byte[] result = new byte[length];
            Array.Copy(source, result, Math.Min(source.Length, length));
            return result;
        }
    }
}

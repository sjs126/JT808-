using JT808.Enums;

namespace JT808.Messages
{
    /// <summary>
    /// 0x0200 Location information report (位置信息汇报)
    /// Alarm flags, status, lat, lon, altitude, speed, direction, timestamp (BCD YYMMDDHHmmss)
    /// </summary>
    public class JT808_0x0200 : IJT808MessageBody
    {
        public JT808MsgId MsgId => JT808MsgId.LocationReport;

        /// <summary>Alarm flags (报警标志, 4 bytes)</summary>
        public uint AlarmFlag { get; set; }

        /// <summary>Status (状态, 4 bytes)</summary>
        public uint Status { get; set; }

        /// <summary>
        /// Latitude in units of 1e-6 degrees (纬度, 4 bytes).
        /// MSB of Status bit 2 = 0 north, 1 south.
        /// </summary>
        public uint Latitude { get; set; }

        /// <summary>
        /// Longitude in units of 1e-6 degrees (经度, 4 bytes).
        /// MSB of Status bit 3 = 0 east, 1 west.
        /// </summary>
        public uint Longitude { get; set; }

        /// <summary>Altitude in meters (海拔高度, 2 bytes)</summary>
        public ushort Altitude { get; set; }

        /// <summary>Speed in 0.1 km/h (速度, 2 bytes)</summary>
        public ushort Speed { get; set; }

        /// <summary>Direction in degrees 0-359 (方向, 2 bytes)</summary>
        public ushort Direction { get; set; }

        /// <summary>Timestamp in BCD format YYMMDDHHmmss (时间, 6 bytes)</summary>
        public string Timestamp { get; set; } = string.Empty;

        public byte[] Encode()
        {
            byte[] tsBytes = Extensions.JT808BcdExtension.EncodeBcd(Timestamp, 6);
            var buffer = new byte[28];
            int offset = 0;
            WriteUInt32(buffer, ref offset, AlarmFlag);
            WriteUInt32(buffer, ref offset, Status);
            WriteUInt32(buffer, ref offset, Latitude);
            WriteUInt32(buffer, ref offset, Longitude);
            WriteUInt16(buffer, ref offset, Altitude);
            WriteUInt16(buffer, ref offset, Speed);
            WriteUInt16(buffer, ref offset, Direction);
            Array.Copy(tsBytes, 0, buffer, offset, 6);
            return buffer;
        }

        public static JT808_0x0200 Decode(byte[] body)
        {
            if (body.Length < 28)
                throw new ArgumentException("Body too short for 0x0200", nameof(body));
            int offset = 0;
            return new JT808_0x0200
            {
                AlarmFlag = ReadUInt32(body, ref offset),
                Status = ReadUInt32(body, ref offset),
                Latitude = ReadUInt32(body, ref offset),
                Longitude = ReadUInt32(body, ref offset),
                Altitude = ReadUInt16(body, ref offset),
                Speed = ReadUInt16(body, ref offset),
                Direction = ReadUInt16(body, ref offset),
                Timestamp = Extensions.JT808BcdExtension.DecodeBcd(body[offset..(offset + 6)])
            };
        }

        private static void WriteUInt32(byte[] buffer, ref int offset, uint value)
        {
            buffer[offset++] = (byte)(value >> 24);
            buffer[offset++] = (byte)(value >> 16);
            buffer[offset++] = (byte)(value >> 8);
            buffer[offset++] = (byte)(value & 0xFF);
        }

        private static void WriteUInt16(byte[] buffer, ref int offset, ushort value)
        {
            buffer[offset++] = (byte)(value >> 8);
            buffer[offset++] = (byte)(value & 0xFF);
        }

        private static uint ReadUInt32(byte[] buffer, ref int offset)
        {
            uint val = (uint)((buffer[offset] << 24) | (buffer[offset + 1] << 16) |
                              (buffer[offset + 2] << 8) | buffer[offset + 3]);
            offset += 4;
            return val;
        }

        private static ushort ReadUInt16(byte[] buffer, ref int offset)
        {
            ushort val = (ushort)((buffer[offset] << 8) | buffer[offset + 1]);
            offset += 2;
            return val;
        }
    }
}

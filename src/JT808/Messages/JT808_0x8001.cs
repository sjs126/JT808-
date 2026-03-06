using JT808.Enums;

namespace JT808.Messages
{
    /// <summary>
    /// 0x8001 Platform general response (平台通用应答)
    /// </summary>
    public class JT808_0x8001 : IJT808MessageBody
    {
        public JT808MsgId MsgId => JT808MsgId.PlatformGeneralResponse;

        /// <summary>Response message serial number (应答消息流水号)</summary>
        public ushort ReplyMsgSerialNumber { get; set; }

        /// <summary>Response message ID (应答消息ID)</summary>
        public ushort ReplyMsgId { get; set; }

        /// <summary>Result (结果)</summary>
        public JT808PlatformResult Result { get; set; }

        public byte[] Encode()
        {
            var buffer = new byte[5];
            buffer[0] = (byte)(ReplyMsgSerialNumber >> 8);
            buffer[1] = (byte)(ReplyMsgSerialNumber & 0xFF);
            buffer[2] = (byte)(ReplyMsgId >> 8);
            buffer[3] = (byte)(ReplyMsgId & 0xFF);
            buffer[4] = (byte)Result;
            return buffer;
        }

        public static JT808_0x8001 Decode(byte[] body)
        {
            if (body.Length < 5)
                throw new ArgumentException("Body too short for 0x8001", nameof(body));
            return new JT808_0x8001
            {
                ReplyMsgSerialNumber = (ushort)((body[0] << 8) | body[1]),
                ReplyMsgId = (ushort)((body[2] << 8) | body[3]),
                Result = (JT808PlatformResult)body[4]
            };
        }
    }
}

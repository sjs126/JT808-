using System.Text;
using JT808.Enums;

namespace JT808.Messages
{
    /// <summary>
    /// 0x8100 Terminal registration response (终端注册应答)
    /// </summary>
    public class JT808_0x8100 : IJT808MessageBody
    {
        public JT808MsgId MsgId => JT808MsgId.TerminalRegisterResponse;

        /// <summary>Response serial number (应答流水号)</summary>
        public ushort ReplyMsgSerialNumber { get; set; }

        /// <summary>Result (结果)</summary>
        public JT808RegisterResult Result { get; set; }

        /// <summary>Authentication code (鉴权码, only present when Result == Success)</summary>
        public string AuthCode { get; set; } = string.Empty;

        public byte[] Encode()
        {
            byte[] authBytes = Encoding.ASCII.GetBytes(AuthCode);
            bool includeAuth = Result == JT808RegisterResult.Success;
            int totalLength = 2 + 1 + (includeAuth ? authBytes.Length : 0);
            var buffer = new byte[totalLength];
            buffer[0] = (byte)(ReplyMsgSerialNumber >> 8);
            buffer[1] = (byte)(ReplyMsgSerialNumber & 0xFF);
            buffer[2] = (byte)Result;
            if (includeAuth)
                Array.Copy(authBytes, 0, buffer, 3, authBytes.Length);
            return buffer;
        }

        public static JT808_0x8100 Decode(byte[] body)
        {
            if (body.Length < 3)
                throw new ArgumentException("Body too short for 0x8100", nameof(body));
            var result = new JT808_0x8100
            {
                ReplyMsgSerialNumber = (ushort)((body[0] << 8) | body[1]),
                Result = (JT808RegisterResult)body[2]
            };
            if (result.Result == JT808RegisterResult.Success && body.Length > 3)
                result.AuthCode = Encoding.ASCII.GetString(body, 3, body.Length - 3);
            return result;
        }
    }
}

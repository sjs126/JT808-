using System.Text;
using JT808.Enums;

namespace JT808.Messages
{
    /// <summary>
    /// 0x0102 Terminal authentication (终端鉴权)
    /// </summary>
    public class JT808_0x0102 : IJT808MessageBody
    {
        public JT808MsgId MsgId => JT808MsgId.TerminalAuth;

        /// <summary>Authentication code (鉴权码)</summary>
        public string AuthCode { get; set; } = string.Empty;

        public byte[] Encode() => Encoding.ASCII.GetBytes(AuthCode);

        public static JT808_0x0102 Decode(byte[] body)
        {
            return new JT808_0x0102
            {
                AuthCode = Encoding.ASCII.GetString(body)
            };
        }
    }
}

using JT808.Enums;

namespace JT808.Messages
{
    /// <summary>
    /// 0x0003 Terminal logout (终端注销) - body is empty
    /// </summary>
    public class JT808_0x0003 : IJT808MessageBody
    {
        public JT808MsgId MsgId => JT808MsgId.TerminalLogout;

        public byte[] Encode() => Array.Empty<byte>();
    }
}

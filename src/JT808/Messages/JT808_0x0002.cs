using JT808.Enums;

namespace JT808.Messages
{
    /// <summary>
    /// 0x0002 Terminal heartbeat (终端心跳) - body is empty
    /// </summary>
    public class JT808_0x0002 : IJT808MessageBody
    {
        public JT808MsgId MsgId => JT808MsgId.TerminalHeartbeat;

        public byte[] Encode() => Array.Empty<byte>();
    }
}

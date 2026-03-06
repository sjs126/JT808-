using JT808.Enums;

namespace JT808.Messages
{
    /// <summary>
    /// Base interface for all JT808 message bodies
    /// </summary>
    public interface IJT808MessageBody
    {
        JT808MsgId MsgId { get; }
        byte[] Encode();
    }
}

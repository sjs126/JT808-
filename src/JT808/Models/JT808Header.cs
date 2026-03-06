using JT808.Enums;

namespace JT808.Models
{
    /// <summary>
    /// JT808 message header (消息头)
    /// </summary>
    public class JT808Header
    {
        /// <summary>Message ID (消息ID)</summary>
        public ushort MsgId { get; set; }

        /// <summary>
        /// Message body properties (消息体属性)
        /// Bits 0-9: body length
        /// Bits 10-12: encryption type
        /// Bit 13: subpackage flag
        /// Bits 14-15: reserved
        /// </summary>
        public ushort MsgBodyProperties { get; set; }

        /// <summary>Terminal phone number in BCD format (终端手机号, 6 bytes BCD)</summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>Message serial number (消息流水号)</summary>
        public ushort MsgSerialNumber { get; set; }

        /// <summary>Total sub-packages count (消息包封装项 - 消息总包数, only present if subpackage)</summary>
        public ushort? TotalSubPackets { get; set; }

        /// <summary>Current sub-package index (消息包封装项 - 包序号, only present if subpackage)</summary>
        public ushort? SubPacketIndex { get; set; }

        /// <summary>Whether the message is a subpackage (是否分包)</summary>
        public bool IsSubPackage => (MsgBodyProperties & 0x2000) != 0;

        /// <summary>Message body length (消息体长度)</summary>
        public ushort MsgBodyLength => (ushort)(MsgBodyProperties & 0x03FF);
    }
}

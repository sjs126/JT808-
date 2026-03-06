using JT808.Enums;

namespace JT808.Models
{
    /// <summary>
    /// JT808 message package wrapper
    /// </summary>
    public class JT808Package
    {
        /// <summary>Start flag (0x7E)</summary>
        public const byte BeginFlag = 0x7E;

        /// <summary>End flag (0x7E)</summary>
        public const byte EndFlag = 0x7E;

        /// <summary>Message header</summary>
        public JT808Header Header { get; set; } = new JT808Header();

        /// <summary>Message body (decoded bytes, before checksum)</summary>
        public byte[] Bodies { get; set; } = Array.Empty<byte>();

        /// <summary>Checksum (XOR of all bytes between flags)</summary>
        public byte CheckCode { get; set; }

        /// <summary>Protocol version</summary>
        public JT808Version Version { get; set; }
    }
}

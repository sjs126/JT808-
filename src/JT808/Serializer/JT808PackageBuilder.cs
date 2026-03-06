using JT808.Enums;
using JT808.Messages;
using JT808.Models;

namespace JT808.Serializer
{
    /// <summary>
    /// Helper for building JT808 packages from message bodies.
    /// </summary>
    public static class JT808PackageBuilder
    {
        /// <summary>
        /// Build a JT808Package from a message body.
        /// </summary>
        public static JT808Package Build(
            IJT808MessageBody body,
            string phoneNumber,
            ushort serialNumber,
            JT808Version version = JT808Version.JTT2013)
        {
            byte[] bodyBytes = body.Encode();
            ushort properties = (ushort)(bodyBytes.Length & 0x03FF);

            return new JT808Package
            {
                Version = version,
                Header = new JT808Header
                {
                    MsgId = (ushort)body.MsgId,
                    MsgBodyProperties = properties,
                    PhoneNumber = phoneNumber,
                    MsgSerialNumber = serialNumber
                },
                Bodies = bodyBytes
            };
        }

        /// <summary>
        /// Serialize a message body directly to a framed byte array.
        /// </summary>
        public static byte[] Serialize(
            IJT808MessageBody body,
            string phoneNumber,
            ushort serialNumber,
            JT808Version version = JT808Version.JTT2013)
        {
            return JT808Serializer.Serialize(Build(body, phoneNumber, serialNumber, version));
        }
    }
}

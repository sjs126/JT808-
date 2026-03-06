using JT808;
using JT808.Enums;
using JT808.Extensions;
using JT808.Messages;
using JT808.Models;
using JT808.Serializer;
using Xunit;

namespace JT808.Tests
{
    public class JT808SerializerTests
    {
        public JT808SerializerTests()
        {
            JT808Initializer.Initialize();
        }

        [Fact]
        public void Escape_ShouldEscape7E()
        {
            byte[] input = { 0x01, 0x7E, 0x02 };
            byte[] expected = { 0x01, 0x7D, 0x02, 0x02 };
            Assert.Equal(expected, JT808Serializer.Escape(input));
        }

        [Fact]
        public void Escape_ShouldEscape7D()
        {
            byte[] input = { 0x01, 0x7D, 0x02 };
            byte[] expected = { 0x01, 0x7D, 0x01, 0x02 };
            Assert.Equal(expected, JT808Serializer.Escape(input));
        }

        [Fact]
        public void Unescape_ShouldUnescape7E()
        {
            byte[] input = { 0x01, 0x7D, 0x02, 0x02 };
            byte[] expected = { 0x01, 0x7E, 0x02 };
            Assert.Equal(expected, JT808Serializer.Unescape(input));
        }

        [Fact]
        public void Unescape_ShouldUnescape7D()
        {
            byte[] input = { 0x01, 0x7D, 0x01, 0x02 };
            byte[] expected = { 0x01, 0x7D, 0x02 };
            Assert.Equal(expected, JT808Serializer.Unescape(input));
        }

        [Fact]
        public void Escape_Unescape_RoundTrip()
        {
            byte[] original = { 0x7E, 0x7D, 0x00, 0xFF, 0x7E, 0x7D };
            byte[] escaped = JT808Serializer.Escape(original);
            byte[] restored = JT808Serializer.Unescape(escaped);
            Assert.Equal(original, restored);
        }

        [Fact]
        public void CalculateChecksum_XorOfAllBytes()
        {
            byte[] data = { 0x01, 0x02, 0x03 };
            byte expected = (byte)(0x01 ^ 0x02 ^ 0x03);
            Assert.Equal(expected, JT808Serializer.CalculateChecksum(data));
        }

        [Fact]
        public void CalculateChecksum_SingleByte_ReturnsSelf()
        {
            Assert.Equal(0xAB, JT808Serializer.CalculateChecksum(new byte[] { 0xAB }));
        }

        [Fact]
        public void Serialize_Deserialize_Heartbeat_RoundTrip_2011()
        {
            var msg = new JT808_0x0002();
            byte[] frame = JT808PackageBuilder.Serialize(msg, "013800138000", 1, JT808Version.JTT2011);

            Assert.Equal(0x7E, frame[0]);
            Assert.Equal(0x7E, frame[frame.Length - 1]);

            JT808Package pkg = JT808Serializer.Deserialize(frame);
            Assert.Equal((ushort)JT808MsgId.TerminalHeartbeat, pkg.Header.MsgId);
            Assert.Equal("013800138000", pkg.Header.PhoneNumber);
            Assert.Equal(1, pkg.Header.MsgSerialNumber);
            Assert.Empty(pkg.Bodies);
        }

        [Fact]
        public void Serialize_Deserialize_Heartbeat_RoundTrip_2013()
        {
            var msg = new JT808_0x0002();
            byte[] frame = JT808PackageBuilder.Serialize(msg, "013800138001", 2, JT808Version.JTT2013);

            JT808Package pkg = JT808Serializer.Deserialize(frame);
            Assert.Equal((ushort)JT808MsgId.TerminalHeartbeat, pkg.Header.MsgId);
            Assert.Equal("013800138001", pkg.Header.PhoneNumber);
            Assert.Equal(2, pkg.Header.MsgSerialNumber);
        }

        [Fact]
        public void Serialize_Deserialize_TerminalGeneralResponse_RoundTrip()
        {
            var msg = new JT808_0x0001
            {
                ReplyMsgSerialNumber = 100,
                ReplyMsgId = (ushort)JT808MsgId.LocationReport,
                Result = 0x00
            };
            byte[] frame = JT808PackageBuilder.Serialize(msg, "013800138000", 5);
            JT808Package pkg = JT808Serializer.Deserialize(frame);

            Assert.Equal((ushort)JT808MsgId.TerminalGeneralResponse, pkg.Header.MsgId);
            var decoded = JT808_0x0001.Decode(pkg.Bodies);
            Assert.Equal(100, decoded.ReplyMsgSerialNumber);
            Assert.Equal((ushort)JT808MsgId.LocationReport, decoded.ReplyMsgId);
            Assert.Equal(0x00, decoded.Result);
        }

        [Fact]
        public void Serialize_Deserialize_PlatformGeneralResponse_RoundTrip()
        {
            var msg = new JT808_0x8001
            {
                ReplyMsgSerialNumber = 200,
                ReplyMsgId = (ushort)JT808MsgId.TerminalRegister,
                Result = JT808PlatformResult.Success
            };
            byte[] frame = JT808PackageBuilder.Serialize(msg, "013800138000", 10);
            JT808Package pkg = JT808Serializer.Deserialize(frame);

            Assert.Equal((ushort)JT808MsgId.PlatformGeneralResponse, pkg.Header.MsgId);
            var decoded = JT808_0x8001.Decode(pkg.Bodies);
            Assert.Equal(200, decoded.ReplyMsgSerialNumber);
            Assert.Equal((ushort)JT808MsgId.TerminalRegister, decoded.ReplyMsgId);
            Assert.Equal(JT808PlatformResult.Success, decoded.Result);
        }

        [Fact]
        public void Deserialize_WrongFlag_ThrowsArgumentException()
        {
            byte[] bad = { 0x00, 0x01, 0x02, 0x7E };
            Assert.Throws<ArgumentException>(() => JT808Serializer.Deserialize(bad));
        }

        [Fact]
        public void Deserialize_BadChecksum_ThrowsInvalidDataException()
        {
            var msg = new JT808_0x0002();
            byte[] frame = JT808PackageBuilder.Serialize(msg, "013800138000", 1);
            // Corrupt the checksum byte (second-to-last before end flag)
            frame[frame.Length - 2] ^= 0xFF;
            Assert.Throws<InvalidDataException>(() => JT808Serializer.Deserialize(frame));
        }
    }
}

using JT808;
using JT808.Enums;
using JT808.Extensions;
using JT808.Messages;
using JT808.Serializer;
using Xunit;

namespace JT808.Tests
{
    public class JT808MessageTests
    {
        public JT808MessageTests()
        {
            JT808Initializer.Initialize();
        }

        // ── 0x0001 Terminal General Response ────────────────────────────────
        [Fact]
        public void TerminalGeneralResponse_Encode_Decode_RoundTrip()
        {
            var msg = new JT808_0x0001
            {
                ReplyMsgSerialNumber = 42,
                ReplyMsgId = (ushort)JT808MsgId.LocationReport,
                Result = 0x00
            };
            var decoded = JT808_0x0001.Decode(msg.Encode());
            Assert.Equal(msg.ReplyMsgSerialNumber, decoded.ReplyMsgSerialNumber);
            Assert.Equal(msg.ReplyMsgId, decoded.ReplyMsgId);
            Assert.Equal(msg.Result, decoded.Result);
        }

        [Fact]
        public void TerminalGeneralResponse_BodyLength_Is5()
        {
            Assert.Equal(5, new JT808_0x0001().Encode().Length);
        }

        [Fact]
        public void TerminalGeneralResponse_Decode_TooShort_Throws()
        {
            Assert.Throws<ArgumentException>(() => JT808_0x0001.Decode(new byte[4]));
        }

        // ── 0x8001 Platform General Response ─────────────────────────────────
        [Fact]
        public void PlatformGeneralResponse_Encode_Decode_RoundTrip()
        {
            var msg = new JT808_0x8001
            {
                ReplyMsgSerialNumber = 99,
                ReplyMsgId = (ushort)JT808MsgId.TerminalAuth,
                Result = JT808PlatformResult.Failure
            };
            var decoded = JT808_0x8001.Decode(msg.Encode());
            Assert.Equal(msg.ReplyMsgSerialNumber, decoded.ReplyMsgSerialNumber);
            Assert.Equal(msg.ReplyMsgId, decoded.ReplyMsgId);
            Assert.Equal(msg.Result, decoded.Result);
        }

        // ── 0x0002 Terminal Heartbeat ─────────────────────────────────────────
        [Fact]
        public void Heartbeat_Encode_ReturnsEmptyBody()
        {
            Assert.Empty(new JT808_0x0002().Encode());
        }

        // ── 0x0003 Terminal Logout ────────────────────────────────────────────
        [Fact]
        public void Logout_Encode_ReturnsEmptyBody()
        {
            Assert.Empty(new JT808_0x0003().Encode());
        }

        // ── 0x8100 Terminal Registration Response ─────────────────────────────
        [Fact]
        public void TerminalRegisterResponse_Success_IncludesAuthCode()
        {
            var msg = new JT808_0x8100
            {
                ReplyMsgSerialNumber = 1,
                Result = JT808RegisterResult.Success,
                AuthCode = "ABC123"
            };
            byte[] encoded = msg.Encode();
            var decoded = JT808_0x8100.Decode(encoded);
            Assert.Equal(JT808RegisterResult.Success, decoded.Result);
            Assert.Equal("ABC123", decoded.AuthCode);
        }

        [Fact]
        public void TerminalRegisterResponse_Failure_NoAuthCode()
        {
            var msg = new JT808_0x8100
            {
                ReplyMsgSerialNumber = 2,
                Result = JT808RegisterResult.VehicleAlreadyRegistered
            };
            byte[] encoded = msg.Encode();
            Assert.Equal(3, encoded.Length);  // serial(2) + result(1), no auth code
            var decoded = JT808_0x8100.Decode(encoded);
            Assert.Equal(JT808RegisterResult.VehicleAlreadyRegistered, decoded.Result);
            Assert.Equal(string.Empty, decoded.AuthCode);
        }

        [Fact]
        public void TerminalRegisterResponse_Decode_TooShort_Throws()
        {
            Assert.Throws<ArgumentException>(() => JT808_0x8100.Decode(new byte[2]));
        }

        // ── 0x0102 Terminal Authentication ───────────────────────────────────
        [Fact]
        public void TerminalAuth_Encode_Decode_RoundTrip()
        {
            var msg = new JT808_0x0102 { AuthCode = "MyAuthCode" };
            var decoded = JT808_0x0102.Decode(msg.Encode());
            Assert.Equal(msg.AuthCode, decoded.AuthCode);
        }

        // ── BCD Extension ─────────────────────────────────────────────────────
        [Fact]
        public void BcdExtension_EncodeDecode_PhoneNumber()
        {
            string phone = "013800138000";
            byte[] encoded = JT808BcdExtension.EncodeBcd(phone, 6);
            string decoded = JT808BcdExtension.DecodeBcd(encoded);
            Assert.Equal(phone, decoded);
        }

        [Fact]
        public void BcdExtension_Encode_PadsLeft()
        {
            byte[] result = JT808BcdExtension.EncodeBcd("1234", 3);
            Assert.Equal(new byte[] { 0x00, 0x12, 0x34 }, result);
        }

        // ── Version-specific frame tests ──────────────────────────────────────
        [Fact]
        public void Authentication_2011_FullFrame_RoundTrip()
        {
            var msg = new JT808_0x0102 { AuthCode = "token2011" };
            byte[] frame = JT808PackageBuilder.Serialize(msg, "013800138000", 3, JT808Version.JTT2011);
            var pkg = JT808Serializer.Deserialize(frame);
            var decoded = JT808_0x0102.Decode(pkg.Bodies);

            Assert.Equal((ushort)JT808MsgId.TerminalAuth, pkg.Header.MsgId);
            Assert.Equal("token2011", decoded.AuthCode);
        }

        [Fact]
        public void Authentication_2013_FullFrame_RoundTrip()
        {
            var msg = new JT808_0x0102 { AuthCode = "token2013" };
            byte[] frame = JT808PackageBuilder.Serialize(msg, "013800138001", 4, JT808Version.JTT2013);
            var pkg = JT808Serializer.Deserialize(frame);
            var decoded = JT808_0x0102.Decode(pkg.Bodies);

            Assert.Equal((ushort)JT808MsgId.TerminalAuth, pkg.Header.MsgId);
            Assert.Equal("token2013", decoded.AuthCode);
        }

        [Fact]
        public void RegisterResponse_2013_FullFrame_RoundTrip()
        {
            var msg = new JT808_0x8100
            {
                ReplyMsgSerialNumber = 1,
                Result = JT808RegisterResult.Success,
                AuthCode = "xY9z"
            };
            byte[] frame = JT808PackageBuilder.Serialize(msg, "013800138001", 1, JT808Version.JTT2013);
            var pkg = JT808Serializer.Deserialize(frame);
            var decoded = JT808_0x8100.Decode(pkg.Bodies);

            Assert.Equal(JT808RegisterResult.Success, decoded.Result);
            Assert.Equal("xY9z", decoded.AuthCode);
        }
    }
}

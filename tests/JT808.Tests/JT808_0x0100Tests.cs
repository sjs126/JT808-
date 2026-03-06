using JT808;
using JT808.Enums;
using JT808.Messages;
using JT808.Serializer;
using Xunit;

namespace JT808.Tests
{
    public class JT808_0x0100Tests
    {
        public JT808_0x0100Tests()
        {
            JT808Initializer.Initialize();
        }

        [Fact]
        public void Encode_Decode_2011_RoundTrip()
        {
            var msg = new JT808_0x0100
            {
                ProvinceId = 31,
                CityId = 115,
                ManufacturerId = "GRALS",
                TerminalModel = "TM-2011",
                TerminalId = "DEV0001",
                LicensePlateColor = 1,
                LicensePlate = "沪A12345",
                Version = JT808Version.JTT2011
            };

            byte[] encoded = msg.Encode();
            var decoded = JT808_0x0100.Decode(encoded, JT808Version.JTT2011);

            Assert.Equal(msg.ProvinceId, decoded.ProvinceId);
            Assert.Equal(msg.CityId, decoded.CityId);
            Assert.Equal(msg.ManufacturerId, decoded.ManufacturerId);
            Assert.Equal(msg.TerminalModel, decoded.TerminalModel);
            Assert.Equal(msg.TerminalId, decoded.TerminalId);
            Assert.Equal(msg.LicensePlateColor, decoded.LicensePlateColor);
            Assert.Equal(msg.LicensePlate, decoded.LicensePlate);
        }

        [Fact]
        public void Encode_Decode_2013_RoundTrip()
        {
            var msg = new JT808_0x0100
            {
                ProvinceId = 11,
                CityId = 1,
                ManufacturerId = "ABCDE",
                TerminalModel = "ModelX-2013-v1",
                TerminalId = "T001234",
                LicensePlateColor = 2,
                LicensePlate = "京B67890",
                Version = JT808Version.JTT2013
            };

            byte[] encoded = msg.Encode();
            var decoded = JT808_0x0100.Decode(encoded, JT808Version.JTT2013);

            Assert.Equal(msg.ProvinceId, decoded.ProvinceId);
            Assert.Equal(msg.CityId, decoded.CityId);
            Assert.Equal(msg.ManufacturerId, decoded.ManufacturerId);
            Assert.Equal(msg.TerminalModel, decoded.TerminalModel);
            Assert.Equal(msg.TerminalId, decoded.TerminalId);
            Assert.Equal(msg.LicensePlateColor, decoded.LicensePlateColor);
            Assert.Equal(msg.LicensePlate, decoded.LicensePlate);
        }

        [Fact]
        public void Encode_2011_BodyLength_IsSmaller_Than_2013()
        {
            var msg2011 = new JT808_0x0100 { Version = JT808Version.JTT2011 };
            var msg2013 = new JT808_0x0100 { Version = JT808Version.JTT2013 };

            // 2011: terminal model is 8 bytes; 2013: 20 bytes (12 bytes difference)
            Assert.Equal(12, msg2013.Encode().Length - msg2011.Encode().Length);
        }

        [Fact]
        public void Decode_2011_TooShort_ThrowsArgumentException()
        {
            byte[] tooShort = new byte[5];
            Assert.Throws<ArgumentException>(() => JT808_0x0100.Decode(tooShort, JT808Version.JTT2011));
        }

        [Fact]
        public void Decode_2013_TooShort_ThrowsArgumentException()
        {
            byte[] tooShort = new byte[5];
            Assert.Throws<ArgumentException>(() => JT808_0x0100.Decode(tooShort, JT808Version.JTT2013));
        }

        [Fact]
        public void Serialize_Deserialize_Register_2011_FullFrame()
        {
            var msg = new JT808_0x0100
            {
                ProvinceId = 31,
                CityId = 115,
                ManufacturerId = "GRALS",
                TerminalModel = "TM11",
                TerminalId = "DEV0001",
                LicensePlateColor = 1,
                LicensePlate = "沪A12345",
                Version = JT808Version.JTT2011
            };

            byte[] frame = JT808PackageBuilder.Serialize(msg, "013800138000", 1, JT808Version.JTT2011);
            var pkg = JT808Serializer.Deserialize(frame);
            var decoded = JT808_0x0100.Decode(pkg.Bodies, JT808Version.JTT2011);

            Assert.Equal((ushort)JT808MsgId.TerminalRegister, pkg.Header.MsgId);
            Assert.Equal(31, decoded.ProvinceId);
            Assert.Equal("GRALS", decoded.ManufacturerId);
            Assert.Equal("沪A12345", decoded.LicensePlate);
        }

        [Fact]
        public void Serialize_Deserialize_Register_2013_FullFrame()
        {
            var msg = new JT808_0x0100
            {
                ProvinceId = 11,
                CityId = 1,
                ManufacturerId = "ABCDE",
                TerminalModel = "ModelX-2013-v1",
                TerminalId = "T001234",
                LicensePlateColor = 2,
                LicensePlate = "京B67890",
                Version = JT808Version.JTT2013
            };

            byte[] frame = JT808PackageBuilder.Serialize(msg, "013800138001", 2, JT808Version.JTT2013);
            var pkg = JT808Serializer.Deserialize(frame);
            var decoded = JT808_0x0100.Decode(pkg.Bodies, JT808Version.JTT2013);

            Assert.Equal((ushort)JT808MsgId.TerminalRegister, pkg.Header.MsgId);
            Assert.Equal(11, decoded.ProvinceId);
            Assert.Equal("ABCDE", decoded.ManufacturerId);
            Assert.Equal("ModelX-2013-v1", decoded.TerminalModel);
            Assert.Equal("京B67890", decoded.LicensePlate);
        }
    }
}

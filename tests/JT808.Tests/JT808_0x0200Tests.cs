using JT808;
using JT808.Enums;
using JT808.Messages;
using JT808.Serializer;
using Xunit;

namespace JT808.Tests
{
    public class JT808_0x0200Tests
    {
        public JT808_0x0200Tests()
        {
            JT808Initializer.Initialize();
        }

        [Fact]
        public void Encode_Decode_RoundTrip()
        {
            var msg = new JT808_0x0200
            {
                AlarmFlag = 0x00000000,
                Status = 0x00000002,
                Latitude = 31960000,    // ~31.96 degrees N
                Longitude = 118980000,  // ~118.98 degrees E
                Altitude = 50,
                Speed = 600,            // 60.0 km/h
                Direction = 90,
                Timestamp = "200101120000"  // 2020-01-01 12:00:00
            };

            byte[] encoded = msg.Encode();
            var decoded = JT808_0x0200.Decode(encoded);

            Assert.Equal(msg.AlarmFlag, decoded.AlarmFlag);
            Assert.Equal(msg.Status, decoded.Status);
            Assert.Equal(msg.Latitude, decoded.Latitude);
            Assert.Equal(msg.Longitude, decoded.Longitude);
            Assert.Equal(msg.Altitude, decoded.Altitude);
            Assert.Equal(msg.Speed, decoded.Speed);
            Assert.Equal(msg.Direction, decoded.Direction);
            Assert.Equal(msg.Timestamp, decoded.Timestamp);
        }

        [Fact]
        public void Encode_Returns28Bytes()
        {
            var msg = new JT808_0x0200();
            Assert.Equal(28, msg.Encode().Length);
        }

        [Fact]
        public void Decode_TooShort_ThrowsArgumentException()
        {
            byte[] tooShort = new byte[10];
            Assert.Throws<ArgumentException>(() => JT808_0x0200.Decode(tooShort));
        }

        [Fact]
        public void Serialize_Deserialize_Location_2011_FullFrame()
        {
            var msg = new JT808_0x0200
            {
                AlarmFlag = 0,
                Status = 0x00000002,
                Latitude = 31960000,
                Longitude = 118980000,
                Altitude = 100,
                Speed = 300,
                Direction = 180,
                Timestamp = "110101120000"
            };

            byte[] frame = JT808PackageBuilder.Serialize(msg, "013800138000", 10, JT808Version.JTT2011);
            var pkg = JT808Serializer.Deserialize(frame);
            var decoded = JT808_0x0200.Decode(pkg.Bodies);

            Assert.Equal((ushort)JT808MsgId.LocationReport, pkg.Header.MsgId);
            Assert.Equal("013800138000", pkg.Header.PhoneNumber);
            Assert.Equal(31960000u, decoded.Latitude);
            Assert.Equal(118980000u, decoded.Longitude);
            Assert.Equal(100, decoded.Altitude);
            Assert.Equal(300, decoded.Speed);
            Assert.Equal(180, decoded.Direction);
            Assert.Equal("110101120000", decoded.Timestamp);
        }

        [Fact]
        public void Serialize_Deserialize_Location_2013_FullFrame()
        {
            var msg = new JT808_0x0200
            {
                AlarmFlag = 0x00000001,
                Status = 0x00000004,
                Latitude = 22500000,
                Longitude = 114000000,
                Altitude = 20,
                Speed = 800,
                Direction = 270,
                Timestamp = "130606180000"
            };

            byte[] frame = JT808PackageBuilder.Serialize(msg, "013900138002", 20, JT808Version.JTT2013);
            var pkg = JT808Serializer.Deserialize(frame);
            var decoded = JT808_0x0200.Decode(pkg.Bodies);

            Assert.Equal((ushort)JT808MsgId.LocationReport, pkg.Header.MsgId);
            Assert.Equal(0x00000001u, decoded.AlarmFlag);
            Assert.Equal(22500000u, decoded.Latitude);
            Assert.Equal(114000000u, decoded.Longitude);
            Assert.Equal("130606180000", decoded.Timestamp);
        }

        [Fact]
        public void AlarmFlag_Encoding_Preserved()
        {
            var msg = new JT808_0x0200 { AlarmFlag = 0xDEADBEEF };
            var decoded = JT808_0x0200.Decode(msg.Encode());
            Assert.Equal(0xDEADBEEFu, decoded.AlarmFlag);
        }
    }
}

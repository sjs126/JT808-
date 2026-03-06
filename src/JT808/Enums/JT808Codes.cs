namespace JT808.Enums
{
    /// <summary>
    /// Message body property bit flags (消息体属性)
    /// </summary>
    public enum JT808BodyDataType : ushort
    {
        None = 0x0000,
        Rsa = 0x0400
    }

    /// <summary>
    /// Terminal registration result codes
    /// </summary>
    public enum JT808RegisterResult : byte
    {
        Success = 0x00,
        VehicleAlreadyRegistered = 0x01,
        NoVehicleInDB = 0x02,
        TerminalAlreadyRegistered = 0x03,
        NoTerminalInDB = 0x04
    }

    /// <summary>
    /// Platform general response result codes
    /// </summary>
    public enum JT808PlatformResult : byte
    {
        Success = 0x00,
        Failure = 0x01,
        MessageError = 0x02,
        Unsupported = 0x03,
        AlarmConfirm = 0x04
    }
}

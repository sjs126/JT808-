namespace JT808.Enums
{
    /// <summary>
    /// JT808 message ID definitions
    /// </summary>
    public enum JT808MsgId : ushort
    {
        // Terminal -> Platform
        /// <summary>Terminal general response (终端通用应答)</summary>
        TerminalGeneralResponse = 0x0001,

        /// <summary>Terminal heartbeat (终端心跳)</summary>
        TerminalHeartbeat = 0x0002,

        /// <summary>Terminal logout (终端注销)</summary>
        TerminalLogout = 0x0003,

        /// <summary>Terminal registration (终端注册)</summary>
        TerminalRegister = 0x0100,

        /// <summary>Terminal authentication (终端鉴权)</summary>
        TerminalAuth = 0x0102,

        /// <summary>Query terminal parameters response (查询终端参数应答)</summary>
        QueryTerminalParamResponse = 0x0104,

        /// <summary>Location information report (位置信息汇报)</summary>
        LocationReport = 0x0200,

        // Platform -> Terminal
        /// <summary>Platform general response (平台通用应答)</summary>
        PlatformGeneralResponse = 0x8001,

        /// <summary>Terminal registration response (终端注册应答)</summary>
        TerminalRegisterResponse = 0x8100,

        /// <summary>Set terminal parameters (设置终端参数)</summary>
        SetTerminalParam = 0x8103,

        /// <summary>Query terminal parameters (查询终端参数)</summary>
        QueryTerminalParam = 0x8104
    }
}

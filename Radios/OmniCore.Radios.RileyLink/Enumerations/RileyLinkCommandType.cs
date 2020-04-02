namespace OmniCore.Radios.RileyLink.Enumerations
{
    public enum RileyLinkCommandType
    {
        None = 0,
        GetState = 1,
        GetVersion = 2,
        GetPacket = 3,
        SendPacket = 4,
        SendAndListen = 5,
        UpdateRegister = 6,
        Reset = 7,
        Led = 8,
        ReadRegister = 9,
        SetModeRegisters = 10,
        SetSwEncoding = 11,
        SetPreamble = 12,
        ResetRadioConfig = 13,
        GetStatistics = 14
    }
}

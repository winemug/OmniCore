namespace OmniCore.Services.Interfaces.Radio;

public enum RileyLinkCommand
{
    Noop = 0,
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
    RadioResetConfig = 13
}
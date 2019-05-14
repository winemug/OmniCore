namespace OmniCore.Py
{
    public enum RadioPacketType
    {
        UN0 = 0b00000000,
        UN1 = 0b00100000,
        ACK = 0b01000000,
        UN3 = 0b01100000,
        CON = 0b10000000,
        PDM = 0b10100000,
        UN6 = 0b11000000,
        POD = 0b11100000
    }
}


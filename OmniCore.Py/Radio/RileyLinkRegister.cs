namespace OmniCore.Py
{
    public enum RileyLinkRegister : byte
    {
        SYNC1 = 0x00,
        SYNC0 = 0x01,
        PKTLEN = 0x02,
        PKTCTRL1 = 0x03,
        PKTCTRL0 = 0x04,
        FSCTRL1 = 0x07,
        FREQ2 = 0x09,
        FREQ1 = 0x0a,
        FREQ0 = 0x0b,
        MDMCFG4 = 0x0c,
        MDMCFG3 = 0x0d,
        MDMCFG2 = 0x0e,
        MDMCFG1 = 0x0f,
        MDMCFG0 = 0x10,
        DEVIATN = 0x11,
        MCSM0 = 0x14,
        FOCCFG = 0x15,
        AGCCTRL2 = 0x17,
        AGCCTRL1 = 0x18,
        AGCCTRL0 = 0x19,
        FREND1 = 0x1a,
        FREND0 = 0x1b,
        FSCAL3 = 0x1c,
        FSCAL2 = 0x1d,
        FSCAL1 = 0x1e,
        FSCAL0 = 0x1f,
        TEST1 = 0x24,
        TEST0 = 0x25,
        PATABLE0 = 0x2e
    }
}

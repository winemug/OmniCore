namespace OmniCore.Py
{
    public enum PdmRequest
    {
        SetupPod = 0x03,
        AssignAddress = 0x07,
        SetDeliveryFlags = 0x08,
        Status = 0x0e,
        AcknowledgeAlerts = 0x11,
        BasalSchedule = 0x13,
        TempBasalSchedule = 0x16,
        BolusSchedule = 0x17,
        ConfigureAlerts = 0x19,
        InsulinSchedule = 0x1a,
        DeactivatePod = 0x1c,
        CancelDelivery = 0x1f
    }
}


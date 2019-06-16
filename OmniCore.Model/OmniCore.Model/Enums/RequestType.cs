namespace OmniCore.Model.Enums
{
    public enum RequestType
    {
        AssignAddress,
        SetupPod,
        SetDeliveryFlags,
        PrimeCannula,
        InsertCannula,
        Status,
        AcknowledgeAlerts,
        ConfigureAlerts,
        SetBasalSchedule,
        CancelBasal,
        SetTempBasal,
        Bolus,
        CancelBolus,
        CancelTempBasal,
        DeactivatePod,
        StartExtendedBolus,
        StopExtendedBolus
    }
}

namespace OmniCore.Model.Enumerations
{
    public enum RequestType
    {
        Pair,
        Prime,
        Insert,
        Status,
        ConfigureAlerts,
        AcknowledgeAlerts,
        SetBasalSchedule,
        CancelBasal,
        SetTempBasal,
        CancelTempBasal,
        Bolus,
        CancelBolus,
        StartExtendedBolus,
        CancelExtendedBolus,
        Deactivate
    }
}
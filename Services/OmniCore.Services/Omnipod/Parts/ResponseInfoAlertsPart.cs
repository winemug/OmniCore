using OmniCore.Services.Interfaces;

namespace OmniCore.Services;

public class ResponseInfoAlertsPart : ResponseInfoPart
{
    public ushort[] Alerts = new ushort[8];

    public ResponseInfoAlertsPart(Bytes data)
    {
        Unknown0 = data.Word(1);
        Alerts[0] = data.Word(3);
        Alerts[1] = data.Word(5);
        Alerts[2] = data.Word(7);
        Alerts[3] = data.Word(9);
        Alerts[4] = data.Word(11);
        Alerts[5] = data.Word(13);
        Alerts[6] = data.Word(15);
        Alerts[7] = data.Word(17);
        Data = data;
    }

    public override RequestStatusType StatusType => RequestStatusType.Alerts;

    public ushort Unknown0 { get; set; }
}
// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Runtime.InteropServices;
using Moq;
using OmniCore.Services;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Pod;
using OmniCore.Tests;

// var bre = new BasalRateEntry
// {
//     HalfHourCount = 18,
//     PulsesPerHour = 600
// };
// var parts = new MessagePart[]
//     {
//         new RequestInsulinSchedulePart(bre),
//         new RequestTempBasalPart(bre)
//     };
//
// var message = ConstructMessage(false, parts);

var be = new BolusEntry
{
    ImmediatePulseCount = 2,
    ImmediatePulseInterval125ms = 2 * 8
};

var parts = new MessagePart[]
    {
        new RequestInsulinSchedulePart(be),
        new RequestBolusPart(be)
    };

var message = ConstructMessage(false, parts);

Console.WriteLine($"{message}");

return;

var dataService = new Mock<IDataService>().Object;
var pod = new Pod(dataService)
{
    ValidFrom = DateTimeOffset.Now,
    ValidTo = DateTimeOffset.Now + TimeSpan.FromHours(80),
    Medication = MedicationType.Insulin,
    RadioAddress = 0x55555555,
    UnitsPerMilliliter = 100,
    Progress = PodProgress.Running
};
var radioConnection = new MockRadioConnection(pod);

var podLock = await pod.LockAsync(CancellationToken.None);
using (var podConn = new PodConnection(pod, radioConnection, podLock, dataService))
{
    var result = await podConn.SetTempBasal(0, 8);
    Console.WriteLine($"{result}");
}

PodMessage ConstructMessage(bool critical, MessagePart[] parts)
{
    var msgParts = new List<IMessagePart>();
    foreach (var part in parts)
    {
        if (part.RequiresNonce)
            part.Nonce = 0x12345678;
        msgParts.Add(part);
    }

    return new PodMessage
    {
        Address = 0x34343434,
        Sequence = 0,
        WithCriticalFollowup = critical,
        Parts = msgParts
    };
}
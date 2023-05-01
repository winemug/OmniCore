// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using MessagePack;
using MessagePack.Resolvers;
using OmniCore.Common.Data;
using OmniCore.Common.Pod;
using OmniCore.Framework.Omnipod;
using OmniCore.Framework.Omnipod.Messages;
using OmniCore.Framework.Omnipod.Requests;
using OmniCore.Framework.Omnipod.Responses;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
//using Moq;
//using OmniCore.Common.Data;
//using OmniCore.Framework.Omnipod.Messages;
//using OmniCore.Services;
//using OmniCore.Services.Interfaces.Core;
//using OmniCore.Services.Interfaces.Pod;
//using OmniCore.Tests;
//using Plugin.BLE.Abstractions.Extensions;

using var fs = new FileStream("d:\\pa.json", FileMode.Open, FileAccess.Read);
var pas = await JsonSerializer.DeserializeAsync<SPodAction[]>(fs);

DateTimeOffset? tsMin = null;
DateTimeOffset? tsMax = null;
long totalTs = 0;
int countTs = 0;

foreach(var pa in pas)
{
    var sent = new object();
    var received = new object();
    if (pa.SentData != null)
        sent = new MessageBuilder().Build(new Bytes(pa.SentData)).Data;

    if (pa.ReceivedData != null)
        received = new MessageBuilder().Build(new Bytes(pa.ReceivedData)).Data;

    //var sSent = JsonSerializer.Serialize(sent);
    //var sReceived = JsonSerializer.Serialize(received);
    //Console.WriteLine($"\n{sent.GetType().Name} Idx: {pa.Index} {pa.RequestSentLatest} <-> {pa.RequestSentEarliest}");
    //Console.WriteLine($"Sent: {sSent}");
    //Console.WriteLine($"{received.GetType().Name}");
    //Console.WriteLine($"Received: {sReceived}");

    if (received is StatusMessage sm)
    {
        if (sm.ProgressModel.Faulted)
            continue;
        var ts = pa.RequestSentEarliest.Value;
        var mins = sm.StatusModel.ActiveMinutes;
        var tsp = ts - TimeSpan.FromMinutes(mins);
        if (!(tsMin < tsp))
            tsMin = tsp;
        if (!(tsMax > tsp))
            tsMax = tsp;

        totalTs += tsp.ToUnixTimeSeconds();
        countTs++;
    }
}
var meanTs = (totalTs / countTs) - 30;
var tsMean = DateTimeOffset.FromUnixTimeSeconds(meanTs);
Debug.WriteLine($"Min: {tsMin} Max: {tsMax} Mean: {tsMean}");


// {"MyProperty1":99,"MyProperty2":9999}

// var dataService = new Mock<IDataService>().Object;
// var pod = new Pod(dataService)
// {
//     ValidFrom = DateTimeOffset.Now,
//     ValidTo = DateTimeOffset.Now + TimeSpan.FromHours(80),
//     Medication = MedicationType.Insulin,
//     RadioAddress = 0x55555555,
//     UnitsPerMilliliter = 100,
//     Progress = PodProgress.Running
// };
// var radioConnection = new MockRadioConnection(pod);
//
// var podLock = await pod.LockAsync(CancellationToken.None);
// using (var podConn = new PodConnection(pod, radioConnection, podLock, dataService))
// {
//     var result = await podConn.SetTempBasal(0, 8);
//     Console.WriteLine($"{result}");
// }

// PodMessage ConstructMessage(bool critical, MessagePart[] parts)
// {
//     var msgParts = new List<IMessagePart>();
//     foreach (var part in parts)
//     {
//         if (part.RequiresNonce)
//             part.Nonce = 0x12345678;
//         msgParts.Add(part);
//     }
//
//     return new PodMessage
//     {
//         Address = 0x34343434,
//         Sequence = 0,
//         WithCriticalFollowup = critical,
//         Parts = msgParts
//     };
// }


class SPodAction
{
    public Guid PodId { get; set; }
    public int Index { get; set; }
    public Guid? ClientId { get; set; }
    public DateTimeOffset? RequestSentEarliest { get; set; }
    public DateTimeOffset? RequestSentLatest { get; set; }
    public byte[]? SentData { get; set; }
    public byte[]? ReceivedData { get; set; }
    public AcceptanceType Result { get; set; }
}

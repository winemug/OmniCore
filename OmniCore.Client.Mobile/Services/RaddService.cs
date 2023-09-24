// using System.Diagnostics;
// using System.Text.Json;
// using Microsoft.VisualStudio.Threading;
// using OmniCore.Common.Amqp;
// using OmniCore.Common.Core;
// using OmniCore.Common.Pod;
// using OmniCore.Framework.Omnipod;
//
// namespace OmniCore.Framework;
//
// public class RaddService : IRaddService
// {
//     private readonly IAmqpService _amqpService;
//     private readonly IPodService _podService;
//     private readonly IRadioService _radioService;
//
//     public RaddService(
//         IPodService podService,
//         IAmqpService amqpService,
//         IRadioService radioService)
//     {
//         _podService = podService;
//         _amqpService = amqpService;
//         _radioService = radioService;
//     }
//
//     protected async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         await stoppingToken.WaitHandle;
//     }
//
//     public async Task<bool> ProcessMessageAsync(AmqpMessage message)
//     {
//         var rr = JsonSerializer.Deserialize<RaddRequest>(message.Text);
//         if (rr == null)
//             return false;
//
//         if (string.IsNullOrEmpty(rr.PodId))
//         {
//             if (rr.TransferActiveSerial.HasValue && rr.TransferActiveLot.HasValue)
//             {
//                 uint? acquiredAddress = null;
//                 if (!rr.TransferActiveAddress.HasValue)
//                 {
//                     using var rc = await _radioService.GetIdealConnectionAsync();
//                     for (var k = 0; k < 3; k++)
//                     {
//                         for (var i = 0; i < 10; i++)
//                         {
//                             var bler = await rc.TryGetPacket(0, 1000);
//                             var packet = PodPacket.FromExchangeResult(bler);
//                             if (packet != null)
//                             {
//                                 Debug.WriteLine($"Packet: {packet}");
//                                 if (acquiredAddress.HasValue && acquiredAddress.Value != packet.Address) break;
//                                 acquiredAddress = packet.Address;
//                             }
//                         }
//
//                         if (acquiredAddress.HasValue)
//                             break;
//                     }
//
//                     if (!acquiredAddress.HasValue)
//                     {
//                         var msg = new AmqpMessage
//                         {
//                             Text = JsonSerializer.Serialize(
//                                 new
//                                 {
//                                     request_id = rr.RequestId,
//                                     success = false
//                                 })
//                         };
//                         _amqpService.PublishMessage(msg);
//                         return true;
//                     }
//
//                     rr.TransferActiveAddress = acquiredAddress;
//
//                     while (true)
//                     {
//                         var packet = await rc.TryGetPacket(0, 5000);
//                         if (packet == null)
//                             break;
//                     }
//                 }
//             }
//
//             var pods = await _podService.GetPodsAsync();
//             var podsmsg = new AmqpMessage
//             {
//                 Text = JsonSerializer.Serialize(
//                     new
//                     {
//                         request_id = rr.RequestId,
//                         pod_ids = pods.Select(p => p.Id.ToString("N")).ToList(),
//                         success = true
//                     })
//             };
//             _amqpService.PublishMessage(podsmsg);
//             return true;
//         }
//
//
//         var pod = await _podService.GetPodAsync(Guid.Parse(rr.PodId));
//         var success = pod != null;
//         if (success)
//             using (var podConnection = await _podService.GetConnectionAsync(pod))
//             {
//                 if (success && rr.NextRecordIndex != null && rr.NextRecordIndex != 0)
//                     success = pod.NextRecordIndex == rr.NextRecordIndex.Value;
//
//                 if (success && rr.UpdateStatus)
//                 {
//                     var response = await podConnection.UpdateStatus();
//                     success = response == PodRequestStatus.Executed;
//                 }
//
//                 if (success && rr.Beep)
//                 {
//                     var response = await podConnection.Beep(BeepType.BipBip);
//                     success = response == PodRequestStatus.Executed;
//                 }
//
//                 if (success && rr.CancelBolus)
//                 {
//                     var response = await podConnection.CancelBolus();
//                     success = response == PodRequestStatus.Executed;
//                 }
//
//                 if (success && rr.CancelTemp)
//                 {
//                     var response = await podConnection.CancelTempBasal();
//                     success = response == PodRequestStatus.Executed;
//                 }
//
//                 if (success && rr.TempBasalTicks.HasValue && rr.TempBasalHalfHours.HasValue)
//                 {
//                     var response =
//                         await podConnection.SetTempBasal(rr.TempBasalTicks.Value, rr.TempBasalHalfHours.Value);
//                     success = response == PodRequestStatus.Executed;
//                 }
//
//                 if (success && rr.BolusTicks is > 0 && !rr.TestBolus)
//                 {
//                     var response = await podConnection.Bolus((int)rr.BolusTicks, 2000);
//                     success = response == PodRequestStatus.Executed;
//                 }
//
//                 if (success && rr.Deactivate)
//                 {
//                     var response = await podConnection.Deactivate();
//                     success = response == PodRequestStatus.Executed;
//                 }
//
//                 if (success && rr.Remove)
//                 {
//                     success = false;
//                     try
//                     {
//                         await _podService.RemovePodAsync(Guid.Parse(rr.PodId));
//                         success = true;
//                     }
//                     catch (Exception e)
//                     {
//                         Console.WriteLine(e);
//                     }
//                 }
//
//                 if (success && rr.TestBolus && rr.BolusTicks is > 0)
//                     await podConnection.Bolus((int)rr.BolusTicks, 2000, true);
//             }
//
//         var resp = new RaddResponse
//         {
//             Success = success,
//             RequestId = rr.RequestId,
//             NextRecordIndex = pod?.NextRecordIndex,
//             Minutes = pod?.StatusModel?.ActiveMinutes,
//             Remaining = pod?.StatusModel?.PulsesRemaining,
//             Delivered = pod?.StatusModel?.PulsesDelivered
//         };
//         var respMessage = new AmqpMessage { Text = JsonSerializer.Serialize(resp) };
//         _amqpService.PublishMessage(respMessage);
//
//         return true;
//     }
// }
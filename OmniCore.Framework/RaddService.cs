using System.Diagnostics;
using System.Text.Json;
using OmniCore.Services.Interfaces.Amqp;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class RaddService : IRaddService
{
    private IPodService _podService;
    private IAmqpService _amqpService;
    private IRadioService _radioService;
    
    public RaddService(
        IPodService podService,
        IAmqpService amqpService,
        IRadioService radioService)
    {
        _podService = podService;
        _amqpService = amqpService;
        _radioService = radioService;
    }

    public async Task Start()
    {
        _amqpService.RegisterMessageProcessor(ProcessMessageAsync);
    }

    public async Task Stop()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ProcessMessageAsync(AmqpMessage message)
    {
        var rr = JsonSerializer.Deserialize<RaddRequest>(message.Text);
        if (rr == null)
            return false;

        if (string.IsNullOrEmpty(rr.pod_id))
        {
            if (rr.transfer_active_serial.HasValue && rr.transfer_active_lot.HasValue)
            {
                uint? acquired_address = null;
                if (!rr.transfer_active_address.HasValue)
                {
                    using var rc = await _radioService.GetIdealConnectionAsync();
                    for (int k = 0; k < 3; k++)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            var packet = await rc.TryGetPacket(0, 1000);
                            if (packet != null)
                            {
                                Debug.WriteLine($"Packet: {packet}");
                                if (acquired_address.HasValue && acquired_address.Value != packet.Address)
                                {
                                    break;
                                }
                                acquired_address = packet.Address;
                            }
                        }

                        if (acquired_address.HasValue)
                            break;
                    }

                    if (!acquired_address.HasValue)
                    {
                        var msg = new AmqpMessage
                        {
                            Text = JsonSerializer.Serialize(
                                new
                                {
                                    request_id = rr.request_id,
                                    success = false
                                })
                        };
                        await _amqpService.PublishMessage(msg);
                        return true;
                    }

                    rr.transfer_active_address = acquired_address;

                    while (true)
                    {
                        var packet = await rc.TryGetPacket(0, 5000);
                        if (packet == null)
                            break;
                    }
                }
            }
            var pods = await _podService.GetPodsAsync();
            var podsmsg = new AmqpMessage
            {
                Text = JsonSerializer.Serialize(
                    new
                    {
                        request_id = rr.request_id,
                        pod_ids = pods.Select(p => p.Id.ToString("N")).ToList(),
                        success = true
                    })
            };
            await _amqpService.PublishMessage(podsmsg);
            return true;
        }
        
        
        var pod = await _podService.GetPodAsync(Guid.Parse(rr.pod_id));
        var success = true;
        using (var podConnection = await _podService.GetConnectionAsync(pod))
        {
            if (rr.next_record_index != null)
            {
                success = pod.NextRecordIndex == rr.next_record_index.Value;
            }
            
            if (success && rr.update_status)
            {
                var response = await podConnection.UpdateStatus();
                success = response == PodRequestStatus.Executed;
            }

            if (success && rr.beep)
            {
                var response = await podConnection.Beep(BeepType.BipBip);
                success = response == PodRequestStatus.Executed;
            }

            if (success && rr.cancel_bolus)
            {
                var response = await podConnection.CancelBolus();
                success = response == PodRequestStatus.Executed;
            }
            
            if (success && rr.cancel_temp)
            {
                var response = await podConnection.CancelTempBasal();
                success = response == PodRequestStatus.Executed;
            }

            if (success && rr.temp_basal_ticks.HasValue && rr.temp_basal_half_hours.HasValue)
            {
                var response = await podConnection.SetTempBasal(rr.temp_basal_ticks.Value, rr.temp_basal_half_hours.Value);
                success = response == PodRequestStatus.Executed;
            }

            if (success && rr.bolus_ticks is > 0)
            {
                var response = await podConnection.Bolus((int)rr.bolus_ticks, 2);
                success = response == PodRequestStatus.Executed;
            }

            if (success && rr.deactivate)
            {
                var response = await podConnection.Deactivate();
                success = response == PodRequestStatus.Executed;
            }
        }

        var resp = new RaddResponse
        {
            success = success,
            request_id = rr.request_id,
            next_record_index=pod.NextRecordIndex,
            minutes = pod.ActiveMinutes,
            remaining = pod.PulsesRemaining,
            delivered = pod.PulsesDelivered
        };
        var respMessage = new AmqpMessage { Text = JsonSerializer.Serialize(resp), Id = message.Id };
        await _amqpService.PublishMessage(respMessage);

        return true;
        
    }
}

public class RaddRequest
{ 
    // public string? pod_id { get; set; }
    // public string? profile_id { get; set; }
    // public int? radio_address { get; set; }
    // public int? record_index { get; set; }
    // public int? units_per_ml { get; set; }
    // public int? medication { get; set; }
    // public int? valid_from { get; set; }
    // public int? valid_to { get; set; }
    public string? request_id { get; set; }
    public uint? transfer_active_serial { get; set; }
    public uint? transfer_active_lot { get; set; }
    public uint? transfer_active_address { get; set; }
    public string? pod_id { get; set; }
    public int? next_record_index { get; set; }
    public bool beep { get; set; }
    public bool update_status { get; set; }
    public int? bolus_ticks { get; set; }
    public int? temp_basal_ticks { get; set; }
    public int? temp_basal_half_hours { get; set; }
    public bool cancel_temp { get; set; }
    public bool cancel_bolus { get; set; }
    public bool deactivate { get; set; }
}

public class RaddResponse
{
    //public string? pod_id { get; set; }
    // public int? record_index { get; set; }
    public string? request_id { get; set; }
    public bool success { get; set; }
    public int next_record_index { get; set; }
    public int? remaining { get; set; }
    
    public int? delivered { get; set; }
    public int? minutes { get; set; }
}
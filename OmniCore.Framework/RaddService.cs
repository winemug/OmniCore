using System.Diagnostics;
using System.Text.Json;
using OmniCore.Services.Interfaces.Amqp;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Pod;
using OmniCore.Services.Interfaces.Radio;

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
    }

    public async Task<bool> ProcessMessageAsync(AmqpMessage message)
    {
        if (message.Type != null)
            return false;

        var rr = JsonSerializer.Deserialize<RaddRequest>(message.Text);
        if (rr == null)
            return false;

        if (string.IsNullOrEmpty(rr.pod_id) && !rr.create)
        {
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

        if (rr.remove)
        {
            await _podService.RemovePodAsync(Guid.Parse(rr.pod_id));
            await _amqpService.PublishMessage(new AmqpMessage { Text = JsonSerializer.Serialize(
             new RaddResponse
             {
                 success = true,
                 request_id = rr.request_id,
                 id = rr.pod_id,
             }
                ) });
            return true;
        }

        var requestPodId = Guid.Empty;
        if (rr.pod_id != null)
            requestPodId = Guid.Parse(rr.pod_id);

        if (rr.create)
        {
            requestPodId = await _podService.NewPodAsync(new Guid("7D799596-3F6D-48E2-AC65-33CA6396788B"), rr.create_units.Value, (MedicationType)rr.create_medication.Value);
        }

        var pod = await _podService.GetPodAsync(requestPodId);
        var success = pod != null;
        if (!success) return false;

        using (var podConnection = await _podService.GetConnectionAsync(pod))
        {
            if (success && rr.next_record_index != null && rr.next_record_index != 0)
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
                var interval = rr.bolus_interval ?? 2000;
                var response = await podConnection.Bolus((int)rr.bolus_ticks, interval);
                success = response == PodRequestStatus.Executed;
            }

            if (success && rr.deactivate)
            {
                var response = await podConnection.Deactivate();
                success = response == PodRequestStatus.Executed;
            }

            if (success && rr.prime)
            {
                var now = DateTime.Now;
                var response = await podConnection.PrimePodAsync(new DateOnly(now.Year, now.Month, now.Day),
                    new TimeOnly(now.Hour, now.Minute, now.Second),
                    true, CancellationToken.None);
                success = response == PodRequestStatus.Executed;
            }

            if (success && rr.start)
            {
                var now = DateTime.Now;
                var basalRateTicks = new int[48];
                for (int i = 0; i < 48; i++)
                    basalRateTicks[i] = rr.start_basal_ticks_per_hour;

                var response = await podConnection.StartPodAsync(
                    new TimeOnly(now.Hour, now.Minute, now.Second), basalRateTicks);
                success = response == PodRequestStatus.Executed;
            }
        }

        var resp = new RaddResponse
        {
            success = success,
            request_id = rr.request_id,
            id = pod?.Id.ToString("N"),
            next_record_index = pod?.NextRecordIndex,
            minutes = pod?.StatusModel?.ActiveMinutes,
            remaining = pod?.StatusModel?.PulsesRemaining,
            delivered = pod?.StatusModel?.PulsesDelivered,
            state = (int?)pod?.ProgressModel?.Progress,
            created = pod?.Created.ToUnixTimeSeconds(),
            activated = pod?.Activated?.ToUnixTimeSeconds(),
            last_connected = pod?.LastRadioPacketReceived?.ToUnixTimeSeconds(),
            medication = (int?)(pod?.Medication),
            units = pod?.UnitsPerMilliliter
        };
        var respMessage = new AmqpMessage { Text = JsonSerializer.Serialize(resp) };
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
    public string? pod_id { get; set; }
    public int? next_record_index { get; set; }
    public bool beep { get; set; }
    public bool update_status { get; set; }
    public int? bolus_ticks { get; set; }
    public int? bolus_interval { get; set; }
    public int? temp_basal_ticks { get; set; }
    public int? temp_basal_half_hours { get; set; }
    public bool cancel_temp { get; set; }
    public bool cancel_bolus { get; set; }
    public bool deactivate { get; set; }
    public bool remove { get; set; }
    public bool create { get; set; }
    public int? create_units { get; set; }
    public int? create_medication { get; set; }
    public bool prime { get; set; }
    public bool start { get; set; }
    public int start_basal_ticks_per_hour { get; set; }
}

public class RaddResponse
{
    //public string? pod_id { get; set; }
    // public int? record_index { get; set; }
    public string? request_id { get; set; }
    public string? id { get; set; }
    public bool success { get; set; }
    public int? next_record_index { get; set; }
    public int? remaining { get; set; }
    
    public int? delivered { get; set; }
    public int? minutes { get; set; }
    public int? state { get; set; }
    public long? created { get; set; }
    public long? activated { get; set; }
    public long? last_connected { get; set; }
    public int? medication { get; set; }
    public int? units { get; set; }
}
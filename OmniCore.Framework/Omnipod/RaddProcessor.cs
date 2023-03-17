using System.Text.Json;
using OmniCore.Services.Interfaces.Amqp;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class RaddProcessor
{
    private IDataService _dataService;
    private IPodService _podService;
    
    public RaddProcessor(IDataService dataService,
        IPodService podService)
    {
        _dataService = dataService;
        _podService = podService;
    }

    public async Task<AmqpMessage> ProcessMessageAsync(AmqpMessage message)
    {
        var rr = JsonSerializer.Deserialize<RaddRequest>(message.Text);
        var pod = await _podService.GetPodAsync();
        var success = true;
        using (var podConnection = await _podService.GetConnectionAsync(pod))
        {
            if (rr.check)
            {
                var response = await podConnection.UpdateStatus();
                success = response == PodResponse.OK;
            }

            if (success && rr.beep)
            {
                var response = await podConnection.Beep(BeepType.BipBip);
                success = response == PodResponse.OK;
            }

            if (success && rr.cancel_bolus)
            {
                var response = await podConnection.CancelBolus();
                success = response == PodResponse.OK;
            }
            
            if (success && rr.cancel_temp)
            {
                var response = await podConnection.CancelTempBasal();
                success = response == PodResponse.OK;
            }

            if (success && rr.zero_temp)
            {
                var response = await podConnection.SetTempBasal(0, 4);
                success = response == PodResponse.OK;
            }

            if (success && rr.bolus_ticks.HasValue && rr.bolus_ticks > 0)
            {
                var response = await podConnection.Bolus((int)rr.bolus_ticks, 2);
                success = response == PodResponse.OK;
            }

            if (success && rr.deactivate)
            {
                var response = await podConnection.Deactivate();
                success = response == PodResponse.OK;
            }
        }

        var resp = new RaddResponse() { success = success, request_id = rr.request_id};
        return new AmqpMessage { Text = JsonSerializer.Serialize(resp), Id = message.Id };
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
    public string request_id { get; set; }
    
    public bool ping { get; set; }
    public bool beep { get; set; }
    public bool check { get; set; }
    public int? bolus_ticks { get; set; }
    public bool zero_temp { get; set; }
    public bool cancel_temp { get; set; }
    public bool cancel_bolus { get; set; }
    public bool deactivate { get; set; }
}

public class RaddResponse
{
    //public string? pod_id { get; set; }
    // public int? record_index { get; set; }
    public string request_id { get; set; }
    public bool success { get; set; }
}


using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Pod;


namespace OmniCore.Services;

public class PodService : IPodService
{
    private IRadioService _radioService;
    private IDataService _dataService;
    private IConfigurationStore _configurationStore;
    private IAmqpService _amqpService;
    public async Task Start()
    {
    }

    public async Task Stop()
    {
    }

    public PodService(IDataService dataService, IRadioService radioService,
        IConfigurationStore configurationStore)
    {
        _dataService = dataService;
        _radioService = radioService;
        _configurationStore = configurationStore;
    }

    private IPod _onePod;
    public async Task<IPod> GetPodAsync()
    {
        if (_onePod == null)
        {
            using (var conn = await _dataService.GetConnectionAsync())
            {
                var row = await conn.QueryFirstOrDefaultAsync("SELECT * FROM pod WHERE ID=@pod_id",
                    new
                    {
                        pod_id = Guid.Parse("5bce07ad-89a2-4369-a8a0-b02f07fcec58").ToString("N")
                    });
                var prr = new PodRuntimeInformation()
                {
                    Lot = 72402,
                    Serial = 3200578,
                    NextMessageSequence = 0,
                    NextPacketSequence = 0,
                    NextRecordIndex = 0,
                    Progress = PodProgress.Running,
                };

                if (row == null)
                {
                    _onePod = new Pod(_dataService)
                    {
                        RadioAddress = 873437826,
                        UnitsPerMilliliter = 200,
                        Medication = MedicationType.Insulin,
                        ValidFrom = DateTimeOffset.Now,
                        ValidTo = DateTimeOffset.Now + TimeSpan.FromHours(80),
                        Info = prr
                    };
                    _onePod.Id = Guid.Parse("5bce07ad-89a2-4369-a8a0-b02f07fcec58");

                    var cc = await _configurationStore.GetConfigurationAsync();
                    await conn.ExecuteAsync(
                        "INSERT INTO pod(id, profile_id, client_id, radio_address, units_per_ml, medication, valid_from, valid_to)" +
                        " VALUES (@id, @profile_id, @client_id, @ra, @upml, @med, @vf, @vt)",
                        new
                        {
                            id = _onePod.Id.ToString("N"),
                            profile_id = "0",
                            client_id = cc.ClientId.Value.ToString("N"),
                            ra = _onePod.RadioAddress,
                            upml = _onePod.UnitsPerMilliliter,
                            med = (int)_onePod.Medication,
                            vf = _onePod.ValidFrom.ToUnixTimeMilliseconds(),
                            vt = _onePod.ValidTo?.ToUnixTimeMilliseconds(),
                        });
                }
                else
                {
                    _onePod = new Pod(_dataService)
                    {
                        Id = Guid.Parse(row.id),
                        RadioAddress = (uint)row.radio_address,
                        UnitsPerMilliliter = (int)row.units_per_ml,
                        Medication = (MedicationType)row.medication,
                        ValidFrom = DateTimeOffset.FromUnixTimeMilliseconds(row.valid_from),
                        ValidTo = DateTimeOffset.FromUnixTimeMilliseconds(row.valid_to),
                    };

                    await _onePod.LoadResponses();
                }
            }
        }
        return _onePod;
    }

    public async Task<IPodConnection> GetConnectionAsync(
        IPod pod,
        CancellationToken cancellationToken = default)
    {
        var radioConnection = await _radioService.GetIdealConnectionAsync(cancellationToken);
        if (radioConnection == null)
            throw new ApplicationException("No radios available");

        var podAllocationLockDisposable = await pod.LockAsync(cancellationToken);
        return new PodConnection(pod, radioConnection, podAllocationLockDisposable, _dataService);
    }
}
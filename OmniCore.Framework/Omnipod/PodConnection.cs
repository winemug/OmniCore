using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using OmniCore.Common.Data;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Amqp;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using OmniCore.Services.Interfaces.Radio;

namespace OmniCore.Services;

public class PodConnection : IDisposable, IPodConnection
{
    private bool _communicationNeedsClosing;
    private readonly PodModel _podModel;
    private readonly IDisposable _podLockDisposable;
    private readonly IRadioConnection _radioConnection;
    private readonly IConfigurationStore _configurationStore;
    private readonly ISyncService _syncService;

    public PodConnection(
        PodModel podModel,
        IRadioConnection radioConnection,
        IDisposable podLockDisposable,
        IConfigurationStore configurationStore,
        ISyncService syncService)
    {
        _podModel = podModel;
        _radioConnection = radioConnection;
        _podLockDisposable = podLockDisposable;
        _configurationStore = configurationStore;
        _syncService = syncService;
    }

    public void Dispose()
    {
        if (_communicationNeedsClosing)
        {
            Task.Run(async () =>
            {
                try
                {
                    await AckExchangeAsync();
                }
                finally
                {
                    _radioConnection.Dispose();
                    _podLockDisposable.Dispose();
                }
            });
        }
        else
        {
            _radioConnection.Dispose();
            _podLockDisposable.Dispose();
        }
    }

    public async Task<PodRequestStatus> PrimePodAsync(
        DateOnly podDate,
        TimeOnly podTime,
        bool relaxDeliveryCrosschecks,
        CancellationToken cancellationToken)
    {
        if (_podModel.Progress > PodProgress.Priming)
            return PodRequestStatus.NotAllowed;

        PodRequestStatus result = PodRequestStatus.Executed;
        if (_podModel.Progress is null or < PodProgress.Paired)
        {
            result = await SetRadioAddress(cancellationToken);
            if (result != PodRequestStatus.Executed)
                return result;
        }

        if (!_podModel.Progress.HasValue)
            return PodRequestStatus.Error;
        
        if (_podModel.Progress == PodProgress.Paired)
        {
            result = await SetupPod(podDate, podTime, 4, cancellationToken);
            if (result != PodRequestStatus.Executed)
                return result;
        }

        if (_podModel.Progress == PodProgress.Paired)
        {
            result = await ConfigureAlerts(new[]
            {
                new AlertConfiguration
                {
                    AlertIndex = 7,
                    SetActive = true,
                    AlertAfter = 5,
                    AlertDurationMinutes = 55,
                    BeepType = BeepType.Beep4x,
                    BeepPattern = BeepPattern.OnceEveryFifteenMinutes
                }
            }, cancellationToken);
            if (result != PodRequestStatus.Executed)
                return result;
        }

        if (relaxDeliveryCrosschecks && _podModel.Progress == PodProgress.Paired)
        {
            result = await SetDeliveryFlags(0, 0, cancellationToken);
            if (result != PodRequestStatus.Executed)
                return result;
        }
        
        if (_podModel.Progress == PodProgress.Paired)
        {
            result = await Bolus(52, 1, cancellationToken);
            if (result != PodRequestStatus.Executed)
                return result;
            if (!_podModel.ImmediateBolusActive.HasValue ||
                !_podModel.Faulted.HasValue)
                return PodRequestStatus.Error;
            
            if (!_podModel.ImmediateBolusActive.Value || _podModel.Progress != PodProgress.Priming
                || _podModel.Faulted.Value)
                return PodRequestStatus.Error;
            
            await Task.Delay(TimeSpan.FromSeconds(52));
            result = await UpdateStatus(cancellationToken);
            if (result != PodRequestStatus.Executed)
                return result;
        }

        if (!_podModel.PulsesPending.HasValue)
            return PodRequestStatus.Error;

        if (_podModel.Progress == PodProgress.Priming)
        {
            await Task.Delay(TimeSpan.FromSeconds(_podModel.PulsesPending.Value + 2));
            result = await UpdateStatus(cancellationToken);
            if (result != PodRequestStatus.Executed)
                return result;
            if (_podModel.Progress != PodProgress.Primed)
                return PodRequestStatus.Error;
        }

        if (_podModel.Progress != PodProgress.Primed)
            return PodRequestStatus.Error;

        return PodRequestStatus.Executed;
    }

    public async Task<PodRequestStatus> StartPodAsync(
        TimeOnly podTime,
        BasalRateEntry[] basalRateEntries,
        CancellationToken cancellationToken = default)
    {
        if (!_podModel.Progress.HasValue || !_podModel.Faulted.HasValue
            || _podModel.Faulted.Value)
            return PodRequestStatus.NotAllowed;
        if (_podModel.Progress < PodProgress.Primed)
            return PodRequestStatus.NotAllowed;
        if (_podModel.Progress >= PodProgress.Running)
            return PodRequestStatus.NotAllowed;

        PodRequestStatus result;
        if (_podModel.Progress == PodProgress.Primed)
        {
            result = await SetBasalSchedule(podTime, basalRateEntries, cancellationToken);
            if (result != PodRequestStatus.Executed)
                return result;
            if (_podModel.Progress != PodProgress.BasalSet)
                return PodRequestStatus.Error;
        }

        if (_podModel.Progress == PodProgress.BasalSet)
        {
            result = await ConfigureAlerts(new[]
            {
                new AlertConfiguration
                {
                    AlertIndex = 7,
                    SetActive = false,
                    AlertAfter = 0,
                    AlertDurationMinutes = 0,
                    BeepType = BeepType.NoSound,
                    BeepPattern = BeepPattern.Once
                },
                new AlertConfiguration
                {
                    AlertIndex = 0,
                    SetActive = false,
                    SetAutoOff = true,
                    AlertAfter = 0,
                    AlertDurationMinutes = 15,
                    BeepType = BeepType.BipBeep4x,
                    BeepPattern = BeepPattern.OnceEveryMinuteForFifteenMinutes
                }
            }, cancellationToken);
            if (result != PodRequestStatus.Executed)
                return result;
            if (_podModel.Faulted.Value)
                return PodRequestStatus.Error;
            
            result = await Bolus(10, 1, cancellationToken);
            if (result != PodRequestStatus.Executed)
                return result;
            if (_podModel.Faulted.Value)
                return PodRequestStatus.Error;

            if (!_podModel.ImmediateBolusActive.HasValue ||
                !_podModel.ImmediateBolusActive.Value ||
                _podModel.Progress != PodProgress.Inserting ||
                _podModel.Faulted.Value)
                return PodRequestStatus.Error;
            
            await Task.Delay(TimeSpan.FromSeconds(10));
            
            result = await UpdateStatus(cancellationToken);
            if (result != PodRequestStatus.Executed)
                return result;
        }
        
        if (_podModel.Progress == PodProgress.Inserting)
        {
            await Task.Delay(TimeSpan.FromSeconds(_podModel.PulsesPending + 2));
            result = await UpdateStatus(cancellationToken);
            if (result != PodRequestStatus.Executed)
                return result;
            if (_podModel.Progress != PodProgress.Running)
                return PodRequestStatus.Error;
        }
        
        return PodRequestStatus.Executed;
    }

    private async Task<PodRequestStatus> SetRadioAddress(CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress >= PodProgress.Paired)
            return PodRequestStatus.NotAllowed;
        
        return await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestAssignAddressPart(_podModel.RadioAddress)
            }
        );
    }

    private async Task<PodRequestStatus> SetupPod(DateOnly podDate, TimeOnly podTime,
        int packetTimeout = 0, CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress != PodProgress.Paired)
            return PodRequestStatus.NotAllowed;

        if (!_podModel.Lot.HasValue || !_podModel.Serial.HasValue)
            return PodRequestStatus.NotAllowed;
        
        return await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestSetupPodPart(_podModel.RadioAddress,
                    _podModel.Lot.Value, _podModel.Serial.Value, packetTimeout,
                    podDate.Year, podDate.Month, podDate.Day, podTime.Hour, podTime.Minute)
            }
        );
    }

    private async Task<PodRequestStatus> SetDeliveryFlags(byte b16, byte b17, CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress != PodProgress.Paired)
            return PodRequestStatus.NotAllowed;
        
        return await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestSetDeliveryFlagsPart(b16, b17)
            }
        );
    }

    public async Task<PodRequestStatus> SetBasalSchedule(
        TimeOnly podTime,
        BasalRateEntry[] basalRateEntries,
        CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress < PodProgress.Primed || _podModel.Progress > PodProgress.RunningLow)
            return PodRequestStatus.NotAllowed;

        PodRequestStatus result;
        result = await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            }
        );
        if (result != PodRequestStatus.Executed)
            return result;

        if (_podModel.BasalActive || _podModel.TempBasalActive || _podModel.ImmediateBolusActive || _podModel.ExtendedBolusActive)
        {
            result = await SendRequestAsync(false, cancellationToken,
                new MessagePart[]
                {
                    new RequestCancelPart(BeepType.NoSound,
                        _podModel.ExtendedBolusActive.Value,
                        _podModel.ImmediateBolusActive.Value,
                        _podModel.TempBasalActive.Value,
                        _podModel.BasalActive.Value)
                });
            if (result != PodRequestStatus.Executed)
                return result;
            if (_podModel.BasalActive)
                return PodRequestStatus.Error;
        }
        
        return await SendRequestAsync(false,
            cancellationToken,
            new MessagePart[]
            {
                new RequestInsulinSchedulePart(basalRateEntries, podTime),
                new RequestBasalPart(basalRateEntries, podTime)
            }
        );
    }

    public async Task<PodRequestStatus> AcknowledgeAlerts(CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress < PodProgress.Paired || _podModel.Progress >= PodProgress.Inactive)
            return PodRequestStatus.NotAllowed;

        return await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestAcknowledgeAlertsPart()
            }
        );
    }
    
    public async Task<PodRequestStatus> UpdateStatus(CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress < PodProgress.Paired || _podModel.Progress >= PodProgress.Inactive)
            return PodRequestStatus.NotAllowed;

        return await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            }
        );
    }

    public async Task<PodRequestStatus> ConfigureAlerts(
        AlertConfiguration[] alertConfigurations,
        CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress < PodProgress.Paired || _podModel.Progress >= PodProgress.Faulted)
            return PodRequestStatus.NotAllowed;

        var result = await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            }
        );
        if (result != PodRequestStatus.Executed)
            return result;

        if (_podModel.Progress < PodProgress.Paired || _podModel.Progress >= PodProgress.Faulted)
            return PodRequestStatus.NotAllowed;

        result = await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestConfigureAlertsPart(alertConfigurations)
            }
        );

        return result;
    }

    public async Task<PodRequestStatus> Beep(BeepType type, CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress < PodProgress.Paired || _podModel.Progress >= PodProgress.Faulted)
            return PodRequestStatus.NotAllowed;

        var result = await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            }
        );
        if (result != PodRequestStatus.Executed)
            return result;

        if (_podModel.Progress < PodProgress.Paired || _podModel.Progress >= PodProgress.Faulted)
            return PodRequestStatus.NotAllowed;

        result = await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestBeepConfigPart(type,
                    false, false, 0,
                    false, false, 0,
                    false, false, 0)
            }
        );

        return result;
    }

    public async Task<PodRequestStatus> CancelTempBasal(CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress < PodProgress.Running || _podModel.Progress >= PodProgress.Faulted)
            return PodRequestStatus.NotAllowed;

        var result = await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            });

        if (result != PodRequestStatus.Executed)
            return result;

        if (_podModel.Progress < PodProgress.Running || _podModel.Progress >= PodProgress.Faulted)
            return PodRequestStatus.NotAllowed;

        var cancelBolus = (_podModel.ImmediateBolusActive || _podModel.ExtendedBolusActive);
        var cancelTempBasal = _podModel.TempBasalActive;

        return await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestCancelPart(BeepType.NoSound,
                    cancelBolus,
                    cancelTempBasal,
                    false)
            });
    }

    public async Task<PodRequestStatus> SetTempBasal(
        int pulsesPerHour,
        int halfHourCount,
        CancellationToken cancellationToken = default)
    {
        //var pulsesPerHour = hourlyRateMilliunits / (_pod.UnitsPerMilliliter / 2);

        if (halfHourCount < 0 || halfHourCount > 12 || pulsesPerHour < 0 || pulsesPerHour > 1800)
            return PodRequestStatus.NotAllowed;

        if (_podModel.Progress < PodProgress.Running || _podModel.Progress >= PodProgress.Faulted)
            return PodRequestStatus.NotAllowed;

        var result = await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            });
        
        if (result != PodRequestStatus.Executed)
            return result;

        if (_podModel.Progress < PodProgress.Running || _podModel.Progress >= PodProgress.Faulted)
            return PodRequestStatus.NotAllowed;
        
        var cancelBolus = (_podModel.ImmediateBolusActive || _podModel.ExtendedBolusActive);
        var cancelTempBasal = _podModel.TempBasalActive;

        if (cancelBolus || cancelTempBasal)
        {
            result = await SendRequestAsync(false, cancellationToken,
                new MessagePart[]
                {
                    new RequestCancelPart(BeepType.NoSound,
                        cancelBolus,
                        cancelTempBasal,
                        false)
                });
            if (result != PodRequestStatus.Executed)
                return result;
        }

        var bre = new BasalRateEntry
        {
            HalfHourCount = halfHourCount,
            PulsesPerHour = pulsesPerHour
        };
        return await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestInsulinSchedulePart(bre),
                new RequestTempBasalPart(bre)
            });
    }

    public async Task<PodRequestStatus> Bolus(
        int bolusPulses,
        int pulseIntervalSeconds,
        CancellationToken cancellationToken = default)
    {
        //var bolusPulses = bolusMilliunits / (_pod.UnitsPerMilliliter / 2);

        if (pulseIntervalSeconds < 2 || bolusPulses < 0 || bolusPulses > 1800 / pulseIntervalSeconds)
            return PodRequestStatus.NotAllowed;

        if (_podModel.Progress < PodProgress.Running || _podModel.Progress >= PodProgress.Faulted)
            return PodRequestStatus.NotAllowed;

        var result = await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            });

        if (result != PodRequestStatus.Executed)
            return result;

        if (_podModel.ImmediateBolusActive || _podModel.ExtendedBolusActive)
        {
            result = await SendRequestAsync(false, cancellationToken,
                new MessagePart[]
                {
                    new RequestCancelPart(BeepType.NoSound,
                        true,
                        false,
                        false)
                });
            if (result != PodRequestStatus.Executed)
                return result;
        }
            
        if (_podModel.Progress < PodProgress.Running || _podModel.Progress >= PodProgress.Faulted)
            return PodRequestStatus.NotAllowed;

        var be = new BolusEntry
        {
            ImmediatePulseCount = bolusPulses,
            ImmediatePulseInterval125ms = pulseIntervalSeconds * 8
        };
        return await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestInsulinSchedulePart(be),
                new RequestBolusPart(be)
            });
    }

    public async Task<PodRequestStatus> CancelBolus(CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress < PodProgress.Running || _podModel.Progress >= PodProgress.Faulted)
            return PodRequestStatus.NotAllowed;

        var result = await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            });

        if (result != PodRequestStatus.Executed)
            return result;

        if (!_podModel.ImmediateBolusActive && !_podModel.ExtendedBolusActive)
            return PodRequestStatus.NotAllowed;

        if (_podModel.Progress < PodProgress.Running || _podModel.Progress >= PodProgress.Faulted)
            return PodRequestStatus.NotAllowed;

        return await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestCancelPart(BeepType.NoSound,
                    true,
                    false,
                    false)
            });
    }


    public async Task<PodRequestStatus> Deactivate(CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress < PodProgress.Paired || _podModel.Progress >= PodProgress.Inactive)
            return PodRequestStatus.NotAllowed;

        var result = await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            }
        );
        if (result != PodRequestStatus.Executed)
            return result;

        if (_podModel.Progress < PodProgress.Paired || _podModel.Progress >= PodProgress.Inactive)
            return PodRequestStatus.NotAllowed;

        if (_podModel.Progress == PodProgress.Running || _podModel.Progress == PodProgress.RunningLow)
        {
            result = await SendRequestAsync(false, cancellationToken,
                new MessagePart[]
                {
                    new RequestCancelPart(BeepType.NoSound,
                        true,
                        true,
                        true)
                });
            if (result != PodRequestStatus.Executed)
                return result;
        }

        return await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestDeactivatePodPart()
            });
    }

    private async Task<PodRequestStatus> SendRequestAsync(
        bool critical,
        CancellationToken cancellationToken,
        MessagePart[] parts,
        int authRetries = 0,
        int syncRetries = 0)
    {
        var cc = await _configurationStore.GetConfigurationAsync();
        var clientId = cc.ClientId.Value;
        
        var initialPacketSequence = _podModel.NextPacketSequence;
        var initialMessageSequence = _podModel.NextMessageSequence;

        var messageToSend = ConstructMessage(critical, parts);

        var sendStart = DateTimeOffset.UtcNow;
        var er = await RunExchangeAsync(
            messageToSend,
            cancellationToken);
        var receiveEnd = DateTimeOffset.UtcNow;

        using (var ocdb = new OcdbContext())
        {
            await ocdb.PodActions.AddAsync(new PodAction
            {
                PodId = _podModel.Id,
                Index = _podModel.NextRecordIndex,
                ClientId = clientId,
                ReceivedData = er.ReceivedMessage?.Body?.ToArray(),
                SentData = er.SentMessage?.Body?.ToArray(),
                Result = er.Result,
                RequestSentEarliest = er.RequestSentEarliest,
                RequestSentLatest = er.RequestSentLatest,
                IsSynced = false
            });
            await ocdb.SaveChangesAsync(cancellationToken);
        }
        
        await _syncService.SyncPodMessage(_podModel.Id, _podModel.NextRecordIndex);
        _podModel.NextRecordIndex++;

        switch (er.Result)
        {
            case AcceptanceType.Accepted:
                _communicationNeedsClosing = true;
                return PodRequestStatus.Executed;

            case AcceptanceType.RejectedResyncRequired:
                if (syncRetries == 0)
                {
                    _podModel.NextMessageSequence = (initialMessageSequence + 2) % 16;
                    syncRetries++;
                    break;
                }
                _communicationNeedsClosing = false;
                return PodRequestStatus.RejectedByPod;
            
            case AcceptanceType.RejectedNonceReseed:
                if (er.ReceivedMessage?.Parts[0] is ResponseErrorPart rep
                    && authRetries < 2)
                {
                    _podModel.NextMessageSequence = initialMessageSequence;
                    _podModel.SyncNonce(rep.ErrorValue, initialMessageSequence);
                    authRetries++;
                    break;
                }
                _communicationNeedsClosing = true;
                return PodRequestStatus.RejectedByPod;
            
            case AcceptanceType.Inconclusive:
                _podModel.NextMessageSequence = (initialMessageSequence + 2) % 16;
                _podModel.NextPacketSequence = 0;
                _communicationNeedsClosing = false;
                return PodRequestStatus.Inconclusive;
            
            case AcceptanceType.Ignored:
                _podModel.NextMessageSequence = (initialMessageSequence + 2) % 16;
                _podModel.NextPacketSequence = 0;
                _communicationNeedsClosing = false;
                return PodRequestStatus.NotSubmitted;
            
            case AcceptanceType.RejectedErrorOccured:
            case AcceptanceType.RejectedFaultOccured:
                _communicationNeedsClosing = true;
                return PodRequestStatus.RejectedByPod;

            case AcceptanceType.RejectedProtocolError:
                _podModel.NextMessageSequence = (initialMessageSequence + 2) % 16;
                _podModel.NextPacketSequence = 0;
                _communicationNeedsClosing = false;
                return PodRequestStatus.RejectedByPod;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // retry
        return await SendRequestAsync(
            critical,
            cancellationToken,
            parts,
            authRetries,
            syncRetries
        );
    }

    private PodMessage ConstructMessage(bool critical, MessagePart[] parts)
    {
        var msgParts = new List<IMessagePart>();
        foreach (var part in parts)
        {
            if (part.RequiresNonce)
                part.Nonce = _podModel.NextNonce();
            msgParts.Add(part);
        }

        return new PodMessage
        {
            Address = _podModel.RadioAddress,
            Sequence = _podModel.NextMessageSequence,
            WithCriticalFollowup = critical,
            Parts = msgParts
        };
    }

    private async Task<ExchangeResult> RunExchangeAsync(
        IPodMessage messageToSend,
        CancellationToken cancellationToken = default
    )
    {
        var messageBody = messageToSend.GetBody();
        var sendPacketCount = messageBody.Length / 31 + 1;
        var sendPacketIndex = 0;

        var nextPacketSequence = _podModel.NextPacketSequence;
        var nextMessageSequence = _podModel.NextMessageSequence;
        DateTimeOffset? firstPacketSent = null;
        DateTimeOffset? lastPacketReceived = null;
        DateTimeOffset? lastPacketSent = null;

        var packetAddressIn = _podModel.Progress < PodProgress.Paired ? 0xFFFFFFFF : _podModel.RadioAddress;
        var packetAddressOut = _podModel.Progress < PodProgress.Paired ? 0xFFFFFFFF : _podModel.RadioAddress;
        var ackDataOutInterim = _podModel.RadioAddress;
        var ackDataIn = _podModel.RadioAddress;

        var er = new ExchangeResult { Result = AcceptanceType.Ignored };
        IPodPacket podFirstResponsePacket = null;
        // Send
        while (sendPacketIndex < sendPacketCount)
        {
            var isLastPacket = (sendPacketIndex == sendPacketCount - 1);
            var byteStart = sendPacketIndex * 31;
            var byteEnd = byteStart + 31;
            if (messageBody.Length < byteEnd)
                byteEnd = messageBody.Length;

            var packetToSend = new PodPacket(
                packetAddressOut,
                sendPacketIndex == 0 ? PodPacketType.Pdm : PodPacketType.Con,
                nextPacketSequence,
                messageBody.Sub(byteStart, byteEnd));

            var pe = await TryExchangePackets(packetToSend, cancellationToken);
            var receivedPacket = PodPacket.FromExchangeResult(pe, packetAddressIn);
            var packetCommandSent = (pe.CommunicationResult != BleCommunicationResult.WriteFailed);
            var bleConnectionFailed = (pe.CommunicationResult != BleCommunicationResult.OK); 

            if (isLastPacket && packetCommandSent)
            {
                er.RequestSentEarliest ??= pe.BleWriteCompleted;
                er.SentMessage = messageToSend;
                er.Result = AcceptanceType.Inconclusive;
            }

            if (bleConnectionFailed)
                continue;
            
            firstPacketSent ??= pe.BleWriteCompleted!.Value;
            if (receivedPacket != null)
            {
                lastPacketReceived = pe.BleReadIndicated;
            }
            else
            {
                var referenceTime = lastPacketReceived ?? firstPacketSent.Value;
                if (referenceTime < DateTimeOffset.UtcNow - TimeSpan.FromSeconds(30))
                {
                    return er.WithResult(null, "Timed out"); // out
                }
                continue; // send loop
            }
            
            Debug.Assert(receivedPacket != null);
            podFirstResponsePacket = receivedPacket;
            
            _podModel.LastRadioPacketReceived = lastPacketReceived;
            if (receivedPacket.Sequence != (nextPacketSequence + 1) % 32)
                continue; // send loop

            nextPacketSequence = (receivedPacket.Sequence + 1) % 32;
            _podModel.NextPacketSequence = nextPacketSequence;

            if (isLastPacket)
            {
                er.RequestSentLatest ??= pe.BleReadIndicated;
                
                if (receivedPacket.Type != PodPacketType.Pod)
                {
                    if (receivedPacket.Type == PodPacketType.Ack)
                        return er.WithResult(AcceptanceType.RejectedResyncRequired, "Pod requires resync");
                    return er.WithResult(AcceptanceType.RejectedProtocolError,
                        "Pod didn't respond with expected packet type");
                }
            }
            else
            {
                // interim send packet
                if (receivedPacket.Type != PodPacketType.Ack)
                {
                    return er.WithResult(AcceptanceType.RejectedProtocolError,
                        "Pod didn't respond with expected packet type");
                }

                if (receivedPacket.Data.DWord(0) != ackDataIn)
                {
                    return er.WithResult(AcceptanceType.RejectedProtocolError,
                        "Pod didn't respond with expected ack data");
                }
            }
            sendPacketIndex++;
        } // send looop

        // receive
        Debug.Assert(podFirstResponsePacket != null, " first received packet != null");

        if (podFirstResponsePacket.Data.Length < 7)
            return er.WithResult(AcceptanceType.Inconclusive,
                "First pod packet is too short to determine type");

        var podFirstResponseData = podFirstResponsePacket.Data.Sub(6);
        
        var b0 = podFirstResponsePacket.Data[4];
        var b1 = podFirstResponsePacket.Data[5];

        var receivedMessageSequence = (b0 & 0b00111100) >> 2;
        var responseMessageLength = (((b0 & 0x03) << 8) | b1) + 4 + 2 + 2;
        var receivedMessageLength = podFirstResponsePacket.Data.Length;

        var podResponsePackets = new List<IPodPacket>();
        podResponsePackets.Add(podFirstResponsePacket);

        while (receivedMessageLength < responseMessageLength)
        {
            var interimAck = new PodPacket(
                packetAddressOut,
                PodPacketType.Ack,
                nextPacketSequence,
                new Bytes(ackDataOutInterim)
            );

            var pe = await TryExchangePackets(interimAck, cancellationToken);
            var receivedPacket = PodPacket.FromExchangeResult(pe, packetAddressIn);
            
            if (receivedPacket == null)
            {
                if (_podModel.LastRadioPacketReceived < DateTimeOffset.Now - TimeSpan.FromSeconds(30))
                {
                    er.ErrorText = "Pod response partially received due to timeout";
                    break; // timed out, exit receiveloop
                }
                continue; // not timed out yet, continue receive loop
            }
            
            Debug.Assert(receivedPacket != null);
            _podModel.LastRadioPacketReceived = DateTimeOffset.Now;
            if (receivedPacket.Sequence != (nextPacketSequence + 1) % 32)
                continue; // sequence mismatch, continue receive loop

            if (receivedPacket.Type != PodPacketType.Con)
            {
                er.ErrorText = "Pod response protocol error (invalid type), only partially valid.";
                break; // out of receive loop
            }

            if (receivedPacket.Data.Length + receivedMessageLength > responseMessageLength)
            {
                er.ErrorText = "Pod response protocol error (message too long), only partially valid.";
                break;
            }

            podResponsePackets.Add(receivedPacket);
            nextPacketSequence = (receivedPacket.Sequence + 1) % 32;
            receivedMessageLength += receivedPacket.Data.Length;
        }

        _podModel.NextPacketSequence = nextPacketSequence;
        var podMessageReceived = PodMessage.FromReceivedPackets(podResponsePackets);
        er.ReceivedMessage = podMessageReceived;
        
        _podModel.NextMessageSequence = (receivedMessageSequence + 1) % 16;
        
        er.Result = AcceptanceType.Inconclusive;
        var firstPartMessageType = (PodMessageType)podFirstResponseData[0];
        var accepted = false;
        switch (firstPartMessageType)
        {
            case PodMessageType.ResponseStatus:
            case PodMessageType.ResponseVersionInfo:
                er.Result = podMessageReceived == null ? AcceptanceType.Inconclusive : AcceptanceType.Accepted;
                break;
            case PodMessageType.ResponseInfo:
                if (podFirstResponseData.Length < 3)
                    er.Result = AcceptanceType.Inconclusive;
                else
                {
                    if (podFirstResponseData[2] == (byte)RequestStatusType.Extended)
                    {
                        if (podFirstResponseData.Length < 10)
                            er.Result = AcceptanceType.Inconclusive;
                        else
                        {
                            if (messageToSend.Parts[0].Type == PodMessageType.RequestStatus
                                && messageToSend.Parts[0].Data[0] == (byte)RequestStatusType.Extended)
                            {
                                er.Result = podMessageReceived == null ? AcceptanceType.Inconclusive : AcceptanceType.Accepted;
                            }
                            else
                            {
                                if (podFirstResponseData[3] == (byte)PodProgress.Inactive)
                                    er.Result = AcceptanceType.Accepted;
                                else
                                    er.Result = AcceptanceType.RejectedFaultOccured;
                            }
                        }
                    }
                    else
                    {
                        er.Result = AcceptanceType.Accepted;
                    }
                }
                break;
            case PodMessageType.ResponseError:
                if (podFirstResponseData.Length < 3)
                    er.Result = AcceptanceType.Inconclusive;
                else
                    if (podFirstResponseData[2] == 0x14)
                        er.Result = AcceptanceType.RejectedNonceReseed;
                    else
                        er.Result = AcceptanceType.RejectedErrorOccured;
                break;
            default:
                er.Result = AcceptanceType.Inconclusive;
                break;
        }

        return er;
    }

    private async Task AckExchangeAsync(
        CancellationToken cancellationToken = default)
    {
        var ackDataOutFinal = _podModel.Progress < PodProgress.Paired ? _podModel.RadioAddress : 0x00000000;
        var finalAck = new PodPacket(
            _podModel.RadioAddress,
            PodPacketType.Ack,
            _podModel.NextPacketSequence,
            new Bytes(ackDataOutFinal));

        DateTimeOffset? successfulSendWithoutResponse = null;
        while (true)
        {
            Debug.WriteLine("Final ack sending");
            var ber = await TryExchangePackets(finalAck, cancellationToken);
            if (ber.CommunicationResult != BleCommunicationResult.OK)
            {
                successfulSendWithoutResponse = null;
                continue;
            }
            var receivedPacket = PodPacket.FromExchangeResult(ber, _podModel.RadioAddress);
            if (receivedPacket != null)
            {
                successfulSendWithoutResponse = null;
                _podModel.LastRadioPacketReceived = ber.BleReadIndicated;
                Debug.WriteLine("Final ack received response");
            }
            else
            {
                if (successfulSendWithoutResponse == null)
                {
                    successfulSendWithoutResponse = ber.BleWriteCompleted;
                }
            }

            if (successfulSendWithoutResponse.HasValue
                && successfulSendWithoutResponse < DateTimeOffset.UtcNow - TimeSpan.FromSeconds(10))
                break;
        }

        Debug.WriteLine("Final send complete");
        _podModel.NextPacketSequence = (_podModel.NextPacketSequence + 1) % 32;
    }

    private async Task<BleExchangeResult> TryExchangePackets(
        IPodPacket packetToSend,
        CancellationToken cancellationToken)
    {
        IPodPacket? received = null;
        Debug.WriteLine($"SEND: {packetToSend}");
        if (!_podModel.LastRadioPacketReceived.HasValue ||
            _podModel.LastRadioPacketReceived < DateTimeOffset.Now - TimeSpan.FromSeconds(30))
            return await _radioConnection.SendAndTryGetPacket(
                0, 0, 0, 150,
                0, 250, 0, packetToSend, cancellationToken);
        return await _radioConnection.SendAndTryGetPacket(
            0, 3, 25, 0,
            0, 250, 0, packetToSend, cancellationToken);
    }
}
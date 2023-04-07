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

    public async Task<PodResponse> PrimePodAsync(
        DateOnly podDate,
        TimeOnly podTime,
        bool relaxDeliveryCrosschecks,
        CancellationToken cancellationToken)
    {
        if (_podModel.Progress > PodProgress.Priming)
            return PodResponse.NotAllowed;

        PodResponse result = PodResponse.OK;
        if (_podModel.Progress is null or < PodProgress.Paired)
        {
            result = await SetRadioAddress(cancellationToken);
            if (result != PodResponse.OK)
                return result;
        }

        if (!_podModel.Progress.HasValue)
            return PodResponse.Error;
        
        if (_podModel.Progress == PodProgress.Paired)
        {
            result = await SetupPod(podDate, podTime, 4, cancellationToken);
            if (result != PodResponse.OK)
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
            if (result != PodResponse.OK)
                return result;
        }

        if (relaxDeliveryCrosschecks && _podModel.Progress == PodProgress.Paired)
        {
            result = await SetDeliveryFlags(0, 0, cancellationToken);
            if (result != PodResponse.OK)
                return result;
        }
        
        if (_podModel.Progress == PodProgress.Paired)
        {
            result = await Bolus(52, 1, cancellationToken);
            if (result != PodResponse.OK)
                return result;
            if (!_podModel.ImmediateBolusActive.HasValue ||
                !_podModel.Faulted.HasValue)
                return PodResponse.Error;
            
            if (!_podModel.ImmediateBolusActive.Value || _podModel.Progress != PodProgress.Priming
                || _podModel.Faulted.Value)
                return PodResponse.Error;
            
            await Task.Delay(TimeSpan.FromSeconds(52));
            result = await UpdateStatus(cancellationToken);
            if (result != PodResponse.OK)
                return result;
        }

        if (!_podModel.PulsesPending.HasValue)
            return PodResponse.Error;

        if (_podModel.Progress == PodProgress.Priming)
        {
            await Task.Delay(TimeSpan.FromSeconds(_podModel.PulsesPending.Value + 2));
            result = await UpdateStatus(cancellationToken);
            if (result != PodResponse.OK)
                return result;
            if (_podModel.Progress != PodProgress.Primed)
                return PodResponse.Error;
        }

        if (_podModel.Progress != PodProgress.Primed)
            return PodResponse.Error;

        return PodResponse.OK;
    }

    public async Task<PodResponse> StartPodAsync(
        TimeOnly podTime,
        BasalRateEntry[] basalRateEntries,
        CancellationToken cancellationToken = default)
    {
        if (!_podModel.Progress.HasValue || !_podModel.Faulted.HasValue
            || _podModel.Faulted.Value)
            return PodResponse.NotAllowed;
        if (_podModel.Progress < PodProgress.Primed)
            return PodResponse.NotAllowed;
        if (_podModel.Progress >= PodProgress.Running)
            return PodResponse.NotAllowed;

        PodResponse result;
        if (_podModel.Progress == PodProgress.Primed)
        {
            result = await SetBasalSchedule(podTime, basalRateEntries, cancellationToken);
            if (result != PodResponse.OK)
                return result;
            if (_podModel.Progress != PodProgress.BasalSet)
                return PodResponse.Error;
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
            if (result != PodResponse.OK)
                return result;
            if (_podModel.Faulted.Value)
                return PodResponse.Error;
            
            result = await Bolus(10, 1, cancellationToken);
            if (result != PodResponse.OK)
                return result;
            if (_podModel.Faulted.Value)
                return PodResponse.Error;

            if (!_podModel.ImmediateBolusActive.HasValue ||
                !_podModel.ImmediateBolusActive.Value ||
                _podModel.Progress != PodProgress.Inserting ||
                _podModel.Faulted.Value)
                return PodResponse.Error;
            
            await Task.Delay(TimeSpan.FromSeconds(10));
            
            result = await UpdateStatus(cancellationToken);
            if (result != PodResponse.OK)
                return result;
        }
        
        if (_podModel.Progress == PodProgress.Inserting)
        {
            await Task.Delay(TimeSpan.FromSeconds(_podModel.PulsesPending + 2));
            result = await UpdateStatus(cancellationToken);
            if (result != PodResponse.OK)
                return result;
            if (_podModel.Progress != PodProgress.Running)
                return PodResponse.Error;
        }
        
        return PodResponse.OK;
    }

    private async Task<PodResponse> SetRadioAddress(CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress >= PodProgress.Paired)
            return PodResponse.NotAllowed;
        
        return await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestAssignAddressPart(_podModel.RadioAddress)
            }
        );
    }

    private async Task<PodResponse> SetupPod(DateOnly podDate, TimeOnly podTime,
        int packetTimeout = 0, CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress != PodProgress.Paired)
            return PodResponse.NotAllowed;

        if (!_podModel.Lot.HasValue || !_podModel.Serial.HasValue)
            return PodResponse.NotAllowed;
        
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

    private async Task<PodResponse> SetDeliveryFlags(byte b16, byte b17, CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress != PodProgress.Paired)
            return PodResponse.NotAllowed;
        
        return await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestSetDeliveryFlagsPart(b16, b17)
            }
        );
    }

    public async Task<PodResponse> SetBasalSchedule(
        TimeOnly podTime,
        BasalRateEntry[] basalRateEntries,
        CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress < PodProgress.Primed || _podModel.Progress > PodProgress.RunningLow)
            return PodResponse.NotAllowed;

        PodResponse result;
        result = await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            }
        );
        if (result != PodResponse.OK)
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
            if (result != PodResponse.OK)
                return result;
            if (_podModel.BasalActive)
                return PodResponse.Error;
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

    public async Task<PodResponse> AcknowledgeAlerts(CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress < PodProgress.Paired || _podModel.Progress >= PodProgress.Inactive)
            return PodResponse.NotAllowed;

        return await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestAcknowledgeAlertsPart()
            }
        );
    }
    
    public async Task<PodResponse> UpdateStatus(CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress < PodProgress.Paired || _podModel.Progress >= PodProgress.Inactive)
            return PodResponse.NotAllowed;

        return await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            }
        );
    }

    public async Task<PodResponse> ConfigureAlerts(
        AlertConfiguration[] alertConfigurations,
        CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress < PodProgress.Paired || _podModel.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

        var result = await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            }
        );
        if (result != PodResponse.OK)
            return result;

        if (_podModel.Progress < PodProgress.Paired || _podModel.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

        result = await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestConfigureAlertsPart(alertConfigurations)
            }
        );

        return result;
    }

    public async Task<PodResponse> Beep(BeepType type, CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress < PodProgress.Paired || _podModel.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

        var result = await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            }
        );
        if (result != PodResponse.OK)
            return result;

        if (_podModel.Progress < PodProgress.Paired || _podModel.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

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

    public async Task<PodResponse> CancelTempBasal(CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress < PodProgress.Running || _podModel.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

        var result = await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            });

        if (result != PodResponse.OK)
            return result;

        if (_podModel.Progress < PodProgress.Running || _podModel.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

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

    public async Task<PodResponse> SetTempBasal(
        int pulsesPerHour,
        int halfHourCount,
        CancellationToken cancellationToken = default)
    {
        //var pulsesPerHour = hourlyRateMilliunits / (_pod.UnitsPerMilliliter / 2);

        if (halfHourCount < 0 || halfHourCount > 12 || pulsesPerHour < 0 || pulsesPerHour > 1800)
            return PodResponse.NotAllowed;

        if (_podModel.Progress < PodProgress.Running || _podModel.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

        var result = await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            });
        
        if (result != PodResponse.OK)
            return result;

        if (_podModel.Progress < PodProgress.Running || _podModel.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;
        
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
            if (result != PodResponse.OK)
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

    public async Task<PodResponse> Bolus(
        int bolusPulses,
        int pulseIntervalSeconds,
        CancellationToken cancellationToken = default)
    {
        //var bolusPulses = bolusMilliunits / (_pod.UnitsPerMilliliter / 2);

        if (pulseIntervalSeconds < 2 || bolusPulses < 0 || bolusPulses > 1800 / pulseIntervalSeconds)
            return PodResponse.NotAllowed;

        if (_podModel.Progress < PodProgress.Running || _podModel.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

        var result = await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            });

        if (result != PodResponse.OK)
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
            if (result != PodResponse.OK)
                return result;
        }
            
        if (_podModel.Progress < PodProgress.Running || _podModel.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

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

    public async Task<PodResponse> CancelBolus(CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress < PodProgress.Running || _podModel.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

        var result = await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            });

        if (result != PodResponse.OK)
            return result;

        if (!_podModel.ImmediateBolusActive && !_podModel.ExtendedBolusActive)
            return PodResponse.NotAllowed;

        if (_podModel.Progress < PodProgress.Running || _podModel.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

        return await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestCancelPart(BeepType.NoSound,
                    true,
                    false,
                    false)
            });
    }


    public async Task<PodResponse> Deactivate(CancellationToken cancellationToken = default)
    {
        if (_podModel.Progress < PodProgress.Paired || _podModel.Progress >= PodProgress.Inactive)
            return PodResponse.NotAllowed;

        var result = await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            }
        );
        if (result != PodResponse.OK)
            return result;

        if (_podModel.Progress < PodProgress.Paired || _podModel.Progress >= PodProgress.Inactive)
            return PodResponse.NotAllowed;

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
            if (result != PodResponse.OK)
                return result;
        }

        return await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestDeactivatePodPart()
            });
    }

    private async Task<PodResponse> SendRequestAsync(
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
        var result = await RunExchangeAsync(
            messageToSend,
            cancellationToken);
        var receiveEnd = DateTimeOffset.UtcNow;

        var sentData = messageToSend.GetBody().ToArray();
        var receivedData = result.Message?.Body?.ToArray();

        using var ocdb = new OcdbContext();
        await ocdb.PodActions.AddAsync(new PodAction
        {
            PodId = _podModel.Id,
            Index = _podModel.NextRecordIndex,
            ClientId = clientId,
            SendStart = sendStart,
            Sent = sentData,
            ReceiveEnd = receiveEnd,
            Received = receivedData,
            Status = result.Status,
            IsSynced = false
        });
        await ocdb.SaveChangesAsync(cancellationToken);
        
        await _syncService.SyncPodMessage(_podModel.Id, _podModel.NextRecordIndex);
        _podModel.NextRecordIndex++;

        if (result.Status != CommunicationStatus.NoResponse)
            _communicationNeedsClosing = true;

        switch (result.Status)
        {
            case CommunicationStatus.None:
                if (result.Message.Parts[0] is ResponseErrorPart rep)
                {
                    if (rep.ErrorCode == 0x14 && authRetries == 0)
                    {
                        //_pod.NextPacketSequence = initialPacketSequence;
                        _podModel.NextMessageSequence = initialMessageSequence;
                        _podModel.SyncNonce(rep.ErrorValue, initialMessageSequence);
                        authRetries++;
                        return await SendRequestAsync(
                            critical,
                            cancellationToken,
                            parts,
                            authRetries,
                            syncRetries
                        );
                    }

                    await _podModel.ProcessResultAsync(result);
                    return PodResponse.Error;
                }

                await _podModel.ProcessResultAsync(result);
                if (_podModel.Faulted)
                    return PodResponse.Faulted;
                return PodResponse.OK;
            case CommunicationStatus.MessageSyncRequired:
                if (syncRetries == 0)
                {
                    _podModel.NextMessageSequence = (initialMessageSequence + 2) % 16;

                    syncRetries++;
                    return await SendRequestAsync(
                        critical,
                        cancellationToken,
                        parts,
                        authRetries,
                        syncRetries
                    );
                }

                return PodResponse.Error;
            case CommunicationStatus.NoResponse:
                return PodResponse.NoResponse;
            case CommunicationStatus.ConnectionInterrupted:
                _podModel.NextMessageSequence = (initialMessageSequence + 2) % 16;
                _podModel.NextPacketSequence = 0;
                return PodResponse.Interrupted;
            case CommunicationStatus.ProtocolError:
                _podModel.NextMessageSequence = (initialMessageSequence + 2) % 16;
                _podModel.NextPacketSequence = 0;
                return PodResponse.Error;
            case CommunicationStatus.UnidentifiedResponse:
                return PodResponse.Error;
            case CommunicationStatus.Unknown:
                return PodResponse.Error;
            default:
                return PodResponse.Error;
        }
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
        IPodPacket? receivedPacket = null;
        var exchangeStarted = DateTimeOffset.Now;
        var nextPacketSequence = _podModel.NextPacketSequence;
        var nextMessageSequence = _podModel.NextMessageSequence;
        DateTimeOffset? firstPacketSent = null;
        DateTimeOffset? lastPacketReceived = null;
        DateTimeOffset? lastPacketSent = null;

        var packetAddressIn = _podModel.Progress < PodProgress.Paired ? 0xFFFFFFFF : _podModel.RadioAddress;
        var packetAddressOut = _podModel.Progress < PodProgress.Paired ? 0xFFFFFFFF : _podModel.RadioAddress;
        var ackDataOutInterim = _podModel.RadioAddress;
        var ackDataIn = _podModel.RadioAddress;

        DateTimeOffset? sendStart = null;
        DateTimeOffset? receiveStart = null;
        
        // Send
        while (sendPacketIndex < sendPacketCount)
        {
            var byteStart = sendPacketIndex * 31;
            var byteEnd = byteStart + 31;
            if (messageBody.Length < byteEnd)
                byteEnd = messageBody.Length;

            var packetToSend = new PodPacket(
                packetAddressOut,
                sendPacketIndex == 0 ? PodPacketType.Pdm : PodPacketType.Con,
                nextPacketSequence,
                messageBody.Sub(byteStart, byteEnd));

            receivedPacket = await TryExchangePackets(packetToSend, cancellationToken, packetAddressIn);
            var now = DateTimeOffset.UtcNow;
            if (!firstPacketSent.HasValue)
                firstPacketSent = now;
            if (receivedPacket != null)
                lastPacketReceived = now;

            if (receivedPacket == null)
            {
                var referenceTime = lastPacketReceived ?? firstPacketSent.Value;
                if (referenceTime < now - TimeSpan.FromSeconds(30))
                    return new ExchangeResult
                    {
                        SendStart = sendStart,
                        SentMessage = messageToSend,
                        ReceiveStart = null,
                        ReceiveResult = ResponseReceiveResult.NothingReceived,
                        SendResult = RequestSendResult.FullySent,
                        AcknowledgementResult = RequestAcknowledgementResult.Inconclusive,
                        ReceivedMessage = null,
                        ErrorText = "Connection timed out",
                        Status = CommunicationStatus.NoResponse,
                    };
                
                if (sendPacketIndex == 0 && firstPacketSent < now - TimeSpan.FromSeconds(30))
                    return new ExchangeResult
                    {
                        SendStart = sendStart,
                        SentMessage = messageToSend,
                        ReceiveStart = null,
                        ReceiveResult = ResponseReceiveResult.NothingReceived,
                        SendResult = RequestSendResult.FullySent,
                        AcknowledgementResult = RequestAcknowledgementResult.Inconclusive,
                        ReceivedMessage = null,
                        ErrorText = "Connection timed out",
                        Status = CommunicationStatus.NoResponse,
                    };
                if (sendPacketIndex > 0 && _podModel.LastRadioPacketReceived < now - TimeSpan.FromSeconds(30))
                {
                    _podModel.NextPacketSequence = nextPacketSequence;
                    return new ExchangeResult
                    {
                        ErrorText = "Connection timed out during communication, message may be partially transmitted.",
                        Status = CommunicationStatus.ConnectionInterrupted,
                        SendStart = sendStart,
                    };
                }

                continue;
            }

            _podModel.LastRadioPacketReceived = now;

            if (receivedPacket.Sequence != (nextPacketSequence + 1) % 32) continue;

            nextPacketSequence = (receivedPacket.Sequence + 1) % 32;
            _podModel.NextPacketSequence = nextPacketSequence;

            if (sendPacketIndex == sendPacketCount - 1)
            {
                // last send packet
                if (receiveStart != null)
                    receiveStart = now;
                
                if (receivedPacket.Type != PodPacketType.Pod)
                {
                    if (receivedPacket.Type == PodPacketType.Ack)
                        return new ExchangeResult
                        {
                            ErrorText = $"Expected Pod first response packet: {receivedPacket}",
                            Status = CommunicationStatus.MessageSyncRequired,
                            SendStart = sendStart,
                            SentMessage = messageToSend,
                            ReceiveStart = receiveStart
                        };
                    return new ExchangeResult
                    {
                        ErrorText = $"Expected Pod first response packet, received: {receivedPacket}",
                        Status = CommunicationStatus.ProtocolError,
                        SendStart = sendStart,
                        SentMessage = messageToSend,
                        ReceiveStart = receiveStart
                    };
                }
            }
            else
            {
                // interim send packet
                if (receivedPacket.Type != PodPacketType.Ack)
                {
                    return new ExchangeResult
                    {
                        ErrorText = $"Expected Ack to continue sending message, received: {receivedPacket}",
                        Status = CommunicationStatus.ProtocolError,
                        SendStart = sendStart,
                        SentMessage = messageToSend,
                        ReceiveStart = receiveStart
                    };
                }

                if (receivedPacket.Data.DWord(0) != ackDataIn)
                {
                    return new ExchangeResult
                    {
                        ErrorText = $"Received ack data with mismatched address: {receivedPacket}",
                        Status = CommunicationStatus.ProtocolError,
                        SendStart = sendStart,
                        SentMessage = messageToSend,
                        ReceiveStart = receiveStart
                    };
                }
            }

            sendPacketIndex++;
        }

        // receive
        Debug.Assert(receivedPacket != null, " first received packet != null");
        var b0 = receivedPacket.Data[4];
        var b1 = receivedPacket.Data[5];

        var receivedMessageSequence = (b0 & 0b00111100) >> 2;
        var responseMessageLength = (((b0 & 0x03) << 8) | b1) + 4 + 2 + 2;
        var receivedMessageLength = receivedPacket.Data.Length;

        var podResponsePackets = new List<IPodPacket>();
        podResponsePackets.Add(receivedPacket);

        while (receivedMessageLength < responseMessageLength)
        {
            var interimAck = new PodPacket(
                packetAddressOut,
                PodPacketType.Ack,
                nextPacketSequence,
                new Bytes(ackDataOutInterim)
            );

            receivedPacket = await TryExchangePackets(interimAck, cancellationToken);

            if (receivedPacket == null || receivedPacket.Address != packetAddressIn)
            {
                if (_podModel.LastRadioPacketReceived < DateTimeOffset.Now - TimeSpan.FromSeconds(30))
                    return new ExchangeResult
                    {
                        ErrorText = "Connection timed out during communication, only partial response received.",
                        Status = CommunicationStatus.ConnectionInterrupted,
                        SendStart = sendStart,
                        SentMessage = messageToSend,
                        ReceiveStart = receiveStart
                    };
                    return new ExchangeResult(CommunicationStatus.ConnectionInterrupted);
                continue;
            }

            _podModel.LastRadioPacketReceived = DateTimeOffset.Now;
            if (receivedPacket.Sequence != (nextPacketSequence + 1) % 32) continue;

            if (receivedPacket.Type != PodPacketType.Con)
            {
                Trace.WriteLine($"Expected type Con, received: {receivedPacket}");
                return new ExchangeResult(CommunicationStatus.ProtocolError);
            }

            if (receivedPacket.Data.Length + receivedMessageLength > responseMessageLength)
            {
                Trace.WriteLine($"Received message exceeds expected data length! last received: {receivedPacket}");
                return new ExchangeResult(CommunicationStatus.ProtocolError);
            }

            podResponsePackets.Add(receivedPacket);
            nextPacketSequence = (receivedPacket.Sequence + 1) % 32;
            receivedMessageLength += receivedPacket.Data.Length;
        }

        _podModel.NextPacketSequence = nextPacketSequence;

        var podMessageReceived = PodMessage.FromReceivedPackets(podResponsePackets);
        if (podMessageReceived == null) return new ExchangeResult(CommunicationStatus.UnidentifiedResponse);
        _podModel.NextMessageSequence = (podMessageReceived.Sequence + 1) % 16;
        return new ExchangeResult(podMessageReceived);
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

        while (true)
        {
            Debug.WriteLine("Final ack sending");
            var received = await TryExchangePackets(finalAck, cancellationToken);
            var now = DateTimeOffset.Now;
            if (received != null && received.Address == _podModel.RadioAddress)
            {
                _podModel.LastRadioPacketReceived = now;
                Debug.WriteLine("Final ack received response");
            }

            if (_podModel.LastRadioPacketReceived < now - TimeSpan.FromSeconds(5))
                break;
            if (cancellationToken.IsCancellationRequested)
                break;
        }

        Debug.WriteLine("Final send complete");
        _podModel.NextPacketSequence = (_podModel.NextPacketSequence + 1) % 32;
    }

    private async Task<IPodPacket?> TryExchangePackets(
        IPodPacket packetToSend,
        CancellationToken cancellationToken,
        uint matchAddress)
    {
        IPodPacket? received = null;
        Debug.WriteLine($"SEND: {packetToSend}");
        if (!_podModel.LastRadioPacketReceived.HasValue ||
            _podModel.LastRadioPacketReceived < DateTimeOffset.Now - TimeSpan.FromSeconds(30))
            received = await _radioConnection.SendAndTryGetPacket(
                0, 0, 0, 150,
                0, 250, 0, packetToSend, cancellationToken);
        else
            received = await _radioConnection.SendAndTryGetPacket(
                0, 3, 25, 0,
                0, 250, 0, packetToSend, cancellationToken);

        if (received != null && received.Address == matchAddress)
        {
            Debug.WriteLine($"RCVD OK--: {received}");
            return received;
        }
        else
        {
            Debug.WriteLine($"RCVD FAIL: {received}");
            return null;
        }
    }
}
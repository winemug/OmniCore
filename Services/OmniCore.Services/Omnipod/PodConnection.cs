using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using OmniCore.Services.Interfaces;
using Plugin.BLE.Abstractions;
using Trace = System.Diagnostics.Trace;
using Dapper;

namespace OmniCore.Services;

public class PodConnection : IDisposable, IPodConnection
{
    private IPod _pod;
    private IDisposable _podLockDisposable;
    private IRadioConnection _radioConnection;
    private bool _communicationNeedsClosing;
    private IDataService _dataService;
    
    public PodConnection(IPod pod,
        IRadioConnection radioConnection,
        IDisposable podLockDisposable,
        IDataService dataService)
    {
        _pod = pod;
        _radioConnection = radioConnection;
        _podLockDisposable = podLockDisposable;
        _dataService = dataService;
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

    public async Task<PodResponse> Pair(CancellationToken cancellationToken = default)
    {
        if (_pod.Info.Progress >= PodProgress.Paired)
            return PodResponse.NotAllowed;

        PodResponse result;
        if (_pod.Info.Progress == PodProgress.Init0)
        {
            result = await SendRequestAsync(false,
                cancellationToken,
                new[]
                {
                    new RequestAssignAddressPart(_pod.RadioAddress)
                }
            );
            if (result != PodResponse.OK)
                return result;
        }

        var now = DateTimeOffset.Now;
        return await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestSetupPodPart(_pod.RadioAddress,
                    _pod.Info.Lot, _pod.Info.Serial, 4,
                    now.Year, now.Month, now.Day, now.Hour, now.Minute)
            }
        );
    }

    public async Task<PodResponse> Activate(CancellationToken cancellationToken = default)
    {
        if (_pod.Info.Progress != PodProgress.Paired)
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
        if (_pod.Info.Progress != PodProgress.Paired)
            return PodResponse.NotAllowed;
        
        throw new NotImplementedException();
    }

   
    public async Task<PodResponse> Start(
        BasalRateEntry[] basalRateEntries,
        CancellationToken cancellationToken = default)
    {
        if (_pod.Info.Progress < PodProgress.Paired || _pod.Info.Progress >= PodProgress.Running)
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
        
        if (_pod.Info.Progress < PodProgress.Paired || _pod.Info.Progress >= PodProgress.Running)
            return PodResponse.NotAllowed;

        throw new NotImplementedException();
    }
    
    public async Task<PodResponse> UpdateStatus(CancellationToken cancellationToken = default)
    {
        if (_pod.Info.Progress < PodProgress.Paired || _pod.Info.Progress >= PodProgress.Inactive)
            return PodResponse.NotAllowed;

        return await SendRequestAsync(false,
            cancellationToken,
            new[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            }
        );
    }

    public async Task<PodResponse> Beep(BeepType type, CancellationToken cancellationToken = default)
    {
        if (_pod.Info.Progress < PodProgress.Paired || _pod.Info.Progress >= PodProgress.Faulted)
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
        
        if (_pod.Info.Progress < PodProgress.Paired || _pod.Info.Progress >= PodProgress.Faulted)
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
        if (_pod.Info.Progress < PodProgress.Running || _pod.Info.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

        var result = await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            });

        if (result != PodResponse.OK)
            return result;

        if (!_pod.Info.TempBasalActive || _pod.Info.ImmediateBolusActive || _pod.Info.ExtendedBolusActive)
            return PodResponse.NotAllowed;
        
        if (_pod.Info.Progress < PodProgress.Running || _pod.Info.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;
        
        return await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestCancelPart(BeepType.NoSound,
                    false,
                    true,
                    false)
            });
    }
    
    public async Task<PodResponse> SetTempBasal(
        int hourlyRateMilliunits,
        int halfHourCount,
        CancellationToken cancellationToken = default)
    {
        int pulsesPerHour = hourlyRateMilliunits / (_pod.UnitsPerMilliliter / 2);
        if (halfHourCount < 0 || halfHourCount > 12 || pulsesPerHour < 0 || pulsesPerHour > 1800)
            return PodResponse.NotAllowed;
        
        if (_pod.Info.Progress < PodProgress.Running || _pod.Info.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

        var result = await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            });

        if (result != PodResponse.OK)
            return result;

        if (_pod.Info.TempBasalActive || _pod.Info.ImmediateBolusActive || _pod.Info.ExtendedBolusActive)
            return PodResponse.NotAllowed;
        
        if (_pod.Info.Progress < PodProgress.Running || _pod.Info.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

        var bre = new BasalRateEntry()
        {
            HalfHourCount = halfHourCount,
            PulsesPerHour = pulsesPerHour,
        };
        return await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestInsulinSchedulePart(bre),
                new RequestTempBasalPart(bre),
            });
    }
    
    public async Task<PodResponse> Bolus(
        int bolusMilliunits,
        int pulseIntervalSeconds,
        CancellationToken cancellationToken = default)
    {
        int bolusPulses = bolusMilliunits / (_pod.UnitsPerMilliliter / 2);
        
        if (pulseIntervalSeconds < 2 || bolusPulses < 0 || bolusPulses > 1800 / pulseIntervalSeconds)
            return PodResponse.NotAllowed;
        
        if (_pod.Info.Progress < PodProgress.Running || _pod.Info.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

        var result = await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            });

        if (result != PodResponse.OK)
            return result;

        if (_pod.Info.ImmediateBolusActive || _pod.Info.ExtendedBolusActive)
            return PodResponse.NotAllowed;
        
        if (_pod.Info.Progress < PodProgress.Running || _pod.Info.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

        var be = new BolusEntry()
        {
            ImmediatePulseCount = bolusPulses,
            ImmediatePulseInterval125ms = pulseIntervalSeconds * 8,
        };
        return await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestInsulinSchedulePart(be),
                new RequestBolusPart(be),
            });
    }

    public async Task<PodResponse> CancelBasal(CancellationToken cancellationToken = default)
    {
        if (_pod.Info.Progress < PodProgress.Running || _pod.Info.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

        var result = await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            });

        if (result != PodResponse.OK)
            return result;

        if (!_pod.Info.BasalActive || _pod.Info.ImmediateBolusActive || _pod.Info.ExtendedBolusActive)
            return PodResponse.NotAllowed;

        if (_pod.Info.Progress < PodProgress.Running || _pod.Info.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

        return await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestCancelPart(BeepType.NoSound,
                    false,
                    false,
                    true)
            });
    }
    
    public async Task<PodResponse> CancelBolus(CancellationToken cancellationToken = default)
    {
        if (_pod.Info.Progress < PodProgress.Running || _pod.Info.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

        var result = await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestStatusPart(RequestStatusType.Default)
            });

        if (result != PodResponse.OK)
            return result;

        if (!_pod.Info.ImmediateBolusActive && !_pod.Info.ExtendedBolusActive)
            return PodResponse.NotAllowed;

        if (_pod.Info.Progress < PodProgress.Running || _pod.Info.Progress >= PodProgress.Faulted)
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

    public async Task<PodResponse> Suspend(CancellationToken cancellationToken = default)
    {
        if (_pod.Info.Progress < PodProgress.Running || _pod.Info.Progress >= PodProgress.Faulted)
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

        if (_pod.Info.Progress < PodProgress.Running || _pod.Info.Progress >= PodProgress.Faulted)
            return PodResponse.NotAllowed;

        return await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestCancelPart(BeepType.NoSound,
                    _pod.Info.ImmediateBolusActive,
                    _pod.Info.TempBasalActive,
                    _pod.Info.BasalActive)
            });
    }
    
    public async Task<PodResponse> Deactivate(CancellationToken cancellationToken = default)
    {
        if (_pod.Info.Progress < PodProgress.Paired || _pod.Info.Progress >= PodProgress.Inactive)
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

        if (_pod.Info.Progress < PodProgress.Paired || _pod.Info.Progress >= PodProgress.Inactive)
            return PodResponse.NotAllowed;

        result = await SendRequestAsync(false, cancellationToken,
            new MessagePart[]
            {
                new RequestCancelPart(BeepType.NoSound,
                    _pod.Info.ImmediateBolusActive,
                    _pod.Info.TempBasalActive,
                    _pod.Info.BasalActive)
            });
        if (result != PodResponse.OK)
            return result;

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
        var initialPacketSequence = _pod.Info.NextPacketSequence;
        var initialMessageSequence = _pod.Info.NextMessageSequence;

        var messageToSend = ConstructMessage(critical, parts);

        var sendStart = DateTimeOffset.UtcNow;
        var result = await RunExchangeAsync(
            messageToSend,
            cancellationToken);
        var receiveEnd = DateTimeOffset.UtcNow;

        using (var conn = await _dataService.GetConnectionAsync())
        {
            await conn.ExecuteAsync(
                "INSERT INTO pod_message(pod_id, record_index, send_start, send_data, " +
                "receive_end, receive_data, exchange_result) " +
                "VALUES(@pod_id, @record_index, @send_start, @send_data, " +
                "@receive_end, @receive_data, @exchange_result)",
                new
                {
                    pod_id = _pod.Id.ToString("N"),
                    record_index = _pod.Info.NextRecordIndex,
                    send_start = sendStart.ToUnixTimeMilliseconds(),
                    send_data = messageToSend.GetBody().ToArray(),
                    receive_end = receiveEnd.ToUnixTimeMilliseconds(),
                    receive_data = result.Message?.Body?.ToArray(),
                    exchange_result = (int)result.Error
                });
            _pod.Info.NextRecordIndex++;
        }

        if (result.Error != CommunicationError.NoResponse)
            _communicationNeedsClosing = true;

        switch (result.Error)
        {
            case CommunicationError.None:
                if (result.Message.Parts[0] is ResponseErrorPart rep)
                {
                    if (rep.ErrorCode == 0x14 && authRetries == 0)
                    {
                        //_pod.Info.NextPacketSequence = initialPacketSequence;
                        _pod.Info.NextMessageSequence = initialMessageSequence;
                        _pod.SyncNonce(rep.ErrorValue, initialMessageSequence);
                        authRetries++;
                        return await SendRequestAsync(
                            critical,
                            cancellationToken,
                            parts,
                            authRetries,
                            syncRetries
                        );
                    }
                    await _pod.ProcessResponseAsync(result.Message);
                    return PodResponse.Error;
                }
                await _pod.ProcessResponseAsync(result.Message);
                if (_pod.Info.Faulted)
                    return PodResponse.Faulted;
                return PodResponse.OK;
            case CommunicationError.MessageSyncRequired:
                if (syncRetries == 0)
                {
                    _pod.Info.NextMessageSequence = (initialMessageSequence + 2) % 16;
                    
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
            case CommunicationError.NoResponse:
                return PodResponse.NoResponse;
            case CommunicationError.ConnectionInterrupted:
                _pod.Info.NextMessageSequence = (initialMessageSequence + 2) % 16;
                _pod.Info.NextPacketSequence = 0;
                return PodResponse.Interrupted;
            case CommunicationError.ProtocolError:
                _pod.Info.NextMessageSequence = (initialMessageSequence + 2) % 16;
                _pod.Info.NextPacketSequence = 0;
                return PodResponse.Error;
            case CommunicationError.UnidentifiedResponse:
                return PodResponse.Error;
            case CommunicationError.Unknown:
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
                part.Nonce = _pod.NextNonce();
            msgParts.Add(part);
        }
        return new PodMessage
        {
            Address = _pod.RadioAddress,
            Sequence = _pod.Info.NextMessageSequence,
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
        int sendPacketIndex = 0;
        IPodPacket receivedPacket = null;
        var exchangeStarted = DateTimeOffset.Now;
        var nextPacketSequence = _pod.Info.NextPacketSequence;
        var nextMessageSequence = _pod.Info.NextMessageSequence;
        DateTimeOffset? firstPacketSent = null;
        DateTimeOffset? lastPacketSent = null;

        var packetAddressIn = _pod.Info.Progress < PodProgress.Paired ? 0xFFFFFFFF : _pod.RadioAddress;
        var packetAddressOut = _pod.Info.Progress < PodProgress.Paired ? 0xFFFFFFFF: _pod.RadioAddress;
        var ackDataOutInterim = _pod.RadioAddress;
        var ackDataIn = _pod.RadioAddress;

        // Send
        while (sendPacketIndex < sendPacketCount)
        {
            int byteStart = sendPacketIndex * 31;
            var byteEnd = byteStart + 31;
            if (messageBody.Length < byteEnd)
                byteEnd = messageBody.Length;

            var packetToSend = new PodPacket(
                packetAddressOut,
                sendPacketIndex == 0 ? PodPacketType.Pdm : PodPacketType.Con,
                nextPacketSequence,
                messageBody.Sub(byteStart, byteEnd));

            receivedPacket = await TryExchangePackets(packetToSend, cancellationToken);
            var now = DateTimeOffset.Now;
            lastPacketSent = now;
            if (!firstPacketSent.HasValue)
                firstPacketSent = now;

            if (receivedPacket == null || receivedPacket.Address != packetAddressIn)
            {
                if (sendPacketIndex == 0 && firstPacketSent < now - TimeSpan.FromSeconds(30))
                {
                    return new ExchangeResult(CommunicationError.NoResponse);
                }
                if (sendPacketIndex > 0 && _pod.Info.LastRadioPacketReceived < now - TimeSpan.FromSeconds(30))
                {
                    _pod.Info.NextPacketSequence = nextPacketSequence;
                    return new ExchangeResult(CommunicationError.ConnectionInterrupted);
                }
                continue;
            }

            _pod.Info.LastRadioPacketReceived = now;
            
            if (receivedPacket.Sequence != (nextPacketSequence + 1) % 32)
            {
                continue;
            }
            
            nextPacketSequence = (receivedPacket.Sequence + 1) % 32;
            _pod.Info.NextPacketSequence = nextPacketSequence;
            
            if (sendPacketIndex == sendPacketCount - 1)
            {
                // last send packet
                if (receivedPacket.Type != PodPacketType.Pod)
                {
                    Trace.WriteLine($"Expected Pod response, received: {receivedPacket}");
                    if (receivedPacket.Type == PodPacketType.Ack)
                        return new ExchangeResult(CommunicationError.MessageSyncRequired);
                    return new ExchangeResult(CommunicationError.ProtocolError);
                }
            }
            else
            {
                // interim send packet
                if (receivedPacket.Type != PodPacketType.Ack)
                {
                    Trace.WriteLine($"Expected Ack to continue sending message, received: {receivedPacket}");
                    return new ExchangeResult(CommunicationError.ProtocolError);
                }

                if (receivedPacket.Data.DWord(0) != ackDataIn)
                {
                    Trace.WriteLine($"Received ack data with mismatched address: {receivedPacket}");
                    return new ExchangeResult(CommunicationError.ProtocolError);
                }
            }
            sendPacketIndex++;
        }

        // receive
        Debug.Assert(receivedPacket != null, " first received packet != null");
        var b0 = receivedPacket.Data[4];
        var b1 = receivedPacket.Data[5];

        int receivedMessageSequence = (b0  & 0b00111100) >> 2;
        int responseMessageLength = ((b0 & 0x03) << 8 | b1) + 4 + 2 + 2;
        int receivedMessageLength = receivedPacket.Data.Length;

        var podResponsePackets = new List<IPodPacket>();
        podResponsePackets.Add(receivedPacket);
        
        while (receivedMessageLength < responseMessageLength)
        {
            PodPacket interimAck = new PodPacket(
                packetAddressOut,
                PodPacketType.Ack,
                nextPacketSequence,
                new Bytes(ackDataOutInterim)
            );

            receivedPacket = await TryExchangePackets(interimAck, cancellationToken);
            
            if (receivedPacket == null || receivedPacket.Address != packetAddressIn)
            {
                if (_pod.Info.LastRadioPacketReceived < DateTimeOffset.Now - TimeSpan.FromSeconds(30))
                {
                    return new ExchangeResult(CommunicationError.ConnectionInterrupted);
                }
                continue;
            }
            
            _pod.Info.LastRadioPacketReceived = DateTimeOffset.Now;
            if (receivedPacket.Sequence != (nextPacketSequence + 1) % 32)
            {
                continue;
            }
            
            if (receivedPacket.Type != PodPacketType.Con)
            {
                Trace.WriteLine($"Expected type Con, received: {receivedPacket}");
                return new ExchangeResult(CommunicationError.ProtocolError);
            }
            if (receivedPacket.Data.Length + receivedMessageLength > responseMessageLength)
            {
                Trace.WriteLine($"Received message exceeds expected data length! last received: {receivedPacket}");
                return new ExchangeResult(CommunicationError.ProtocolError);
            }
            
            podResponsePackets.Add(receivedPacket);
            nextPacketSequence = (receivedPacket.Sequence + 1) % 32;
            receivedMessageLength += receivedPacket.Data.Length;
        }
      
        _pod.Info.NextPacketSequence = nextPacketSequence;
        
        var podMessageReceived = PodMessage.FromReceivedPackets(podResponsePackets);
        if (podMessageReceived == null)
        {
            return new ExchangeResult(CommunicationError.UnidentifiedResponse);
        }
        _pod.Info.NextMessageSequence = (podMessageReceived.Sequence + 1) % 16;
        return new ExchangeResult(podMessageReceived);
    }

    private async Task AckExchangeAsync(
        CancellationToken cancellationToken = default)
    {
        uint ackDataOutFinal = _pod.Info.Progress < PodProgress.Paired ? _pod.RadioAddress : 0x00000000;
        PodPacket finalAck = new PodPacket(
            _pod.RadioAddress,
            PodPacketType.Ack,
            _pod.Info.NextPacketSequence,
            new Bytes(ackDataOutFinal));

        while (true)
        {
            Debug.WriteLine($"Final ack sending");
            var received = await TryExchangePackets(finalAck, cancellationToken);
            var now = DateTimeOffset.Now;
            if (received != null && received.Address == _pod.RadioAddress)
            {
                _pod.Info.LastRadioPacketReceived = now;
                Debug.WriteLine($"Final ack received response");
            }

            if (_pod.Info.LastRadioPacketReceived < now - TimeSpan.FromSeconds(5))
                break;
            if (cancellationToken.IsCancellationRequested)
                break;
        }
        Debug.WriteLine($"Final send complete");
        _pod.Info.NextPacketSequence = (_pod.Info.NextPacketSequence + 1) % 32;
    }

    private async Task<IPodPacket> TryExchangePackets(
        IPodPacket packetToSend,
        CancellationToken cancellationToken)
    {
        IPodPacket received = null;
        Debug.WriteLine($"SEND: {packetToSend}");
        if (!_pod.Info.LastRadioPacketReceived.HasValue ||
            _pod.Info.LastRadioPacketReceived < DateTimeOffset.Now - TimeSpan.FromSeconds(30))
        {
            received = await _radioConnection.SendAndTryGetPacket(
                0, 0, 0, 150,
                0, 250, 0, packetToSend, cancellationToken);
        }
        else
        {
            received =  await _radioConnection.SendAndTryGetPacket(
                0, 3, 25, 0,
                0, 250, 0, packetToSend, cancellationToken);
        }

        if (received != null)
        {
            Debug.WriteLine($"RCVD: {received}");
        }
        else
        {
            Debug.WriteLine($"RCVD: --------");
        }
        return received;
    }

}

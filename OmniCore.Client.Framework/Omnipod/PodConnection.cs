using System.Diagnostics;
using OmniCore.Client.Model;
using OmniCore.Common.Core;
using OmniCore.Common.Entities;
using OmniCore.Common.Pod;
using OmniCore.Common.Radio;
using OmniCore.Framework.Omnipod.Requests;
using OmniCore.Framework.Omnipod.Responses;
using OmniCore.Shared.Enums;

namespace OmniCore.Framework.Omnipod;

public class PodConnection : IDisposable, IPodConnection
{
    private readonly IDisposable _podLockDisposable;
    private readonly IPodModel _podModel;
    private readonly IRadioConnection _radioConnection;
    private readonly ISyncService _syncService;
    private bool _communicationNeedsClosing;
    private readonly Guid _requestingClientId;

    public PodConnection(
        Guid requestingClientId,
        IPodModel podModel,
        IRadioConnection radioConnection,
        IDisposable podLockDisposable,
        ISyncService syncService)
    {
        _requestingClientId = requestingClientId;
        _podModel = podModel;
        _radioConnection = radioConnection;
        _podLockDisposable = podLockDisposable;
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
        Debug.WriteLine("Prime started");
        PodRequestStatus result;
        if (_podModel.VersionModel == null) // radio address not set (no recorded version response)
        {
            Debug.WriteLine("Version Info missing, assigning address");
            result = await SendRequestAsync(false,
                cancellationToken,
                new AssignAddressMessage
                {
                    Address = _podModel.RadioAddress
                },
                true
            );
            if (result != PodRequestStatus.Executed)
                return result;
            Debug.WriteLine("Assignment done");
        }

        if (_podModel.VersionModel == null) // must be set now
            return PodRequestStatus.RejectedByApp;

        Debug.WriteLine("Version info verified");

        if (_podModel.ActivationParametersModel == null) // actpar null, no long version info received
        {
            Debug.WriteLine("Act parameters missing, setting clock");
            result = await SendRequestAsync(false, cancellationToken,
                new SetClockMessage
                {
                    RadioAddress = _podModel.RadioAddress,
                    Date = podDate,
                    Time = podTime,
                    PacketTimeout = 50,
                    Lot = _podModel.VersionModel.Lot,
                    Serial = _podModel.VersionModel.Serial
                },
                true);

            if (result != PodRequestStatus.Executed)
                return result;
            Debug.WriteLine("Set clock done");
        }

        if (_podModel.ActivationParametersModel == null) // must be set now
            return PodRequestStatus.RejectedByPod;

        Debug.WriteLine("Act param verified");
        if (_podModel.ProgressModel == null) // must be set now
            return PodRequestStatus.RejectedByPod;

        Debug.WriteLine("Progress exists");
        if (_podModel.ProgressModel.Progress > PodProgress.Priming)
            return PodRequestStatus.RejectedByPod;

        Debug.WriteLine("Progress exists");
        if (_podModel.ProgressModel.Progress == PodProgress.Paired)
        {
            Debug.WriteLine("Progress paired");

            if (_podModel.StatusModel == null) // config alerts not run if no status obtained so far
            {
                Debug.WriteLine("Config alerts not run, running");
                result = await SendRequestAsync(false, cancellationToken,
                    new SetAlertsMessage
                    {
                        AlertConfigurations = new[]
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
                        }
                    });

                if (result != PodRequestStatus.Executed)
                    return result;

                Debug.WriteLine("Config alerts done");
                if (relaxDeliveryCrosschecks) // note: setdeliveryflags will be skipped if configurealerts failed
                {
                    Debug.WriteLine("Relax checks running");
                    result = await SendRequestAsync(false, cancellationToken,
                        new SetDeliveryVerificationMessage
                        {
                            VerificationFlag0 = 0,
                            VerificationFlag1 = 0
                        });
                    if (result != PodRequestStatus.Executed)
                        return result;
                    Debug.WriteLine("Relax checks did run");
                }
            }

            Debug.WriteLine("Starting prime bolus");

            result = await SendRequestAsync(false, cancellationToken,
                new StartBolusMesage
                {
                    ImmediatePulseCount = 52,
                    ImmediatePulseIntervalMilliseconds = 1000,
                    ExtendedPulseCount = 0,
                    ExtendedHalfHourCount = 0
                });
            if (result != PodRequestStatus.Executed)
                return result;

            Debug.WriteLine("Started prime bolus");

            Debug.Assert(_podModel.StatusModel != null);
            if (!_podModel.StatusModel.ImmediateBolusActive ||
                _podModel.ProgressModel.Progress != PodProgress.Priming)
                return PodRequestStatus.RejectedByApp;

            Debug.WriteLine("Awaiting prime bolus completion 55 seconds");
            await Task.Delay(TimeSpan.FromSeconds(55));
        }

        Debug.WriteLine("Update status");
        result = await UpdateStatus(cancellationToken);
        if (result != PodRequestStatus.Executed)
            return result;
        Debug.WriteLine("Updated");

        Debug.Assert(_podModel.StatusModel != null);

        if (_podModel.ProgressModel.Progress == PodProgress.Priming)
        {
            Debug.WriteLine($"Still priming, waiting {_podModel.StatusModel.PulsesPending + 3} seconds");
            await Task.Delay(TimeSpan.FromSeconds(_podModel.StatusModel.PulsesPending + 3));
            Debug.WriteLine("Updating status");
            result = await UpdateStatus(cancellationToken);
            if (result != PodRequestStatus.Executed)
                return result;
            Debug.WriteLine("Updated");
        }

        if (_podModel.ProgressModel.Progress != PodProgress.Primed)
            return PodRequestStatus.RejectedByApp;

        Debug.WriteLine("Prime complete");
        return PodRequestStatus.Executed;
    }

    public async Task<PodRequestStatus> StartPodAsync(
        TimeOnly podTime,
        int[] pulsesPerHour48HalfHours,
        CancellationToken cancellationToken = default)
    {
        if (_podModel.ProgressModel == null)
            return PodRequestStatus.RejectedByApp;

        if (_podModel.ProgressModel.Faulted)
            return PodRequestStatus.RejectedByApp;

        if (_podModel.ProgressModel.Progress < PodProgress.Primed)
            return PodRequestStatus.RejectedByApp;
        if (_podModel.ProgressModel.Progress >= PodProgress.Running)
            return PodRequestStatus.RejectedByApp;

        PodRequestStatus result;
        if (_podModel.ProgressModel.Progress == PodProgress.Primed)
        {
            Debug.WriteLine("Pod is primed, setting basal");

            result = await SendRequestAsync(false, cancellationToken,
                new StartBasalMessage
                {
                    PulsesPerHour48HalfHours = pulsesPerHour48HalfHours
                });
            if (result != PodRequestStatus.Executed)
                return result;
            if (_podModel.ProgressModel.Progress != PodProgress.BasalSet)
                return PodRequestStatus.RejectedByApp;
            Debug.WriteLine("basal set");
        }

        if (_podModel.ProgressModel.Progress == PodProgress.BasalSet)
        {
            Debug.WriteLine("configuring alerts");
            result = await SendRequestAsync(false, cancellationToken,
                new SetAlertsMessage
                {
                    AlertConfigurations = new[]
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
                    }
                });

            if (result != PodRequestStatus.Executed)
                return result;

            Debug.WriteLine("configured, inserting");

            result = await SendRequestAsync(false, cancellationToken,
                new StartBolusMesage
                {
                    ImmediatePulseCount = 10,
                    ImmediatePulseIntervalMilliseconds = 1000,
                    ExtendedPulseCount = 0,
                    ExtendedHalfHourCount = 0
                });
            if (result != PodRequestStatus.Executed)
                return result;

            if (_podModel.ProgressModel.Progress != PodProgress.Inserting)
                return PodRequestStatus.RejectedByApp;
            Debug.WriteLine("insertion confirmed, waiting");

            await Task.Delay(TimeSpan.FromSeconds(13));

            Debug.WriteLine("update status");
            result = await UpdateStatus(cancellationToken);
            if (result != PodRequestStatus.Executed)
                return result;
            Debug.WriteLine("updated");
        }

        Debug.Assert(_podModel.StatusModel != null);

        if (_podModel.ProgressModel.Progress == PodProgress.Inserting)
        {
            Debug.WriteLine("waiting for insert to complete");

            await Task.Delay(TimeSpan.FromSeconds(_podModel.StatusModel.PulsesPending + 2));
            result = await UpdateStatus(cancellationToken);
            if (result != PodRequestStatus.Executed)
                return result;
            Debug.WriteLine("wait finished");
        }

        if (_podModel.ProgressModel.Progress != PodProgress.Running)
            return PodRequestStatus.RejectedByApp;
        Debug.WriteLine("all donesies");
        return PodRequestStatus.Executed;
    }


    public async Task<PodRequestStatus> SetBasalSchedule(
        TimeOnly podTime,
        int[] pulsesPerHour48HalfHours,
        CancellationToken cancellationToken = default)
    {
        var result = await UpdateStatus(cancellationToken);
        if (result != PodRequestStatus.Executed)
            return result;
        Debug.Assert(_podModel.ProgressModel != null);
        Debug.Assert(_podModel.StatusModel != null);

        if (_podModel.ProgressModel.Progress < PodProgress.Primed
            || _podModel.ProgressModel.Progress > PodProgress.RunningLow
            || _podModel.ProgressModel.Faulted
            || _podModel.StatusModel.ImmediateBolusActive)
            return PodRequestStatus.RejectedByApp;

        if (_podModel.StatusModel.BasalActive)
        {
            result = await SendRequestAsync(false,
                cancellationToken,
                new StopDeliveryMessage { StopBasal = true });

            if (result != PodRequestStatus.Executed)
                return result;
        }

        if (_podModel.StatusModel.BasalActive)
            return PodRequestStatus.RejectedByApp;

        return await SendRequestAsync(false,
            cancellationToken,
            new StartBasalMessage
            {
                PodTime = podTime,
                PulsesPerHour48HalfHours = pulsesPerHour48HalfHours
            });
    }

    public async Task<PodRequestStatus> AcknowledgeAlerts(CancellationToken cancellationToken = default)
    {
        var result = await UpdateStatus(cancellationToken);
        if (result != PodRequestStatus.Executed)
            return result;
        Debug.Assert(_podModel.ProgressModel != null);
        Debug.Assert(_podModel.StatusModel != null);

        if (_podModel.ProgressModel.Progress < PodProgress.Paired
            || _podModel.ProgressModel.Progress >= PodProgress.Faulted
            || _podModel.ProgressModel.Faulted
            || _podModel.StatusModel.ImmediateBolusActive)
            return PodRequestStatus.RejectedByApp;

        var md = new AcknowledgeAlertsMessage();
        for (var i = 0; i < 8; i++)
            md.AlertIndices[i] = true;

        return await SendRequestAsync(false,
            cancellationToken,
            md);
    }

    public async Task<PodRequestStatus> UpdateStatus(CancellationToken cancellationToken = default)
    {
        if (_podModel.VersionModel == null)
            return PodRequestStatus.RejectedByApp;

        if (_podModel.ProgressModel != null)
            if (_podModel.ProgressModel.Progress < PodProgress.Paired
                || _podModel.ProgressModel.Progress >= PodProgress.Deactivated)
                return PodRequestStatus.RejectedByApp;

        return await SendRequestAsync(false,
            cancellationToken,
            new GetStatusMessage { StatusType = PodStatusType.Compact }
        );
    }

    public async Task<PodRequestStatus> ConfigureAlerts(
        AlertConfiguration[] alertConfigurations,
        CancellationToken cancellationToken = default)
    {
        var result = await UpdateStatus(cancellationToken);
        if (result != PodRequestStatus.Executed)
            return result;
        Debug.Assert(_podModel.ProgressModel != null);
        Debug.Assert(_podModel.StatusModel != null);

        cancellationToken.ThrowIfCancellationRequested();

        if (_podModel.ProgressModel.Progress < PodProgress.Paired
            || _podModel.ProgressModel.Progress >= PodProgress.Faulted
            || _podModel.ProgressModel.Faulted
            || _podModel.StatusModel.ImmediateBolusActive)
            return PodRequestStatus.RejectedByApp;

        result = await SendRequestAsync(false,
            cancellationToken,
            new SetAlertsMessage { AlertConfigurations = alertConfigurations }
        );

        return result;
    }

    public async Task<PodRequestStatus> Beep(BeepType type, CancellationToken cancellationToken = default)
    {
        var result = await UpdateStatus(cancellationToken);
        if (result != PodRequestStatus.Executed)
            return result;
        Debug.Assert(_podModel.ProgressModel != null);
        Debug.Assert(_podModel.StatusModel != null);

        cancellationToken.ThrowIfCancellationRequested();

        if (_podModel.ProgressModel.Progress < PodProgress.Paired
            || _podModel.ProgressModel.Progress >= PodProgress.Faulted
            || _podModel.ProgressModel.Faulted
            || _podModel.StatusModel.ImmediateBolusActive)
            return PodRequestStatus.RejectedByApp;

        return await SendRequestAsync(false,
            cancellationToken,
            new SetBeepingMessage { BeepNow = type }
        );
    }

    public async Task<PodRequestStatus> CancelTempBasal(CancellationToken cancellationToken = default)
    {
        var result = await UpdateStatus(cancellationToken);
        if (result != PodRequestStatus.Executed)
            return result;
        Debug.Assert(_podModel.ProgressModel != null);
        Debug.Assert(_podModel.StatusModel != null);

        cancellationToken.ThrowIfCancellationRequested();

        if (_podModel.ProgressModel.Progress < PodProgress.Running
            || _podModel.ProgressModel.Faulted
            || _podModel.ProgressModel.Progress >= PodProgress.Faulted
            || _podModel.StatusModel.ImmediateBolusActive
            || !_podModel.StatusModel.TempBasalActive)
            return PodRequestStatus.RejectedByApp;

        return await SendRequestAsync(
            false,
            cancellationToken,
            new StopDeliveryMessage { StopTempBasal = true });
    }

    public async Task<PodRequestStatus> SetTempBasal(
        int pulsesPerHour,
        int halfHourCount,
        CancellationToken cancellationToken = default)
    {
        if (halfHourCount < 0 || halfHourCount > 24 || pulsesPerHour < 0 || pulsesPerHour > 1800)
            return PodRequestStatus.RejectedByApp;

        var result = await UpdateStatus(cancellationToken);
        if (result != PodRequestStatus.Executed)
            return result;
        Debug.Assert(_podModel.ProgressModel != null);
        Debug.Assert(_podModel.StatusModel != null);

        cancellationToken.ThrowIfCancellationRequested();

        if (_podModel.ProgressModel.Progress < PodProgress.Running
            || _podModel.ProgressModel.Faulted
            || _podModel.ProgressModel.Progress >= PodProgress.Faulted
            || _podModel.StatusModel.ImmediateBolusActive)
            return PodRequestStatus.RejectedByApp;

        if (_podModel.StatusModel.TempBasalActive)
        {
            result = await SendRequestAsync(
                false,
                cancellationToken,
                new StopDeliveryMessage { StopTempBasal = true });

            cancellationToken.ThrowIfCancellationRequested();

            if (result != PodRequestStatus.Executed)
                return result;
        }

        if (_podModel.ProgressModel.Progress < PodProgress.Running
            || _podModel.ProgressModel.Faulted
            || _podModel.ProgressModel.Progress >= PodProgress.Faulted)
            return PodRequestStatus.RejectedByApp;

        return await SendRequestAsync(
            false,
            cancellationToken,
            new StartTempBasalMessage
            {
                HalfHourCount = halfHourCount,
                PulsesPerHour = pulsesPerHour
            });
    }

    public async Task<PodRequestStatus> Bolus(
        int bolusPulses,
        int pulseIntervalMilliseconds,
        bool special = false,
        CancellationToken cancellationToken = default)
    {
        if (pulseIntervalMilliseconds < 2000 || bolusPulses < 0 || bolusPulses > 1800000 / pulseIntervalMilliseconds)
            return PodRequestStatus.RejectedByApp;

        var result = await UpdateStatus(cancellationToken);
        if (result != PodRequestStatus.Executed)
            return result;
        Debug.Assert(_podModel.ProgressModel != null);
        Debug.Assert(_podModel.StatusModel != null);

        cancellationToken.ThrowIfCancellationRequested();

        if (_podModel.ProgressModel.Progress < PodProgress.Running
            || _podModel.ProgressModel.Faulted
            || _podModel.ProgressModel.Progress >= PodProgress.Faulted
            || _podModel.StatusModel.ImmediateBolusActive)
            return PodRequestStatus.RejectedByApp;

        return await SendRequestAsync(
            false,
            cancellationToken,
            new StartBolusMesage
            {
                ImmediatePulseCount = bolusPulses,
                ImmediatePulseIntervalMilliseconds = pulseIntervalMilliseconds,
                SpecialBolus = special
            });
    }

    public async Task<PodRequestStatus> CancelBolus(CancellationToken cancellationToken = default)
    {
        var result = await UpdateStatus(cancellationToken);
        if (result != PodRequestStatus.Executed)
            return result;
        Debug.Assert(_podModel.ProgressModel != null);
        Debug.Assert(_podModel.StatusModel != null);

        cancellationToken.ThrowIfCancellationRequested();

        if (_podModel.ProgressModel.Progress < PodProgress.Running
            || _podModel.ProgressModel.Faulted
            || _podModel.ProgressModel.Progress >= PodProgress.Faulted
            || !_podModel.StatusModel.ImmediateBolusActive)
            return PodRequestStatus.RejectedByApp;


        return await SendRequestAsync(
            false,
            cancellationToken,
            new StopDeliveryMessage
            {
                StopBolus = true
            });
    }


    public async Task<PodRequestStatus> Deactivate(CancellationToken cancellationToken = default)
    {
        var result = await UpdateStatus(cancellationToken);
        if (result != PodRequestStatus.Executed)
            return result;
        Debug.Assert(_podModel.ProgressModel != null);
        Debug.Assert(_podModel.StatusModel != null);

        cancellationToken.ThrowIfCancellationRequested();

        if (_podModel.ProgressModel.Progress < PodProgress.Running
            || _podModel.ProgressModel.Progress >= PodProgress.Deactivated)
            return PodRequestStatus.RejectedByApp;

        if (_podModel.ProgressModel.Progress <= PodProgress.Faulted
            && !_podModel.ProgressModel.Faulted)
            if (_podModel.StatusModel.BasalActive
                || _podModel.StatusModel.TempBasalActive
                || _podModel.StatusModel.ImmediateBolusActive
                || _podModel.StatusModel.ExtendedBolusActive)
            {
                result = await SendRequestAsync(
                    false,
                    cancellationToken,
                    new StopDeliveryMessage
                    {
                        StopBasal = _podModel.StatusModel.BasalActive,
                        StopTempBasal = _podModel.StatusModel.TempBasalActive,
                        StopBolus = _podModel.StatusModel.ImmediateBolusActive,
                        StopExtendedBolus = _podModel.StatusModel.ExtendedBolusActive
                    });
                if (result != PodRequestStatus.Executed)
                    return result;
            }

        return await SendRequestAsync(
            false,
            cancellationToken,
            new DeactivateMessage()
        );
    }

    private async Task<PodRequestStatus> SendRequestAsync(
        bool critical,
        CancellationToken cancellationToken,
        IMessageData messageData,
        bool broadcast = false,
        int authRetries = 0,
        int syncRetries = 0)
    {
        //var initialPacketSequence = _podModel.NextPacketSequence;
        var initialMessageSequence = _podModel.NextMessageSequence;

        var mb = new MessageBuilder()
            .WithSequence(_podModel.NextMessageSequence)
            .WithAddress(broadcast ? 0xFFFFFFFF : _podModel.RadioAddress);
        if (critical)
            mb.AsCritical();
        if (_podModel.NonceProvider != null)
            mb.WithNonceProvider(_podModel.NonceProvider);

        var messageToSend = mb.Build(messageData);

        var er = await RunExchangeAsync(
            messageToSend,
            cancellationToken);

        if (er.ReceivedMessage != null && er.RequestSentLatest.HasValue)
            _podModel.ProcessReceivedMessage(er.ReceivedMessage, er.RequestSentLatest.Value);

        await using (var ocdb = new OcdbContext())
        {
            await ocdb.PodActions.AddAsync(new PodAction
            {
                PodId = _podModel.Id,
                Index = _podModel.NextRecordIndex,
                ClientId = _requestingClientId,
                ReceivedData = er.ReceivedMessage?.Body.ToArray(),
                SentData = er.SentMessage?.Body.ToArray(),
                Result = er.Result,
                RequestSentEarliest = er.RequestSentEarliest,
                RequestSentLatest = er.RequestSentLatest,
                IsSynced = false
            });
            await ocdb.SaveChangesAsync(cancellationToken);
        }

        _syncService.TriggerSync();

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
                if (er.ReceivedMessage?.Data is NonceSyncMessage nsm && authRetries < 2 &&
                    _podModel.NonceProvider != null)
                {
                    _podModel.NextMessageSequence = initialMessageSequence;
                    _podModel.NonceProvider.SyncNonce(nsm.SyncWord, initialMessageSequence);
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

        cancellationToken.ThrowIfCancellationRequested();

        // retry
        return await SendRequestAsync(
            critical,
            cancellationToken,
            messageData,
            broadcast,
            authRetries,
            syncRetries
        );
    }

    private async Task<ExchangeResult> RunExchangeAsync(
        IPodMessage messageToSend,
        CancellationToken cancellationToken = default
    )
    {
        var messageBody = messageToSend.Body;
        var sendPacketCount = messageBody.Length / 31 + 1;
        var sendPacketIndex = 0;

        var nextPacketSequence = _podModel.NextPacketSequence;

        DateTimeOffset? firstPacketSent = null;
        DateTimeOffset? lastPacketReceived = null;

        //var packetAddressIn = _podModel.ProgressModel?.Progress >= PodProgress.Paired ? _podModel.RadioAddress : 0xFFFFFFFF;
        //var packetAddressOut = _podModel.ProgressModel?.Progress >= PodProgress.Paired ? _podModel.RadioAddress : 0xFFFFFFFF;
        var packetAddressIn = messageToSend.Address;
        var packetAddressOut = messageToSend.Address;
        var ackDataOutInterim = _podModel.RadioAddress;
        var ackDataIn = _podModel.RadioAddress;

        var er = new ExchangeResult { Result = AcceptanceType.Ignored };
        IPodPacket? podFirstResponsePacket = null;
        // Send
        while (sendPacketIndex < sendPacketCount)
        {
            var isLastPacket = sendPacketIndex == sendPacketCount - 1;
            var byteStart = sendPacketIndex * 31;
            var byteEnd = byteStart + 31;
            if (messageBody.Length < byteEnd)
                byteEnd = messageBody.Length;

            var packetToSend = new PodPacket(
                packetAddressOut,
                sendPacketIndex == 0 ? PodPacketType.Pdm : PodPacketType.Con,
                nextPacketSequence,
                messageBody.Sub(byteStart, byteEnd));

            cancellationToken.ThrowIfCancellationRequested();

            var pe = await TryExchangePackets(packetToSend, isLastPacket ? CancellationToken.None : cancellationToken);
            var receivedPacket = PodPacket.FromExchangeResult(pe, packetAddressIn);
            var packetCommandSent = pe.CommunicationResult != BleCommunicationResult.WriteFailed;
            var bleConnectionFailed = pe.CommunicationResult != BleCommunicationResult.OK;

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
                    return er.WithResult(null, "Timed out"); // out
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
                    return er.WithResult(AcceptanceType.RejectedProtocolError,
                        "Pod didn't respond with expected packet type");

                if (receivedPacket.Data.DWord(0) != ackDataIn)
                    return er.WithResult(AcceptanceType.RejectedProtocolError,
                        "Pod didn't respond with expected ack data");
            }

            sendPacketIndex++;
        } // send loop

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

        var podResponsePackets = new List<IPodPacket> { podFirstResponsePacket };

        while (receivedMessageLength < responseMessageLength)
        {
            var interimAck = new PodPacket(
                packetAddressOut,
                PodPacketType.Ack,
                nextPacketSequence,
                new Bytes(ackDataOutInterim)
            );

            var pe = await TryExchangePackets(interimAck, CancellationToken.None);
            var receivedPacket = PodPacket.FromExchangeResult(pe, packetAddressIn);

            if (receivedPacket == null)
            {
                if (_podModel.LastRadioPacketReceived < DateTimeOffset.UtcNow - TimeSpan.FromSeconds(30))
                {
                    er.ErrorText = "Pod response partially received due to timeout";
                    break; // timed out, exit receive loop
                }

                continue; // not timed out yet, continue receive loop
            }

            Debug.Assert(receivedPacket != null);
            _podModel.LastRadioPacketReceived = DateTimeOffset.UtcNow;
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
        var receivedMessageBody = new Bytes();
        foreach (var packet in podResponsePackets) receivedMessageBody.Append(packet.Data);

        IPodMessage? podMessageReceived = null;
        try
        {
            podMessageReceived = new MessageBuilder().Build(receivedMessageBody);
            er.ReceivedMessage = podMessageReceived;
            _podModel.NextMessageSequence = (receivedMessageSequence + 1) % 16;
        }
        catch (Exception ex)
        {
        }

        er.Result = AcceptanceType.Inconclusive;
        var firstPartMessageType = (PodMessagePartType)podFirstResponseData[0];
        switch (firstPartMessageType)
        {
            case PodMessagePartType.ResponseStatus:
            case PodMessagePartType.ResponseVersionInfo:
                er.Result = podMessageReceived == null ? AcceptanceType.Inconclusive : AcceptanceType.Accepted;
                break;
            case PodMessagePartType.ResponseInfo:
                if (podFirstResponseData.Length < 3)
                {
                    er.Result = AcceptanceType.Inconclusive;
                }
                else
                {
                    if (podFirstResponseData[2] == (byte)PodStatusType.Extended)
                    {
                        if (podFirstResponseData.Length < 10)
                        {
                            er.Result = AcceptanceType.Inconclusive;
                        }
                        else
                        {
                            if (messageToSend.Data is GetStatusMessage gsm &&
                                (gsm.StatusType == PodStatusType.Extended || gsm.StatusType == PodStatusType.Compact))
                                er.Result = podMessageReceived == null
                                    ? AcceptanceType.Inconclusive
                                    : AcceptanceType.Accepted;
                            else
                                er.Result = podFirstResponseData[3] == (byte)PodProgress.Deactivated
                                    ? AcceptanceType.Accepted
                                    : AcceptanceType.RejectedFaultOccured;
                        }
                    }
                    else
                    {
                        er.Result = AcceptanceType.Accepted;
                    }
                }

                break;
            case PodMessagePartType.ResponseError:
                if (podFirstResponseData.Length < 3)
                    er.Result = AcceptanceType.Inconclusive;
                else if (podFirstResponseData[2] == 0x14)
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
        bool broadcast = false,
        CancellationToken cancellationToken = default)
    {
        var ackDataOutFinal = broadcast ? _podModel.RadioAddress : 0x00000000;
        var finalAck = new PodPacket(
            broadcast ? 0xFFFFFFFF : _podModel.RadioAddress,
            PodPacketType.Ack,
            _podModel.NextPacketSequence,
            new Bytes(ackDataOutFinal));

        DateTimeOffset? successfulSendWithoutResponse = null;
        var sendFinalAck = true;
        var lastHeard = DateTimeOffset.UtcNow;
        while (DateTimeOffset.UtcNow - lastHeard < TimeSpan.FromSeconds(3))
        {
            BleExchangeResult ber;
            if (sendFinalAck)
            {
                Debug.WriteLine("Final ack sending and waiting");
                ber = await _radioConnection.SendAndTryGetPacket(
                    0, 1, 50, 0,
                    0, 310, 0, finalAck, cancellationToken);
            }
            else
            {
                Debug.WriteLine("Final ack waiting");
                ber = await _radioConnection.TryGetPacket(
                    0, 540);
            }

            var receivedPacket = PodPacket.FromExchangeResult(ber);
            if (receivedPacket != null)
            {
                sendFinalAck = true;
                lastHeard = DateTimeOffset.UtcNow;
            }
            else
            {
                sendFinalAck = false;
            }
        }

        Debug.WriteLine("Final send complete");
        _podModel.LastRadioPacketReceived = null;
        _podModel.NextPacketSequence = (_podModel.NextPacketSequence + 1) % 32;
    }

    private async Task<BleExchangeResult> TryExchangePackets(
        IPodPacket packetToSend,
        CancellationToken cancellationToken)
    {
        BleExchangeResult? result = null;
        PodPacket? received = null;
        if (!_podModel.LastRadioPacketReceived.HasValue ||
            _podModel.LastRadioPacketReceived < DateTimeOffset.UtcNow - TimeSpan.FromSeconds(30))
        {
            Debug.WriteLine($"SEND: {packetToSend} with preamble");
            result = await _radioConnection.SendAndTryGetPacket(
                0, 0, 0, 25,
                0, 60, 20, packetToSend, cancellationToken);
            received = PodPacket.FromExchangeResult(result);
        }

        else
            //if (received == null)
        {
            Debug.WriteLine($"SEND: {packetToSend} no preamble");
            result = await _radioConnection.SendAndTryGetPacket(
                0, 0, 0, 0,
                0, 95, 10, packetToSend, cancellationToken);
            received = PodPacket.FromExchangeResult(result);
        }

        Debug.WriteLine($"RCVD: {received}");
        return result!;
    }
}
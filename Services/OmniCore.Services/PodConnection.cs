using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Services;

public class PodConnection : IDisposable
{
    private Pod _pod;
    private IDisposable _podLockDisposable;
    private RadioConnection _radioConnection;

    public PodConnection(Pod pod,
        RadioConnection radioConnection,
        IDisposable podLockDisposable)
    {
        _pod = pod;
        _radioConnection = radioConnection;
        _podLockDisposable = podLockDisposable;
    }
    public void Dispose()
    {
        _podLockDisposable?.Dispose();
    }
    
    public async Task UpdateStatus(CancellationToken cancellationToken = default)
    {
        var me = new MessageExchange(
            new RadioMessage
            {
                Address = _pod.RadioAddress,
                Sequence = _pod.NextMessageSequence,
                WithCriticalFollowup = false,
                Parts = new List<RadioMessagePart>()
                {
                    new RequestStatusPart(RequestStatusType.Default)
                }
            },
            _radioConnection,
            _pod.NextPacketSequence);

        Debug.WriteLine($"ema sending message");
        var result = await me.RunExchangeAsync(cancellationToken);
        _pod.NextMessageSequence = result.NextMessageSequence;
        _pod.NextPacketSequence = result.NextPacketSequence;
        Debug.WriteLine($"ema next msgseq: {result.NextMessageSequence} pktseq: {result.NextPacketSequence}\nResponse: {result.Response}");
    }

    public async Task Beep(BeepType type, CancellationToken cancellationToken = default)
    {
        var me = new MessageExchange(
            new RadioMessage
            {
                Address = _pod.RadioAddress,
                Sequence = _pod.NextMessageSequence,
                WithCriticalFollowup = false,
                Parts = new List<RadioMessagePart>()
                {
                    new RequestBeepConfigPart(type,
                        false, false, 0,
                        false, false, 0,
                        false, false, 0)
                }
            },
            _radioConnection,
            _pod.NextPacketSequence);

        Debug.WriteLine($"ema sending message");
        var result = await me.RunExchangeAsync(cancellationToken);
        _pod.NextMessageSequence = result.NextMessageSequence;
        _pod.NextPacketSequence = result.NextPacketSequence;
        Debug.WriteLine($"ema next msgseq: {result.NextMessageSequence} pktseq: {result.NextPacketSequence}\nResponse: {result.Response}");
    }

    public async Task Suspend(CancellationToken cancellationToken = default)
    {
        var me = new MessageExchange(
            new RadioMessage
            {
                Address = _pod.RadioAddress,
                Sequence = _pod.NextMessageSequence,
                WithCriticalFollowup = false,
                Parts = new List<RadioMessagePart>()
                {
                    new RequestCancelPart(_pod.NextNonce(),
                        BeepType.NoSound,
                        false,
                        false,
                        true)
                }
            },
            _radioConnection,
            _pod.NextPacketSequence);

        Debug.WriteLine($"ema sending message");
        var result = await me.RunExchangeAsync(cancellationToken);
        Debug.WriteLine($"Response: {result.Response}");

        _pod.NextPacketSequence = result.NextPacketSequence;
        if (result.Response.Parts[0] is ResponseErrorPart)
        {
            var rep = (ResponseErrorPart)result.Response.Parts[0];
            me = new MessageExchange(
                new RadioMessage
                {
                    Address = _pod.RadioAddress,
                    Sequence = _pod.NextMessageSequence,
                    WithCriticalFollowup = false,
                    Parts = new List<RadioMessagePart>()
                    {
                        new RequestCancelPart(_pod.NextNonce(rep.ErrorValue, _pod.NextMessageSequence),
                            BeepType.NoSound,
                            false,
                            false,
                            true)
                    }
                },
                _radioConnection,
                _pod.NextPacketSequence);
            Debug.WriteLine($"ema resending message");
            result = await me.RunExchangeAsync(cancellationToken);
            Debug.WriteLine($"Response: {result.Response}");
        }
        
        _pod.NextMessageSequence = result.NextMessageSequence;
        Debug.WriteLine($"ema next msgseq: {result.NextMessageSequence} pktseq: {result.NextPacketSequence}");
    }

}
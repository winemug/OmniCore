using OmniCore.Services;
using OmniCore.Services.Interfaces.Pod;
using OmniCore.Services.Interfaces.Radio;

namespace OmniCore.Tests;

public class MockRadioConnection : IRadioConnection
{
    private IPodMessage _receivedMessage;
    private IPodMessage _sendMessage;
    private IPodModel _podModel;
    private int _btCommDelay = 375;

    private List<IPodPacket> _receivedPackets;
    public MockRadioConnection(IPodModel podModel)
    {
        _podModel = podModel;
        _receivedPackets = new List<IPodPacket>();
    }
    
    public void Dispose()
    {
    }

    public Task<IPodPacket> TryGetPacket(byte channel, uint timeoutMs, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IPodPacket>(null);
    }

    public Task<bool> SendPacket(byte channel, byte repeatCount, ushort delayMilliseconds, ushort preambleExtensionMs, IPodPacket packet,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public async Task<IPodPacket> SendAndTryGetPacket(byte sendChannel, byte sendRepeatCount, ushort sendRepeatDelayMs,
        ushort sendPreambleExtensionMs, byte listenChannel, uint listenTimeoutMs, byte listenRetryCount, IPodPacket packet,
        CancellationToken cancellationToken)
    {
        await Task.Delay((sendRepeatCount+1) * (sendRepeatDelayMs + sendPreambleExtensionMs) + _btCommDelay);
        Console.WriteLine($"Pod received packet: {packet}");

        IPodPacket sendPacket = null;
        if (packet.Address == _podModel.RadioAddress || packet.Address == 0xFFFFFFFF)
        {
            _receivedPackets.Add(packet);
            _receivedMessage = PodMessage.FromReceivedPackets(_receivedPackets);
        }
        
        if (sendPacket == null)
            await Task.Delay((int)(listenTimeoutMs * (listenRetryCount+1)));
        else
            await Task.Delay(_btCommDelay + new Random().Next(25, 125));

        Console.WriteLine($"Pod sends packet: {sendPacket}");
        return sendPacket;
    }
}

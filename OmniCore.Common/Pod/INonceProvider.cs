namespace OmniCore.Common.Pod;

public interface INonceProvider
{
    uint NextNonce();
    void SyncNonce(ushort syncWord, int syncMessageSequence);
}
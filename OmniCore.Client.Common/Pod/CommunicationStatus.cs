namespace OmniCore.Common.Pod;

public enum CommunicationStatus
{
    NoResponse,
    ConnectionInterrupted,
    MessageSyncRequired,
    ProtocolError
}


// Pod not responded
// Pod responded but weirdly

// RequestSentWithNoResponse
// RequestPartialWithNoResponse
// RequestPartialWithResponse
// RequestSendInconclusive
// ReceivedAndAccepted response
// 

// Request unsent
// Request partially sent
// Request fully sent


// RequestAccepted



// ConnectionEvents

// BLE connect success
// BLE init success
// BLE init failed

// BLE read success
// BLE write success

// BLE connect failed
// BLE read failed
// BLE write failed


// POD sent partial request
// POD received partial OK
// POD sent full request
// POD received nothing
// POD received sync request
// POD received partial response
// POD sent partial OK
// POD received full response
// POD sent full OK


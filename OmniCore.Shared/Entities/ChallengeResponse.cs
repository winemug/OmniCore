namespace OmniCore.Shared.Entities;

public class ChallengeResponse
{
    public Guid RequestId { get; set; }
    public string VerificationCode { get; set; }
}
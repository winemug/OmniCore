using System;

namespace OmniCore.Services.Entities
{
    public class ChallengeResponse
    {
        public Guid RequestId { get; set; }
        public string VerificationCode { get; set; }
    }
}
namespace OmniCore.Shared.Api;

public record AccountVerificationRequest
{
    public string Email { get; init; }
    public string Password { get; init; }
    public string Code { get; init; }
}
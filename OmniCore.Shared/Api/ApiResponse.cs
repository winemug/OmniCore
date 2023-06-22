namespace OmniCore.Shared.Api;

public record ApiResponse
{
    public bool Success { get; init; }
    public string Message { get; init; }
}
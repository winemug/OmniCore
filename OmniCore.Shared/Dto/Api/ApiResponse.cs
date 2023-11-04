namespace OmniCore.Shared.Dto.Api;

public record ApiResponse
{
    public bool Success { get; init; }
    public string Message { get; init; }
}
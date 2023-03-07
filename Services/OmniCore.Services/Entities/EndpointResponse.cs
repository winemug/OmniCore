namespace OmniCore.Services.Entities;

public class EndpointResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string Dsn { get; set; }
    public string Queue { get; set; }
    public string Exchange { get; set; }
    public string UserId { get; set; }
}
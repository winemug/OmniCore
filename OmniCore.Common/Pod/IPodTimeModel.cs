namespace OmniCore.Common.Pod;

public interface IPodTimeModel
{
    public DateTimeOffset ValueWhen { get; set; }
    public TimeOnly Value { get; set; }
    public TimeOnly Now { get; set; }
    public TimeOnly Then(DateTimeOffset when);
}
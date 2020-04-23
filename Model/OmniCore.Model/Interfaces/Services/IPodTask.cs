namespace OmniCore.Model.Interfaces.Services
{
    public interface IPodTask : ITask
    {
        IPodTask WithRequest(IPodRequest request);
        IPodRequest Request { get; }
        IPodResponse Response { get; }
    }
}
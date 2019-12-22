using OmniCore.Model.Interfaces.Data;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreDataServices
    {
        IPodService OmnipodErosService { get; }
        IRadioService RileyLinkRadioService { get; }
        IRepositoryService RepositoryService { get; }
        // IUserService UserService { get; }
    }
}
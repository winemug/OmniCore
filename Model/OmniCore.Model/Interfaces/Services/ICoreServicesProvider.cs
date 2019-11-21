using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreServicesProvider
    {
        Task<ICoreServices> GetLocalServices();
        Task<ICoreServices> GetRemoteServices(ICoreServicesDescriptor serviceDescriptor, ICoreCredentials credentials);
        Task<IAsyncEnumerable<ICoreServicesDescriptor>> ListNearServices();
    }
}

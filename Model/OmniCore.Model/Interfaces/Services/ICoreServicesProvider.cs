using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services
{
    public interface ICoreServicesProvider
    {
        ICoreServices LocalServices { get; }
        Task<ICoreServices> GetRemoteServices(ICoreServicesDescriptor serviceDescriptor, ICoreCredentials credentials);
        Task<IAsyncEnumerable<ICoreServicesDescriptor>> ListRemoteServices();
    }
}

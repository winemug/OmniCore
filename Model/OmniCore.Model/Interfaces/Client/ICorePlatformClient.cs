using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces.Client
{
    public interface ICorePlatformClient : IClientResolvable
    {
        Task AttachToService(Type concreteType, ICoreClientConnection connection);
        Task DetachFromService(ICoreClientConnection connection);
        SynchronizationContext SynchronizationContext { get; }
    }
}
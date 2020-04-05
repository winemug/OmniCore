using System;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IIntegrationComponent : IDisposable, IServiceComponent, IServerResolvable
    {
    }
}
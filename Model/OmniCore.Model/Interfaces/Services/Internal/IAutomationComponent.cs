using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IAutomationComponent : IDisposable, IServiceComponent, IServerResolvable
    {
    }
}

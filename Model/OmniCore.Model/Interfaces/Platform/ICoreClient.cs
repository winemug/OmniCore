using System;
using System.Threading;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface ICoreClient
    {
        ICoreServices CoreServices { get; set; }
        ICoreContainer Container { get; }
        SynchronizationContext SynchronizationContext { get; }
    }
}
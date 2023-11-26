using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Abstractions.Services;
public interface ICoreService
{
    IDisposable ForegroundActivityContext();
    Task InteractiveStartAsync();
    Task OnCreatedAsync();
    Task OnActivatedAsync();
    Task OnDeactivatedAsync();
    Task OnStoppedAsync();
    Task OnResumedAsync();
    Task OnDestroyingAsync();
}

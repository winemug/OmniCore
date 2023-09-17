using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Interfaces.Services;
public interface ICoreService
{
    ValueTask OnCreatedAsync();
    ValueTask OnActivatedAsync();
    ValueTask OnDeactivatedAsync();
    ValueTask OnStoppedAsync();
    ValueTask OnResumedAsync();
    ValueTask OnDestroyingAsync();
}

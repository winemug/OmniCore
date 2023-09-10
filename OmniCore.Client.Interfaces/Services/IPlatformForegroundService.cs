using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Interfaces.Services
{
    public interface IPlatformForegroundService
    {
        Task<IDisposable> RunInForegroundAsync();
    }
}

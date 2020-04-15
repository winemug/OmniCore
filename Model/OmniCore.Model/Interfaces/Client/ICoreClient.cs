using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces.Client
{
    public interface ICoreClient : ICoreClientFunctions
    {
        T GetView<T>(bool viaShell, object parameter = null)
            where T : IView;

        Task<ICoreApi> GetApi(CancellationToken cancellationToken);
        Task PushView<T>() where T : IView;
        Task PushView<T>(object parameter) where T : IView;
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces.Client
{
    public interface IClient : IClientInstance
    {
        Task<T> GetView<T>(bool viaShell, object parameter = null)
            where T : IView;
        Task<IApi> GetApi(CancellationToken cancellationToken);
        Task PushView<T>() where T : IView;
        Task PushView<T>(object parameter) where T : IView;
    }
}
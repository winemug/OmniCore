using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces
{
    public interface IClient 
    {
        Task<T> GetView<T>(bool viaShell, object parameter = null)
            where T : IView;
        Task<IServiceApi> GetServiceApi(CancellationToken cancellationToken);
        Task NavigateTo<T>() where T : IView;
        Task NavigateTo<T>(object parameter) where T : IView;
    }
}
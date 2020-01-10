using System;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Common
{
    public interface IView : IClientResolvable
    {
        IObservable<IView> WhenAppearing();
        IObservable<IView> WhenDisappearing();
    }

    public interface IView<in TModel> : IView where TModel : IViewModel
    {
        void SetViewModel(TModel viewModel);
    }
}

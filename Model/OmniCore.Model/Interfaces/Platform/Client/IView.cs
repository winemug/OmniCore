using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IView : IClientResolvable
    {
    }

    public interface IView<in TModel> : IView where TModel : IViewModel
    {
        void SetViewModel(TModel viewModel);
    }
}

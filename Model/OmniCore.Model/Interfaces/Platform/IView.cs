using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IView
    {
    }

    public interface IView<in TModel> : IView where TModel : IViewModel
    {
        void SetViewModel(TModel viewModel);
    }
}

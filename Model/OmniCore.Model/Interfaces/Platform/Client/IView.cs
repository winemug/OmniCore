using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Common
{
    public interface IView : IClientResolvable
    {
    }

    public interface IView<in TModel> : IView where TModel : IViewModel
    {
        void SetViewModel(TModel viewModel);
    }
}

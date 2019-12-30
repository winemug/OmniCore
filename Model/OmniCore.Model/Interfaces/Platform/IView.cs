namespace OmniCore.Model.Interfaces.Services
{
    public interface IView<out T> where T : IViewModel
    {
        T ViewModel { get;  }
    }
}

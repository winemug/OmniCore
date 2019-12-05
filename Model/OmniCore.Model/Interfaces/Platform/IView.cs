namespace OmniCore.Model.Interfaces.Platform
{
    public interface IView<out T> where T : IViewModel
    {
        T ViewModel { get;  }
    }
}

namespace OmniCore.Client.Abstractions;

public interface IDataViewModel<T> : IViewModel
{
    Task LoadDataAsync(T data);
}

public interface IViewModel
{
    Task OnPaused();
    Task OnResumed();
    Task OnNavigatingTo();
    Task OnNavigatingAway();
    Task BindToView(IView view);
}
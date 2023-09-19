namespace OmniCore.Client.Interfaces;

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
    Task BindToView(Page page);
}
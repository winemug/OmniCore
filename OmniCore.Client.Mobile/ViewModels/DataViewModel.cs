using OmniCore.Client.Interfaces;

namespace OmniCore.Client.Mobile.ViewModels;

public abstract class DataViewModel<T> : ViewModel, IDataViewModel<T>
{
    public abstract Task LoadDataAsync(T data);
}
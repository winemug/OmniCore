using OmniCore.Maui.ViewModels;

namespace OmniCore.Maui.Services;

public class ViewModelViewMapper
{
    private Dictionary<Type, Type> _vmvDict = new Dictionary<Type, Type>();
    public void AddMapping<TView, TViewModel>(MauiAppBuilder appBuilder)
        where TView : Page
        where TViewModel : BaseViewModel
    {
        appBuilder.Services.AddTransient<TViewModel>();
        _vmvDict.Add(typeof(TView), typeof(TViewModel));
    }

    public BaseViewModel GetViewModel(Type viewType, IServiceProvider provider)
    {
        return provider.GetService(_vmvDict[viewType]) as BaseViewModel;
    }
}
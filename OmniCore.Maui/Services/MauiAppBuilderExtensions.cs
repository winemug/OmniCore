using OmniCore.Maui.ViewModels;

namespace OmniCore.Maui.Services;

public static class MauiAppBuilderExtensions
{
    private static Dictionary<Type, Type> _vmvDict = new Dictionary<Type, Type>();
    
    public static void AddMapping<TView, TViewModel>(this MauiAppBuilder  appBuilder)
        where TView : Page
        where TViewModel : BaseViewModel
    {
        appBuilder.Services.AddTransient<TViewModel>();
        _vmvDict.Add(typeof(TView), typeof(TViewModel));
    }
    
    public static BaseViewModel? GetViewModel(this IServiceProvider provider, Type viewType)
    {
        return provider.GetService(_vmvDict[viewType]) as BaseViewModel;
    }
}
using Android.Media;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Markup;
namespace OmniCore.Maui.Models;

public abstract partial class PageModel : ObservableObject
{
    [RelayCommand]
    protected virtual async Task NavigatedTo()
    {
    }

    [RelayCommand]
    protected virtual async Task NavigatedFrom()
    {
    }
    
    [RelayCommand]
    protected virtual async Task Appearing()
    {
    }
}

public static class PageModelExtensions
{
    public static Page WithModel(this Page page, PageModel model)
    {
        page.Behaviors(
            new EventToCommandBehavior
            {
                EventName = nameof(Page.NavigatedTo),
                Command = model.NavigatedToCommand
            },
            new EventToCommandBehavior
            {
                EventName = nameof(Page.NavigatedFrom),
                Command = model.NavigatedFromCommand
            },
            new EventToCommandBehavior
            {
                EventName = nameof(Page.Appearing),
                Command = model.AppearingCommand
            });
        page.BindingContext = model;
        return page;
    }
}
using Android.Media;
using OmniCore.Maui.Models;
using OmniCore.Maui.PageModels;
using HomePageModel = OmniCore.Maui.PageModels.HomePageModel;

namespace OmniCore.Maui.Pages;

public partial class HomePage : ContentPage
{
    public HomePage(HomePageModel model)
    {
        InitializeComponent();
        this.WithModel(model);
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Maui.ViewModels;
using OmniCore.Services.Interfaces.Platform;

namespace OmniCore.Maui.Views;

public partial class DefaultPage : ContentPage
{
    public DefaultPage()
    {
        InitializeComponent();
    }

    private void Button_OnClicked(object sender, EventArgs e)
    {
        var fsh = App.Current.Handler.MauiContext.Services.GetService<IPlatformService>();
        fsh.StartService();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        var pi = App.Current.Handler.MauiContext.Services.GetService<IPlatformInfo>();
        if (!await pi.VerifyPermissions())
        {
            Debug.WriteLine("verify failed");
        }
        else
        {
            Debug.WriteLine("verify fine");
        }

    }


}


using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class AboutViewModelOld : BaseViewModelOld
    {
        public AboutViewModelOld()
        {
            Title = "About";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));
        }

        public ICommand OpenWebCommand { get; }
    }
}
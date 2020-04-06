using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.Models;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Services.Facade;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Home
{
    public class RadioDetailViewModel : BaseViewModel
    {
        public RadioDetailViewModel(ICoreClient client) : base(client)
        {
            Test1Command = new Command(() => { });
        }

        public RadioModel Radio { get; set; }
        public ICommand Test1Command { get; set; }

        protected override Task OnPageAppearing()
        {
            Radio = new RadioModel((IErosRadio) Parameter);
            return Task.CompletedTask;
        }
    }
}
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Test
{
    public class TestDetailViewModel : BaseViewModel
    {
        public bool ExecuteCommandEnabled { get; set; }
        public ICommand StatusCommand { get; }
        
        public TestDetailViewModel(IClient client) : base(client)
        {
            StatusCommand = new Command(async () =>
            {
                await ExecuteStatus();
            });
        }

        private async Task ExecuteStatus()
        {
            ExecuteCommandEnabled = false;
            
            ExecuteCommandEnabled = true;
        }
    }
}
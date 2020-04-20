using System;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Utilities.Extensions;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Base
{
    public class DialogViewModel : BaseViewModel
    {
        public ICommand ConfirmCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        
        public bool ConfirmEnabled { get; set; }

        protected Func<Task> ConfirmAction;
        protected Func<Task> CancelAction;
        
        public DialogViewModel(IClient client) : base(client)
        {
            ConfirmAction = () => Task.CompletedTask;
            CancelAction = () => Task.CompletedTask;
            
            WhenParameterSet().Subscribe(async p =>
            {
                var actions = ((Func<Task> Confirm, Func<Task> Cancel))p;
                ConfirmAction = actions.Confirm;
                CancelAction = actions.Cancel;

                if (ConfirmCommand == null)
                    ConfirmCommand = new Command(async () => await ConfirmAction());

                if (CancelCommand == null)
                    CancelCommand = new Command(async () => await CancelAction());
                
            }).AutoDispose(this);
        }
    }
}
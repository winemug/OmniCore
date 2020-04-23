using System;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities.Extensions;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Base
{
    public class DialogViewModel : BaseViewModel
    {
        public ICommand DialogOkCommand { get; set; }
        public ICommand DialogCancelCommand { get; set; }
        
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

                if (DialogOkCommand == null)
                    DialogOkCommand = new Command(async () => await ConfirmAction());

                if (DialogCancelCommand == null)
                    DialogCancelCommand = new Command(async () => await CancelAction());
                
            }).AutoDispose(this);
        }
    }
}
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.Views.Wizards.Permissions;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Utilities.Extensions;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Wizards
{

    public class SetupWizardViewModel : BaseViewModel
    {
        public bool IsMaybe { get; set; }
        public bool IsPossibly { get; set; }
        public bool IsUnlikely { get; set; }
        public bool ContinueEnabled => IsUnlikely;

        private readonly ICommonFunctions CommonFunctions;

        public ICommand ContinueCommand { get; }

        public ICommand ExitCommand { get; }
        
        public SetupWizardViewModel(IClient client,
            ICommonFunctions commonFunctions,
            IPlatformConfiguration platformConfiguration) : base(client)
        {
            CommonFunctions = commonFunctions;

            ContinueCommand = new Command(async () =>
            {
                if (IsUnlikely)
                {
                    platformConfiguration.TermsAccepted = true;
                    await client.PushView<PermissionsWizardRootView>();
                }
            });
            
            ExitCommand = new Command(() =>
            {
                CommonFunctions.Exit();
            });
            
            WhenPropertyChanged(this, p => p.IsMaybe)
                .Subscribe(maybe =>
                {
                    IsUnlikely = false;
                }).AutoDispose(this);
            
            WhenPropertyChanged(this, p => p.IsPossibly)
                .Subscribe(possibly =>
                {
                    IsUnlikely = false;
                }).AutoDispose(this);

            WhenPropertyChanged(this, p => p.IsUnlikely)
                .Subscribe(unlikely =>
                {
                    if (unlikely)
                    {
                        using var _ = SuspendPropertyFeedbackObservable();
                        IsMaybe = false;
                        IsPossibly = false;
                    }
                }).AutoDispose(this);
        }
    }
}
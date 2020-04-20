using System;
using System.Windows.Input;
using OmniCore.Client.Views.Wizards.Permissions;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Utilities.Extensions;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Base.Dialogs
{
    public class TermsDialogViewModel : DialogViewModel
    {
        public bool IsMaybe { get; set; }
        public bool IsPossibly { get; set; }
        public bool IsUnlikely { get; set; }
        public bool ContinueEnabled => IsUnlikely;

        private readonly ICommonFunctions CommonFunctions;

        public TermsDialogViewModel(
            IClient client,
            ICommonFunctions commonFunctions,
            IPlatformConfiguration platformConfiguration) : base(client)
        {
            CommonFunctions = commonFunctions;

            ConfirmCommand = new Command(async () =>
            {
                if (IsUnlikely)
                {
                    platformConfiguration.TermsAccepted = true;
                    await ConfirmAction();
                }
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
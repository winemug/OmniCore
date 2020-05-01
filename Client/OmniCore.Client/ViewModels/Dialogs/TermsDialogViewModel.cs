using System;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities.Extensions;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Base.Dialogs
{
    public class TermsDialogViewModel : DialogViewModel
    {
        public bool IsMaybe { get; set; }
        public bool IsPossibly { get; set; }
        public bool IsUnlikely { get; set; }
        public bool DialogOkEnabled { get; set; }

        private readonly IPlatformFunctions PlatformFunctions;

        public TermsDialogViewModel(
            IClient client,
            IPlatformFunctions platformFunctions,
            IPlatformConfiguration platformConfiguration) : base(client)
        {
            PlatformFunctions = platformFunctions;

            DialogOkCommand = new Command(async () =>
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
                }).DisposeWith(this);
            
            WhenPropertyChanged(this, p => p.IsPossibly)
                .Subscribe(possibly =>
                {
                    IsUnlikely = false;
                }).DisposeWith(this);

            WhenPropertyChanged(this, p => p.IsUnlikely)
                .Subscribe(unlikely =>
                {
                    if (unlikely)
                    {
                        using var _ = SuspendPropertyFeedbackObservable();
                        IsMaybe = false;
                        IsPossibly = false;
                        DialogOkEnabled = true;
                    }
                    else
                    {
                        DialogOkEnabled = false;
                    }
                }).DisposeWith(this);
        }
    }
}
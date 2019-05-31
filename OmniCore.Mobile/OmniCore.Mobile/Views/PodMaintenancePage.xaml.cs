using OmniCore.Mobile.ViewModels;
using OmniCore.Model;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PodMaintenancePage : ContentPage
    {
        private PodMaintenanceViewModel viewModel;

        public PodMaintenancePage()
        {
            InitializeComponent();
            BindingContext = viewModel = new PodMaintenanceViewModel();
        }

        private async void ActivateClicked(object sender, EventArgs e)
        {
            viewModel.ActivateButtonEnabled = false;
            try
            {
                if (App.PodProvider.Current != null)
                {
                    if (App.PodProvider.Current.Pod.Status.Progress < PodProgress.Running)
                    {
                        var dlgResult = await DisplayAlert("Pod Activation",
                            @"There is already a pod in activation progress, do you really want to start over with a new pod activation?"
                            + " Note: If you want to start over, you will have to discard the current pod and fill a new pod.",
                            "Start New Activation", "Cancel");

                        if (!dlgResult)
                            return;
                    }
                    else if (App.PodProvider.Current.Pod.Status.Progress <= PodProgress.RunningLow)
                    {
                        var dlgResult = await DisplayAlert("Pod Activation",
                            @"There is already an active pod running. Are you sure you want to activate a new pod? ",
                            "Activate New Pod", "Cancel");

                        if (!dlgResult)
                            return;
                    }

                    if (App.PodProvider.Current.Pod.Status.Progress < PodProgress.Inactive)
                    {
                        var dlgResult = await DisplayAlert("Pod Activation",
                            @"Would you like to deactivate the current pod before starting activation of a new pod? ",
                            "Deactivate", "Continue without deactivation");

                        while (dlgResult)
                        {
                            var ctsDeactivate = new CancellationTokenSource();
                            var progressDeactivate = new MessageProgress();
                            var resultDeactivate = await Deactivate(progressDeactivate, ctsDeactivate.Token);
                            if (resultDeactivate.Success)
                            {
                                await DisplayAlert("Pod Activation", "Existing pod has been deactivated successfully.", "Continue");
                                break;
                            }
                            else
                            {
                                dlgResult = await DisplayAlert(
                                    "Pod Activation",
                                    "Failed to deactivate the pod. If you want to try again, click the Deactivate button. Otherwise click Continue to start activating a new pod.",
                                    "Deactivate", "Continue without deactivation");

                                if (!dlgResult)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                var actDlgResult = await DisplayAlert(
                                "Pod Activation",
                                "Fill a new pod with insulin. Make sure the pod has beeped two times during the filling process. When you are finished, press Activate to start the process.",
                                "Activate", "Cancel");

                if (!actDlgResult)
                    return;

                //var newPodManager = App.PodProvider.New();

                //var cts = new CancellationTokenSource();
                //var progress = new MessageProgress();
                //var result = await Activate(progress, cts.Token);

                


            }
            finally
            {
                viewModel.ActivateButtonEnabled = true;
            }
        }

        private async void DeactivateClicked(object sender, EventArgs e)
        {
            viewModel.DeactivateButtonEnabled = false;
            try
            {
                if (App.PodProvider.Current.Pod.Status.Progress < PodProgress.Running)
                {
                    var dlgResult = await DisplayAlert("Pod Deactivation",
                        @"This pod has not been started yet. Are you sure you want to deactivate it without starting? " +
                        "Note: After successful deactivation the pod will shut down and will become unusable.",
                        "Deactivate", "Cancel");

                    if (!dlgResult)
                        return;
                }
                else if (App.PodProvider.Current.Pod.Status.Progress <= PodProgress.RunningLow)
                {
                    var dlgResult = await DisplayAlert("Pod Deactivation",
                        @"This pod is currently active and running. Are you sure you want to deactivate it? " +
                        "Note: After successful deactivation the pod will stop insulin delivery completely and will shut down.",
                        "Deactivate", "Cancel");

                    if (!dlgResult)
                        return;
                }
                var cts = new CancellationTokenSource();
                var progress = new MessageProgress();
                var result = await Deactivate(progress, cts.Token);
                if (result.Success)
                {
                    await DisplayAlert("Pod Deactivation", "Pod has been deactivated successfully.", "OK");
                }
                else
                {
                    await DisplayAlert("Pod Deactivation", "Failed to deactivate the pod.", "OK");
                }
            }
            finally
            {
                viewModel.DeactivateButtonEnabled = true;
            }
        }

        private async Task<IMessageExchangeResult> Deactivate(MessageProgress mp, CancellationToken ct)
        {
            return await App.PodProvider.Current.Deactivate(mp, ct).ConfigureAwait(false);
        }

        private async Task<IMessageExchangeResult> Activate(MessageProgress mp, CancellationToken ct)
        {
            return await App.PodProvider.Current.Deactivate(mp, ct).ConfigureAwait(false);
        }
    }
}
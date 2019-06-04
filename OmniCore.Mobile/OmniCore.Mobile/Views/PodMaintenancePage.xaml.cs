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

        private async Task<bool> ActivationStep1Pairing()
        {
            var progress = new MessageProgress();
            MessageExchangeDisplay.SetProgress(progress);

            var result = await Task.Run(async () => await App.PodProvider.Current.Pair(progress, 60).ConfigureAwait(false));
            if (!result.Success)
            {
                await DisplayAlert("Pod Activation",
                    "Failed to pair with the pod.", "OK");
                return false;
            }
            return true;
        }

        private async Task<bool> ActivationStep2Priming()
        {
            var progress = new MessageProgress();
            MessageExchangeDisplay.SetProgress(progress);

            var result = await Task.Run(async () => await App.PodProvider.Current.Activate(progress).ConfigureAwait(false));
            if (!result.Success)
            {
                await DisplayAlert("Pod Activation",
                    "Failed to prime the pod.", "OK");
                return false;
            }
            return true;
        }

        private async void ResumeActivateClicked(object sender, EventArgs e)
        {
            try
            {
                bool actDlgResult;
                var podManager = App.PodProvider.Current;
                if (podManager.Pod.Status.Progress < PodProgress.ReadyForInjection)
                {
                    actDlgResult = await DisplayAlert(
                                    "Pod Activation",
                                    "Press Activate to resume activating this pod.",
                                    "Activate", "Cancel");

                    if (!actDlgResult)
                        return;

                    if (podManager.Pod.Status.Progress < PodProgress.PairingSuccess)
                    {
                        if (!await ActivationStep1Pairing())
                            return;
                    }

                    if (!await ActivationStep2Priming())
                        return;
                }

                actDlgResult = await DisplayAlert(
                                    "Pod Activation",
                                    "Press Start to inject to cannula and start the pod.",
                                    "Start", "Cancel");

                if (!actDlgResult)
                    return;

                var basalSchedule = new decimal[48];
                for (int i = 0; i < 48; i++)
                    basalSchedule[i] = 0.60m;

                var progress = new MessageProgress();
                MessageExchangeDisplay.SetProgress(progress);
                var result = await Task.Run(async () => await podManager.InjectAndStart(progress, basalSchedule, 60).ConfigureAwait(false));

                if (!result.Success)
                {
                    await DisplayAlert("Pod Activation",
                        "Failed to start the pod.", "OK");
                }
            }
            finally
            {
            }
        }

        private async void ActivateClicked(object sender, EventArgs e)
        {
            viewModel.ActivateNewButtonVisible = false;
            try
            {
                var podManager = App.PodProvider.Current;
                if (podManager != null && podManager.Pod.Status != null)
                {
                    if (podManager.Pod.Status.Progress < PodProgress.Running)
                    {
                        var dlgResult = await DisplayAlert("Pod Activation",
                            @"There is already a pod in activation progress, do you really want to start over with a new pod activation?"
                            + " Note: If you want to start over, you will have to discard the current pod and fill a new pod.",
                            "Start New Activation", "Cancel");

                        if (!dlgResult)
                            return;
                    }
                    else if (podManager.Pod.Status.Progress <= PodProgress.RunningLow)
                    {
                        var dlgResult = await DisplayAlert("Pod Activation",
                            @"There is already an active pod running. Are you sure you want to activate a new pod? ",
                            "Activate New Pod", "Cancel");

                        if (!dlgResult)
                            return;
                    }

                    if (podManager.Pod.Status.Progress < PodProgress.Inactive)
                    {
                        var dlgResult = await DisplayAlert("Pod Activation",
                            @"Would you like to deactivate the current pod before starting activation of a new pod? ",
                            "Deactivate", "Continue without deactivation");

                        while (dlgResult)
                        {
                            var progressDeactivate = new MessageProgress();
                            MessageExchangeDisplay.SetProgress(progressDeactivate);

                            var resultDeactivate = await Task.Run(async () => await App.PodProvider.Current.Deactivate(progressDeactivate).ConfigureAwait(false));
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

                if (App.PodProvider.Current == null || App.PodProvider.Current.Pod.Status != null)
                    podManager = App.PodProvider.New();

                var progress = new MessageProgress();
                MessageExchangeDisplay.SetProgress(progress);

                var result = await Task.Run(async () => await podManager.Pair(progress, 60).ConfigureAwait(false));
                if (!result.Success)
                    return;

                progress = new MessageProgress();
                MessageExchangeDisplay.SetProgress(progress);

                result = await Task.Run(async () => await podManager.Activate(progress).ConfigureAwait(false));
                if (!result.Success)
                    return;

                var basalSchedule = new decimal[48];
                for (int i = 0; i < 48; i++)
                    basalSchedule[i] = 0.60m;

                await DisplayAlert(
                                "Pod Activation",
                                "Ready... set.. go!", "OK");

                progress = new MessageProgress();
                MessageExchangeDisplay.SetProgress(progress);
                result = await Task.Run(async () => await podManager.InjectAndStart(progress, basalSchedule, 60).ConfigureAwait(false));

            }
            finally
            {
                viewModel.ActivateNewButtonVisible = true;
            }
        }

        private async void DeactivateClicked(object sender, EventArgs e)
        {
            viewModel.DeactivateButtonVisible = false;
            try
            {
                var podManager = App.PodProvider.Current;
                var progress = new MessageProgress();
                MessageExchangeDisplay.SetProgress(progress);

                if (podManager.Pod.Status == null || podManager.Pod.Status.Progress < PodProgress.PairingSuccess)
                {
                    var statusResult = await Task.Run(async () => await podManager.UpdateStatus(progress).ConfigureAwait(false));
                }

                if (podManager.Pod.Status == null || podManager.Pod.Status.Progress < PodProgress.PairingSuccess)
                {
                    var dlgResult = await DisplayAlert("Pod Deactivation",
                        @"This pod has not been paired yet and cannot be deactivated. Would you like to remove the pod from the system? " +
                        "Note: If you remove it, you won't be able to resume its activation process.",
                        "Remove Pod", "Cancel");

                    if (dlgResult)
                    {
                        App.PodProvider.Archive();
                    }
                    return;
                }

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
                progress = new MessageProgress();
                MessageExchangeDisplay.SetProgress(progress);

                var result = await Task.Run(async () => await App.PodProvider.Current.Deactivate(progress).ConfigureAwait(false));

                if (result.Success)
                {
                    App.PodProvider.Archive();
                    await DisplayAlert("Pod Deactivation", "Pod has been deactivated successfully.", "OK");
                }
                else
                {
                    var dlgResult = await DisplayAlert("Pod Deactivation", "Failed to deactivate the pod. Would you like to remove the pod from the system?" +
                        "Note: If you remove it, you won't be able to control this pod anymore and if the pod is working, it will continue to deliver basals as programmed.", "Remove Pod", "Cancel");

                    if (dlgResult)
                    {
                        App.PodProvider.Archive();
                    }
                }
            }
            finally
            {
                viewModel.DeactivateButtonVisible = true;
            }
        }
    }
}
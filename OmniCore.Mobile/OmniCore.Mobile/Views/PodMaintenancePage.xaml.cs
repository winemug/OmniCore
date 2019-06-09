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

        private async void DeactivateClicked(object sender, EventArgs e)
        {
            viewModel.DeactivateButtonEnabled = false;
            try
            {
                var podProvider = App.Instance.PodProvider;
                var podManager = podProvider.Current;

                using(var conversation = await podManager.StartConversation())
                {
                    if (podManager.Pod.Status == null || podManager.Pod.Status.Progress < PodProgress.PairingSuccess)
                        await Task.Run(async () => await podManager.UpdateStatus(conversation).ConfigureAwait(false));

                    if (podManager.Pod.Status == null || podManager.Pod.Status.Progress < PodProgress.PairingSuccess)
                    {
                        var dlgResult = await DisplayAlert("Pod Deactivation",
                            @"This pod has not been paired yet and cannot be deactivated. Would you like to remove the pod from the system? " +
                            "Note: If you remove it, you won't be able to resume its activation process.",
                            "Remove Pod", "Cancel");

                        if (dlgResult)
                        {
                            podProvider.Archive();
                        }
                        return;
                    }

                    if (podManager.Pod.Status.Progress < PodProgress.Running)
                    {
                        var dlgResult = await DisplayAlert("Pod Deactivation",
                            @"This pod has not been started yet. Are you sure you want to deactivate it without starting? " +
                            "Note: After successful deactivation the pod will shut down and will become unusable.",
                            "Deactivate", "Cancel");

                        if (!dlgResult)
                            return;
                    }
                    else if (podManager.Pod.Status.Progress <= PodProgress.RunningLow)
                    {
                        var dlgResult = await DisplayAlert("Pod Deactivation",
                            @"This pod is currently active and running. Are you sure you want to deactivate it? " +
                            "Note: After successful deactivation the pod will stop insulin delivery completely and will shut down.",
                            "Deactivate", "Cancel");

                        if (!dlgResult)
                            return;
                    }

                    await Task.Run(async () => await podManager.Deactivate(conversation).ConfigureAwait(false));

                    if (!conversation.Failed)
                    {
                        podProvider.Archive();
                        await DisplayAlert("Pod Deactivation", "Pod has been deactivated successfully.", "OK");
                    }
                    else
                    {
                        var dlgResult = await DisplayAlert("Pod Deactivation", "Failed to deactivate the pod. Would you like to remove the pod from the system?" +
                            "Note: If you remove it, you won't be able to control this pod anymore and if the pod is working, it will continue to deliver basals as programmed.", "Remove Pod", "Cancel");

                        if (dlgResult)
                        {
                            podProvider.Archive();
                            await DisplayAlert("Pod Deactivation", "Pod has been removed.", "OK");
                        }
                    }
                }
            }
            finally
            {
                viewModel.DeactivateButtonEnabled = true;
            }
        }

        private async void ActivateClicked(object sender, EventArgs e)
        {
            viewModel.ActivateNewButtonEnabled = false;
            try
            {
                await Activate();
            }
            finally
            {
                viewModel.ActivateNewButtonEnabled = true;
            }
        }

        private async void ResumeActivateClicked(object sender, EventArgs e)
        {
            viewModel.ResumeActivationButtonEnabled = false;
            try
            {
                await Activate();
            }
            finally
            {
                viewModel.ResumeActivationButtonEnabled = true;
            }
        }


        private async Task Activate()
        {
            var podProvider = App.Instance.PodProvider;
            bool actDlgResult;

            if (podProvider.Current == null || podProvider.Current.Pod.Status == null)
            {

                actDlgResult = await DisplayAlert(
                            "Pod Activation",
                            "Fill a new pod with insulin. Make sure the pod has beeped two times during the filling process. When you are finished, press Activate to start the process.",
                            "Activate", "Cancel");

                if (!actDlgResult)
                    return;

                podProvider.New();
            }
            else
            {
                actDlgResult = await DisplayAlert(
                            "Pod Activation",
                            "Press Resume to continue activating the current pod.",
                            "Resume", "Cancel");

                if (!actDlgResult)
                    return;
            }

            var podManager = podProvider.Current;

            using (var conversation = await podManager.StartConversation())
            {
                if (podManager.Pod.Status != null)
                {
                    await Task.Run(async () => await podManager.UpdateStatus(conversation).ConfigureAwait(false));
                }

                if (podManager.Pod.Status == null || podManager.Pod.Status.Progress < PodProgress.PairingSuccess)
                {
                    await Task.Run(async () => await podManager.Pair(conversation, 60).ConfigureAwait(false));

                    if (conversation.Failed)
                    {
                        await DisplayAlert("Pod Activation", "Failed to pair the pod.", "OK");
                        return;
                    }
                }

                if (podManager.Pod.Status.Progress < PodProgress.ReadyForInjection)
                {
                    await Task.Run(async () => await podManager.Activate(conversation).ConfigureAwait(false));
                    if (conversation.Failed)
                    {
                        await DisplayAlert("Pod Activation", "Failed to activate the pod.", "OK");
                        return;
                    }

                    actDlgResult = await DisplayAlert(
                        "Pod Activation",
                        "Pod has been activated successfully. Apply the pod and press Start to inject the cannula and start the pod.",
                        "Start", "Cancel");

                    if (!actDlgResult)
                        return;
                }
                else
                {
                    actDlgResult = await DisplayAlert(
                        "Pod Activation",
                        "Apply the pod and press Start to inject the cannula and start the pod.",
                        "Start", "Cancel");

                    if (!actDlgResult)
                        return;
                }

                var basalSchedule = new decimal[48];
                for (int i = 0; i < 48; i++)
                    basalSchedule[i] = 0.40m;

                await Task.Run(async () => await podManager.InjectAndStart(conversation, basalSchedule, 60).ConfigureAwait(false));
                if (conversation.Failed)
                {
                    await DisplayAlert("Pod Activation", "Failed to start the pod.", "OK");
                    return;
                }

                await DisplayAlert("Pod Activation",
                                    "Pod started.",
                                    "OK");
            }
        }

    }
}
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
                var podManager = podProvider.PodManager;

                IConversation conversation;

                if (podManager.Pod.LastStatus == null || podManager.Pod.LastStatus.Progress < PodProgress.PairingSuccess)
                {
                    using (conversation = await podManager.StartConversation())
                        await podManager.UpdateStatus(conversation);
                }

                if (podManager.Pod.LastStatus == null || podManager.Pod.LastStatus.Progress < PodProgress.PairingSuccess)
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

                if (podManager.Pod.LastStatus.Progress < PodProgress.Running)
                {
                    var dlgResult = await DisplayAlert("Pod Deactivation",
                        @"This pod has not been started yet. Are you sure you want to deactivate it without starting? " +
                        "Note: After successful deactivation the pod will shut down and will become unusable.",
                        "Deactivate", "Cancel");

                    if (!dlgResult)
                        return;
                }
                else if (podManager.Pod.LastStatus.Progress <= PodProgress.RunningLow)
                {
                    var dlgResult = await DisplayAlert("Pod Deactivation",
                        @"This pod is currently active and running. Are you sure you want to deactivate it? " +
                        "Note: After successful deactivation the pod will stop insulin delivery completely and will shut down.",
                        "Deactivate", "Cancel");

                    if (!dlgResult)
                        return;
                }

                using (conversation = await podManager.StartConversation())
                    await podManager.Deactivate(conversation);

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

            if (podProvider.PodManager == null || podProvider.PodManager.Pod.LastStatus == null)
            {
                actDlgResult = await DisplayAlert(
                            "Pod Activation",
                            "Fill a new pod with insulin. Make sure the pod has beeped two times during the filling process. When you are finished, press Activate to start the process.",
                            "Activate", "Cancel");

                if (!actDlgResult)
                    return;

                if (podProvider.PodManager == null)
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

            var podManager = podProvider.PodManager;
            IConversation conversation;

            if (podManager.Pod.LastStatus == null)
            {
                using (conversation = await podManager.StartConversation())
                    await podManager.UpdateStatus(conversation);
            }

            if (podManager.Pod.LastStatus == null || podManager.Pod.LastStatus.Progress < PodProgress.PairingSuccess)
            {
                using (conversation = await podManager.StartConversation())
                    await podManager.Pair(conversation, 60);
                if (conversation.Failed)
                {
                    await DisplayAlert("Pod Activation", "Failed to pair the pod.", "OK");
                    return;
                }
            }

            if (podManager.Pod.LastStatus.Progress < PodProgress.ReadyForInjection)
            {
                using (conversation = await podManager.StartConversation())
                {
                    await podManager.Activate(conversation);
                }
                if (conversation.Failed)
                {
                    await DisplayAlert("Pod Activation", "Failed to activate the pod.", "OK");
                    return;
                }
                else
                {
                    actDlgResult = await DisplayAlert(
                        "Pod Activation",
                        "Pod has been primed and activated successfully. Apply the pod and press Start to inject the cannula and start the pod.",
                        "Start", "Cancel");
                }
            }
            else
            {
                actDlgResult = await DisplayAlert(
                    "Pod Activation",
                    "Apply the pod and press Start to inject the cannula and start the pod.",
                    "Start", "Cancel");
            }
            if (!actDlgResult)
                return;


            var basalSchedule = new decimal[48];
            for (int i = 0; i < 48; i++)
                basalSchedule[i] = 0.40m;
            using (conversation = await podManager.StartConversation())
            {
                await podManager.InjectAndStart(conversation, basalSchedule, 60);
            }
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
using Microsoft.AppCenter.Crashes;
using OmniCore.Mobile.Base;
using OmniCore.Mobile.ViewModels.Pod;
using OmniCore.Model;
using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile.Views.Pod
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Fody.ConfigureAwait(true)]
    public partial class MaintenancePage : ContentPage
    {
        public MaintenancePage()
        {
            InitializeComponent();
            new MaintenanceViewModel(this);
        }

        private async void DeactivateClicked(object sender, EventArgs e)
        {
            try
            {
                var podProvider = App.Instance.PodProvider;
                var pod = podProvider.SinglePod;

                IConversation conversation;

                if (pod.LastStatus == null || pod.LastStatus.Progress < PodProgress.PairingSuccess)
                {
                    using (conversation = await pod.StartConversation(App.Instance.ExchangeProvider, 
                        "Checking Activation Status"))
                    {
                        await pod.UpdateStatus(conversation, timeout: 30000);
                    }
                }

                if (pod.LastStatus == null || pod.LastStatus.Progress < PodProgress.PairingSuccess)
                {
                    var dlgResult = await DisplayAlert("Pod Deactivation",
                        @"This pod has not been paired yet and cannot be deactivated. Would you like to remove the pod from the system? " +
                        "Note: If you remove it, you won't be able to resume its activation process.",
                        "Remove Pod", "Cancel");

                    if (dlgResult)
                    {
                        await podProvider.Archive(pod);
                    }
                    return;
                }

                if (pod.LastStatus.Progress < PodProgress.Running)
                {
                    var dlgResult = await DisplayAlert("Pod Deactivation",
                        @"This pod has not been started yet. Are you sure you want to deactivate it without starting? " +
                        "Note: After successful deactivation the pod will shut down and will become unusable.",
                        "Deactivate", "Cancel");

                    if (!dlgResult)
                        return;
                }
                else if (pod.LastStatus.Progress <= PodProgress.RunningLow)
                {
                    var dlgResult = await DisplayAlert("Pod Deactivation",
                        @"This pod is currently active and running. Are you sure you want to deactivate it? " +
                        "Note: After successful deactivation the pod will stop insulin delivery completely and will shut down.",
                        "Deactivate", "Cancel");

                    if (!dlgResult)
                        return;
                }

                using (conversation = await pod.StartConversation(App.Instance.ExchangeProvider, 
                    "Deactivate Pod"))
                    await pod.Deactivate(conversation);

                if (!conversation.Failed)
                {
                    await podProvider.Archive(pod);
                    await DisplayAlert("Pod Deactivation", "Pod has been deactivated successfully.", "OK");
                }
                else
                {
                    var dlgResult = await DisplayAlert("Pod Deactivation", "Failed to deactivate the pod. Would you like to remove the pod from the system?" +
                        "Note: If you remove it, you won't be able to control this pod anymore and if the pod is working, it will continue to deliver basals as programmed.", "Remove Pod", "Cancel")
						;

                    if (dlgResult)
                    {
                        await podProvider.Archive(pod);
                        await DisplayAlert("Pod Deactivation", "Pod has been removed.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                OmniCoreServices.Logger.Error("Error during deactivation", ex);
                Crashes.TrackError(ex);
            }
        }

        private async void ActivateClicked(object sender, EventArgs e)
        {
            try
            {
                var podProvider = App.Instance.PodProvider;
                var pod = podProvider.SinglePod;
                bool actDlgResult;

                if (pod?.LastStatus == null)
                {
                    actDlgResult = await DisplayAlert(
                                "Pod Activation",
                                "Fill a new pod with insulin. Make sure the pod has beeped two times during the filling process. When you are finished, press Activate to start the process.",
                                "Activate", "Cancel");

                    if (!actDlgResult)
                        return;

                    if (pod == null)
                        await podProvider.New();
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

                pod = podProvider.SinglePod;
                IConversation conversation;

                if (pod.LastStatus == null)
                {
                    using (conversation = await pod.StartConversation(App.Instance.ExchangeProvider, 
                        "Update Status"))
                        await pod.UpdateStatus(conversation, timeout: 30000);
                }

                var repo = await ErosRepository.GetInstance();
                var activeProfile = await repo.GetProfile();

                if (pod.LastStatus == null || pod.LastStatus.Progress < PodProgress.PairingSuccess)
                {
                    using (conversation = await pod.StartConversation(App.Instance.ExchangeProvider, 
                        "Pair Pod"))
                        await pod.Pair(conversation, activeProfile);
                    if (conversation.Failed)
                    {
                        await DisplayAlert("Pod Activation", "Failed to pair the pod.", "OK");
                        return;
                    }
                }

                if (pod.LastStatus.Progress < PodProgress.ReadyForInjection)
                {
                    using (conversation = await pod.StartConversation(App.Instance.ExchangeProvider, 
                        "Activate Pod"))
                    {
                        await pod.Activate(conversation);
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


                using (conversation = await pod.StartConversation(App.Instance.ExchangeProvider, 
                    "Start Pod"))
                {
                    await pod.InjectAndStart(conversation, activeProfile);
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
            catch(Exception ex)
            {
                OmniCoreServices.Logger.Error("Error during activation", ex);
                Crashes.TrackError(ex);

            }
        }
    }
}
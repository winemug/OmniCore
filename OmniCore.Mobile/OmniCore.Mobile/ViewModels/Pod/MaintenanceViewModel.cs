using OmniCore.Mobile.Base;
using OmniCore.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels.Pod
{
    public class MaintenanceViewModel : PageViewModel
    {
        public string ActivationText
        {
            get
            {
                if (Pod?.LastStatus?.Progress == null)
                    return "Activate New Pod";
                else if (Pod.LastStatus.Progress.Value < PodProgress.Running)
                    return "Resume Activation";
                else
                    return "Pod Active";
            }
        }

        public bool ActivateEnabled =>
            !IsInConversation &&
            (Pod?.LastStatus == null || Pod?.LastStatus?.Progress < PodProgress.Running);

        public bool DeactivateEnabled =>
            !IsInConversation && Pod?.LastStatus?.Progress != null && Pod.LastStatus.Progress.Value >= PodProgress.PairingSuccess
            && Pod.LastStatus.Progress.Value <= PodProgress.Inactive;

        public MaintenanceViewModel(Page page):base(page)
        {
            Disposables.Add(this.OnPropertyChanges().Subscribe((propertyName) =>
            {
                if (propertyName == nameof(Pod) || propertyName == nameof(ActiveConversation))
                {
                    OnPropertyChanged(nameof(ActivateEnabled));
                    OnPropertyChanged(nameof(DeactivateEnabled));
                }
            }));

            MessagingCenter.Subscribe<IStatus>(this, MessagingConstants.PodStatusUpdated, (newStatus) =>
            {
                OnPropertyChanged(nameof(ActivateEnabled));
                OnPropertyChanged(nameof(DeactivateEnabled));
            });
        }

        [method: SuppressMessage("", "CS1998", Justification = "Not applicable")]
        protected async override Task<BaseViewModel> BindData()
        {
            OnPropertyChanged(nameof(ActivateEnabled));
            OnPropertyChanged(nameof(DeactivateEnabled));
            return this;
        }

        protected override void OnDisposeManagedResources()
        {
            MessagingCenter.Unsubscribe<IStatus>(this, MessagingConstants.PodStatusUpdated);
        }
    }
}

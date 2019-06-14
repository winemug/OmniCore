using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OmniCore.Mobile.ViewModels
{
    public class PodMaintenanceViewModel : BaseViewModel
    {
        private bool buttonsEnabled = true;
        private bool deactivateButtonVisible;
        private bool deactivateButtonEnabled = true;
        private bool activateNewButtonVisible;
        private bool activateNewButtonEnabled = true;
        private bool resumeActivationButtonVisible;
        private bool resumeActivationButtonEnabled = true;

        public bool ButtonsEnabled
        {
            get => buttonsEnabled; set => SetProperty(ref buttonsEnabled, value);
        }

        public bool DeactivateButtonVisible
        {
            get => deactivateButtonVisible; set => SetProperty(ref deactivateButtonVisible, value);
        }

        public bool DeactivateButtonEnabled
        {
            get => deactivateButtonEnabled; set => SetProperty(ref deactivateButtonEnabled, value);
        }

        public bool ActivateNewButtonVisible
        {
            get => activateNewButtonVisible; set => SetProperty(ref activateNewButtonVisible, value);
        }

        public bool ActivateNewButtonEnabled
        {
            get => activateNewButtonEnabled; set => SetProperty(ref activateNewButtonEnabled, value);
        }

        public bool ResumeActivationButtonVisible
        {
            get => resumeActivationButtonVisible; set => SetProperty(ref resumeActivationButtonVisible, value);
        }

        public bool ResumeActivationButtonEnabled
        {
            get => resumeActivationButtonEnabled; set => SetProperty(ref resumeActivationButtonEnabled, value);
        }

        protected override void OnPodPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DeactivateButtonVisible = false;
            ActivateNewButtonVisible = false;
            ResumeActivationButtonVisible = false;

            if (Pod != null && Pod.LastStatus != null && Pod.LastStatus.Progress >= Model.Enums.PodProgress.PairingSuccess
                && Pod.LastStatus.Progress < Model.Enums.PodProgress.Inactive)
                DeactivateButtonVisible = true;

            if (Pod != null && Pod.LastStatus != null && Pod.LastStatus.Progress < Model.Enums.PodProgress.Running &&
                Pod.LastStatus.Progress >= Model.Enums.PodProgress.PairingSuccess)
                ResumeActivationButtonVisible = true;
            else if (Pod == null || Pod.LastStatus == null || Pod.LastStatus.Progress < Model.Enums.PodProgress.PairingSuccess)
                ActivateNewButtonVisible = true;
        }
    }
}

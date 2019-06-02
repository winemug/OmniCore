using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OmniCore.Mobile.ViewModels
{
    public class PodMaintenanceViewModel : BaseViewModel
    {
        private bool deactivateButtonVisible = false;
        public bool DeactivateButtonVisible
        {
            get { return deactivateButtonVisible; }
            set { SetProperty(ref deactivateButtonVisible, value); }
        }

        private bool activateNewButtonVisible;
        public bool ActivateNewButtonVisible
        {
            get { return activateNewButtonVisible; }
            set { SetProperty(ref activateNewButtonVisible, value); }
        }

        private bool resumeActivationButtonVisible = false;
        public bool ResumeActivationButtonVisible
        {
            get { return resumeActivationButtonVisible; }
            set { SetProperty(ref resumeActivationButtonVisible, value); }
        }

        protected override void OnPodPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DeactivateButtonVisible = false;
            ActivateNewButtonVisible = false;
            ResumeActivationButtonVisible = false;

            if (Pod != null && Pod.Status != null)
            {
                if (Pod.Status.Progress < Model.Enums.PodProgress.Running)
                    ResumeActivationButtonVisible = true;

                if (Pod.Status != null)
                    DeactivateButtonVisible = true;
            }
            else
            {
                ActivateNewButtonVisible = true;
            }
        }
    }
}

using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace OmniCore.Mobile.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected IPod Pod;

        public BaseViewModel()
        {
            App.PodProvider.PodChanged += PodProvider_PodChanged;
            AttachToCurrentPod();
        }

        private void PodProvider_PodChanged(object sender, EventArgs e)
        {
            AttachToCurrentPod();
            OnPropertyChanged(string.Empty);
        }

        private void AttachToCurrentPod()
        {
            if (Pod != null)
            {
                Pod.PropertyChanged -= Pod_PropertyChanged;
            }

            if (App.PodProvider.Current != null)
            {
                Pod = App.PodProvider.Current.Pod;
                Pod.PropertyChanged += Pod_PropertyChanged;
            }
            else
            {
                Pod = null;
            }
            OnPodPropertyChanged(this, new PropertyChangedEventArgs(string.Empty));
        }

        private void Pod_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPodPropertyChanged(sender, e);
        }

        protected virtual void OnPodPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}

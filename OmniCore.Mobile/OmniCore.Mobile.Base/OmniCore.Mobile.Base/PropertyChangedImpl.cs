using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Xamarin.Forms;

namespace OmniCore.Mobile.Base
{
    [Fody.ConfigureAwait(true)]
    public class PropertyChangedImpl : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (PropertyChanged != null)
            {
                if (SynchronizationContext.Current == OmniCoreServices.UiSyncContext)
                {
                    PropertyChanged.Invoke(sender, eventArgs);
                }
                else
                {
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                    {
                        PropertyChanged.Invoke(sender, eventArgs);
                    });
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

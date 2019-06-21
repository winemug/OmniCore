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
    /*
     set => (.+) = value;
     set => SetProperty(ref $1, value); 
    */
    [Fody.ConfigureAwait(true)]
    public class PropertyChangedImpl : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private Timer notifyTimer = null;
        private HashSet<string> propertyNames;

        public PropertyChangedImpl()
        {
            propertyNames = new HashSet<string>();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                if (SynchronizationContext.Current == OmniCoreServices.UiSyncContext)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    });
                }

                // OmniCoreServices.UiSyncContext.Post( () =>
                //     {
                //     PropertyChanged.Invoke(this, ea);
                //     }
                //     )
                //Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                //{
                //    PropertyChanged.Invoke(this, ea);
                //});
            }
        }
            //lock(this)
            //{
            //    propertyNames.Add(propertyName);
            //    if (notifyTimer == null)
            //    {
            //        notifyTimer = new Timer(
            //            _ =>
            //            {
            //                if (PropertyChanged == null)
            //                    return;

            //                OmniCoreServices.UiSyncContext.Post(
            //                    (state) =>
            //                    {
            //                        string[] notifyNames;
            //                        lock (this)
            //                        {
            //                            notifyNames = new string[propertyNames.Count];
            //                            propertyNames.CopyTo(notifyNames);
            //                            propertyNames.Clear();
            //                            notifyTimer = null;
            //                        }
            //                        foreach (var p in notifyNames)
            //                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
            //                    }, null);
            //            }, null, 100, Timeout.Infinite);
            //    }
            //}

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
    }
}

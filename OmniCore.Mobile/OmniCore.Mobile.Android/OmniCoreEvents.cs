using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OmniCore.Mobile.Base.Interfaces;
using OmniCore.Model.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Mobile.Android
{
    public class OmniCoreEvents : IAppEventSubscriber, IAppEventPublisher
    {

        public const string PodChanged = "PodsChanged";
        public const string PodStatusUpdated = "PodStatusUpdated";
        public const string NewResultReceived = "NewResultReceived";
        public const string AppResuming = "AppResuming";
        public const string AppSleeping = "AppSleeping";
        public const string ConversationStarted = "ConversationStarted";
        public const string ConversationEnded = "ConversationEnded";

        public void RaisePodChanged(IPodProvider source)
        {
            MessagingCenter.Send(source, PodChanged);
        }

        public void OnPodChanged(object subscriber, Action<IPodProvider> callback)
        {
            MessagingCenter.Subscribe<IPodProvider>(subscriber, PodChanged, callback);
        }

        public void OnPodChanged(object subscriber)
        {
            MessagingCenter.Unsubscribe<IPodProvider>(subscriber, PodChanged);
        }
    }
}
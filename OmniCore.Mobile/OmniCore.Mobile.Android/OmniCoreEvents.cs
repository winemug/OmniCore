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
using OmniCore.Client.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Client.Droid
{
    //public class OmniCoreEvents : IAppEventSubscriber, IAppEventPublisher
    //{

    //    public const string PodChanged = "PodsChanged";
    //    public const string PodStatusUpdated = "PodStatusUpdated";
    //    public const string NewResultReceived = "NewResultReceived";
    //    public const string AppResuming = "AppResuming";
    //    public const string AppSleeping = "AppSleeping";
    //    public const string ConversationStarted = "ConversationStarted";
    //    public const string ConversationEnded = "ConversationEnded";

    //    public void RaisePodChanged<T>(IPodProvider<T,U,V> source) where T : IPod, new()
    //    {
    //        MessagingCenter.Send(source, PodChanged);
    //    }

    //    public void OnPodChanged<T>(object subscriber, Action<IPodProvider<T>> callback) where T : IPod, new()
    //    {
    //        MessagingCenter.Subscribe<IPodProvider<T>>(subscriber, PodChanged, callback);
    //    }

    //    public void OnPodChanged<T>(object subscriber) where T : IPod, new()
    //    {
    //        MessagingCenter.Unsubscribe<IPodProvider<T>>(subscriber, PodChanged);
    //    }
    //}
}
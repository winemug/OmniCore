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
using OmniCore.Mobile.Droid;
using OmniCore.Mobile.Interfaces;

[assembly: Xamarin.Forms.Dependency(typeof(OmniCoreReceiver))]
namespace OmniCore.Mobile.Droid
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    [IntentFilter(new [] { "net.balya.OmniCore.Mobile.Android" })]
    public class OmniCoreReceiver : BroadcastReceiver, ILocalRequestPublisher
    {
        private List<ILocalRequestSubscriber> Subscribers;
        public OmniCoreReceiver()
        {
            Subscribers = new List<ILocalRequestSubscriber>();
        }

        public override async void OnReceive(Context context, Intent intent)
        {
            var intentText = intent.GetStringExtra("request");
            Console.WriteLine($"Broadcast received: {intentText}");

            foreach (var subscriber in Subscribers)
                await subscriber.OnRequestReceived(intentText);
        }

        public void Subscribe(ILocalRequestSubscriber subscriber)
        {
            Subscribers.Add(subscriber);
        }

        public void Unsubscribe(ILocalRequestSubscriber subscriber)
        {
            Subscribers.Remove(subscriber);
        }
    }
}
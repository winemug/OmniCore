﻿// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using Android.App;
// using Android.Content;
// using Android.OS;
// using Android.Runtime;
// using Android.Views;
// using Android.Widget;
// using Xamarin.Forms;
// using OmniCore.Model.Utilities;
// using Microsoft.AppCenter.Crashes;
// using OmniCore.Model.Interfaces.Platform;
// using Unity;
//
// namespace OmniCore.Mobile.Droid.Platform
// {
//     public class RemoteRequestPublisher : IRemoteRequestPublisher
//     {
//         private static List<IRemoteRequestSubscriber> Subscribers = new List<IRemoteRequestSubscriber>();
//
//         [Unity.Dependency]
//         public IApplicationLogger Logger { get; set; }
//
//         public async Task<string> GetResult(string request)
//         {
//             foreach(var subscriber in Subscribers)
//             {
//                 try
//                 {
//                     var result = await subscriber.OnRequestReceived(request);
//                     if (!string.IsNullOrEmpty(result))
//                         return result;
//                 }
//                 catch(Exception e)
//                 {
//                     Logger.Error("Error executing on request received", e);
//                     Crashes.TrackError(e);
//                 }
//             }
//             return null;
//         }
//
//         public void Subscribe(IRemoteRequestSubscriber subscriber)
//         {
//             lock (this)
//             {
//                 Subscribers.Add(subscriber);
//             }
//         }
//
//         public void Unsubscribe(IRemoteRequestSubscriber subscriber)
//         {
//             lock (this)
//             {
//                 Subscribers.Remove(subscriber);
//             }
//         }
//     }
// }


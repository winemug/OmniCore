using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Push;
using OmniCore.Model.Interfaces;

namespace OmniCore.Client.Droid.Platform
{
    public class PlatformConfiguration : IPlatformConfiguration, IInitializable
    {
        private const string SharedPreferenceName = "OmniCore";
        private const string KeyTermsAccepted = "TermsAccepted";
        private const string KeyDefaultUserSetUp = "DefaultUserSetUp";
        private const string KeyServiceEnabled = "ServiceEnabled";

        public PlatformConfiguration()
        {
        }
        public bool ServiceEnabled
        {
            get => ReadBool(KeyServiceEnabled, false);
            set => WriteKey(KeyServiceEnabled, value);
        }

        public bool TermsAccepted
        {
            get => ReadBool(KeyTermsAccepted, false);
            set => WriteKey(KeyTermsAccepted, value);
        }

        public bool DefaultUserSetUp
        {
            get => ReadBool(KeyDefaultUserSetUp, false);
            set => WriteKey(KeyDefaultUserSetUp, value);
        }

        private ISharedPreferences GetPreferences() =>
            Application.Context.GetSharedPreferences(SharedPreferenceName, FileCreationMode.Private);

        private void WriteKey(string key, string value)
        {
            using var p = GetPreferences();
            using var editor = p.Edit();
            editor.PutString(key, value);
            editor.Commit();
        }

        private void WriteKey(string key, float value)
        {
            using var p = GetPreferences();
            using var editor = p.Edit();
            editor.PutFloat(key, value);
            editor.Commit();
        }

        private void WriteKey(string key, bool value)
        {
            using var p = GetPreferences();
            using var editor = p.Edit();
            editor.PutBoolean(key, value);
            editor.Commit();
        }

        private void WriteKey(string key, long value)
        {
            using var p = GetPreferences();
            using var editor = p.Edit();
            editor.PutLong(key, value);
            editor.Commit();
        }
        
        private string ReadString(string key, string defaultValue)
        {
            using var p = GetPreferences();
            return p.GetString(key, defaultValue);
        }

        private bool ReadBool(string key, bool defaultValue)
        {
            using var p = GetPreferences();
            return p.GetBoolean(key, defaultValue);
        }
        
        private long ReadLong(string key, long defaultValue)
        {
            using var p = GetPreferences();
            return p.GetLong(key, defaultValue);
        }
        
        private float ReadFloat(string key, float defaultValue)
        {
            using var p = GetPreferences();
            return p.GetFloat(key, defaultValue);
        }

        public Task Initialize()
        {
            if (!AppCenter.Configured)
                Push.PushNotificationReceived += (sender, e) =>
                {
                    // Add the notification message and title to the message
                    var summary = "Push notification received:" +
                                  $"\n\tNotification title: {e.Title}" +
                                  $"\n\tMessage: {e.Message}";

                    // If there is custom data associated with the notification,
                    // print the entries
                    if (e.CustomData != null)
                    {
                        summary += "\n\tCustom data:\n";
                        foreach (var key in e.CustomData.Keys) summary += $"\t\t{key} : {e.CustomData[key]}\n";
                    }

                    // Send the notification summary to debug output
                    // Logger.Debug(summary);
                };

            AppCenter.Start("android=51067176-2950-4b0e-9230-1998460d7981;", typeof(Analytics), typeof(Crashes),
                typeof(Push));
            return Task.CompletedTask;
        }
    }
}
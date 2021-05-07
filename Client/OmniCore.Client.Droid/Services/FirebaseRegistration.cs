using System.Diagnostics;
using Firebase.Iid;

namespace OmniCore.Client.Droid.Services
{
    public class FirebaseRegistration : FirebaseInstanceIdService
    {
        public override void OnTokenRefresh()
        {
            var refreshedToken = FirebaseInstanceId.Instance.Token;
            Debug.WriteLine("Refreshed token: " + refreshedToken);
        }
    }
}
using OmniCore.Mobile.ViewModels.Test;
using OmniCore.Model.Eros;
using OmniCore.Model.Eros.Data;
using OmniCore.Model.Exceptions;
using OmniCore.Radio.RileyLink;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Mobile.Views.Test
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DebugPage : ContentPage
    {
        public DebugPage()
        {
            InitializeComponent();
            BindingContext = new DebugViewModel(this);
            var rp = new ErosRadioPreferences() { ConnectToAny = true };
            rl = new RileyLink(rp);
            pod = new ErosPod() { RadioAddress = 0x34ADCC56 };
        }

        private async void RandomPod_Clicked(object sender, EventArgs e)
        {
            App.Instance.PodProvider.Register(1111, 2222, 0x34ADCC56);
        }

        private RileyLink rl;
        private ErosPod pod;
        private async void AddressTest_Clicked(object sender, EventArgs e)
        {

            var msg = new ErosMessageBuilder().WithAssignAddress(0x34ADCC56).Build() as ErosMessage;
            var mep = new ErosMessageExchangeParameters() { TransmissionLevelOverride = Model.Enums.TxPower.A3_BelowNormal };
            var rme = new RileyLinkMessageExchange(mep, pod, rl);

            var mutex = new SemaphoreSlim(1, 1);
            var conv = new ErosConversation(mutex, pod);
            var progress = new MessageExchangeProgress(conv, msg.RequestType);
            progress.Result.Statistics = new RileyLinkStatistics();

            try
            {
                await rme.InitializeExchange(progress);

                var response = await rme.GetResponse(msg, progress);

                rme.ParseResponse(response, pod, progress);

                pod.Id = Guid.NewGuid();
                pod.Created = DateTimeOffset.UtcNow;
                App.Instance.PodProvider.Archive();
                App.Instance.PodProvider.Register(pod.Lot.Value, pod.Serial.Value, pod.RadioAddress);
                Debug.WriteLine($"{pod}");
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error: {ex}");
            }

            //    var packets = rme.GetRadioPackets(msg);
            //    var packet = packets[0];

            //    try
            //    {
            //        await rl.EnsureDevice(null);
            //        try
            //        {
            //            var receivedData = await rl.SendAndGetPacket(null, packet.GetPacketData(), 0, 50, 300, 5, 300);
            //            Debug.WriteLine($"DATA RECEIVED: {receivedData}");
            //        }
            //        catch(OmniCoreTimeoutException)
            //        {
            //            Debug.WriteLine($"Timeout");
            //        }
            //        try
            //        {
            //            var receivedData = await rl.SendAndGetPacket(null, packet.GetPacketData(), 0, 0, 120, 20, 40);
            //            Debug.WriteLine($"DATA RECEIVED: {receivedData}");
            //        }
            //        catch (OmniCoreTimeoutException)
            //        {
            //            Debug.WriteLine($"Timeout");
            //        }
            //    }
            //    catch (OmniCoreException ex)
            //    {
            //        Debug.WriteLine($"Exception: {ex}");
            //    }
        }
    }
}
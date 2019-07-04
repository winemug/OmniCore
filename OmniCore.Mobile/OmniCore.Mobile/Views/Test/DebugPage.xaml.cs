using OmniCore.Mobile.Base;
using OmniCore.Mobile.ViewModels.Test;
using OmniCore.Model.Eros;
using OmniCore.Model.Eros.Data;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
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
            new DebugViewModel(this);
            var rp = new ErosRadioPreferences() { ConnectToAny = true };
            rl = new RileyLink(rp);
        }

        private RileyLink rl;

        private async void RadioConnectionTest_Clicked(object sender, EventArgs e)
        {
            await PrepareForTest();

            for (uint t0 = 40; t0 < 100; t0 += 10)
            {
                for(uint t1 = 30; t1 < 100; t1 += 10)
                {
                    for(ushort s0 = 100; s0 < 350; s0 += 25)
                    {
                        for (ushort s1 = 10; s1 < 100; s1 += 10)
                        {
                            Debug.WriteLine($"################ CONNECTIVITY TEST: Running with {t0} {t1} {s0} {s1}");
                            var stats = await RunConnectivityTest(t0, t1, s0, s1);
                        }
                    }
                }
            }
        }

        private async Task PrepareForTest()
        {
            App.Instance.PodProvider.Register(42692, 461465, 883805270);

            var stats = new RileyLinkStatistics();
            var mep = new ErosMessageExchangeParameters()
            {
            };

            var pod = App.Instance.PodProvider.PodManager.Pod;
            await ExecuteCommand(stats, mep, new ErosMessageBuilder().WithStatus().Build());
            if (pod.LastStatus.BasalState == Model.Enums.BasalState.Temporary)
            {
                await ExecuteCommand(stats, mep, new ErosMessageBuilder().WithCancelTempBasal().Build());
            }
        }

        private async Task<RileyLinkStatistics> RunConnectivityTest(uint timeout0, uint timeout1, ushort seed0, ushort seed1)
        {
            var stats = new RileyLinkStatistics();
            var mep = new ErosMessageExchangeParameters()
            {
                TransmissionLevelOverride = Model.Enums.TxPower.A4_Normal,
                AllowAutoLevelAdjustment = false,
                FirstPacketTimeout = timeout0,
                SubsequentPacketTimeout = timeout1,
                FirstPacketPreambleLength = seed0,
                SubsequentPacketPreambleLength = seed1
            };

            await ExecuteCommand(stats, mep, new ErosMessageBuilder().WithStatus().Build());
            await ExecuteCommand(stats, mep, new ErosMessageBuilder().WithTempBasal(0, 12).Build());
            await ExecuteCommand(stats, mep, new ErosMessageBuilder().WithCancelTempBasal().Build());
            await ExecuteCommand(stats, mep, new ErosMessageBuilder().WithStatus().Build());
            return stats;
        }

        private async Task ExecuteCommand(RileyLinkStatistics stats, ErosMessageExchangeParameters parameters, IMessage msg)
        {
            var pod = App.Instance.PodProvider.PodManager.Pod;
            var rme = new RileyLinkMessageExchange(parameters, pod, rl);
            var mutex = new SemaphoreSlim(1, 1);
            var wakeLock = OmniCoreServices.Application.NewBluetoothWakeLock("radio_test");
            if (!await wakeLock.Acquire(10000))
                return;

            var conv = new ErosConversation(mutex, wakeLock, pod);
            var progress = new MessageExchangeProgress(conv, msg.RequestType);
            progress.Result.Statistics = stats;
            try
            {
                await rme.InitializeExchange(progress);
                var response = await rme.GetResponse(msg, progress);
                rme.ParseResponse(response, pod, progress);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex}");
            }
        }

        private void ExitApp_Clicked(object sender, EventArgs e)
        {
            OmniCoreServices.Application.Exit();
        }
    }
}
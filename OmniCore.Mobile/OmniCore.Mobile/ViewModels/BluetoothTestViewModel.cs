using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Mobile.Annotations;
using OmniCore.Services;
using OmniCore.Services.Interfaces;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Extensions;
using Polly;
using Unity;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels
{
    public class BluetoothTestViewModel : BaseViewModel
    {
        public Command StartCommand { get; }
        public Command StopCommand { get; }
        public Command DoCommand { get; }
        
        public string RssiText { get; private set; }

        private RadioConnection _rlc1;
        private RadioConnection _rlc2;
        private RadioConnection _rlc3;
        private ICoreService _coreService;
        private RadioService _radioService;
        private IForegroundServiceHelper _foregroundServiceHelper;

        public BluetoothTestViewModel()
        {
            StartCommand = new Command(StartClicked);
            StopCommand = new Command(StopClicked);
            DoCommand = new Command(DoClicked);
            _coreService = App.Container.Resolve<ICoreService>();
            _radioService = App.Container.Resolve<RadioService>();
            _foregroundServiceHelper = App.Container.Resolve<IForegroundServiceHelper>();
        }

        private void StartClicked()
        {
            _foregroundServiceHelper.StartForegroundService();
        }

        private int _messageSequence = 6;
        private int _packetSequence = 24;
        private async void DoClicked()
        {
            using (var conn = await _radioService.GetConnectionAsync("ema"))
            {
                // Debug.WriteLine("ema getpacket loop");
                // for(int i=0; i<10; i++)
                // {
                //     var packet = await conn.TryGetPacket(0, 100);
                //     if (packet != null)
                //         Debug.WriteLine($"ema result: {packet}");
                //     else
                //     {
                //         Debug.WriteLine($"ema result: n/a");
                //     }
                // }

                // var me = new MessageExchange(
                //     new RadioMessage
                //     {
                //         Address = 0x34c867a2,
                //         Sequence = _messageSequence,
                //         WithCriticalFollowup = false,
                //         Parts = new List<RadioMessagePart>() { new RequestStatusPart(RequestStatusType.Default) }
                //     },
                //     conn,
                //     _packetSequence);

                var me = new MessageExchange(
                    new RadioMessage
                    {
                        Address = 0x34c867a2,
                        Sequence = _messageSequence,
                        WithCriticalFollowup = false,
                        Parts = new List<RadioMessagePart>()
                        {
                            new RequestBeepConfigPart(BeepType.BipBipBip2x,
                                false, false, 0,
                                false, false, 0,
                                false, false, 0)
                        }
                    },
                    conn,
                    _packetSequence);

                Debug.WriteLine($"ema sending message");
                var result = await me.RunExchangeAsync();
                _messageSequence = result.NextMessageSequence;
                _packetSequence = result.NextPacketSequence;
                Debug.WriteLine($"ema next msgseq: {result.NextMessageSequence} pktseq: {result.NextPacketSequence}\n Response: {result.Response}");
                //_packetSequence = 0;
            }
        }

        private void StopClicked()
        {
            _foregroundServiceHelper.StopForegroundService();
        }
    }
}
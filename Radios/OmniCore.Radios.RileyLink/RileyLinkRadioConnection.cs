using OmniCore.Model.Interfaces;
using OmniCore.Repository;
using OmniCore.Repository.Entities;
using OmniCore.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadioConnection : IRadioConnection
    {
        private IRadioPeripheral Peripheral;
        private IDisposable ConnectedSubscription = null;
        private IDisposable ConnectionFailedSubscription = null;
        private IDisposable DisconnectedSubscription = null;

        private Radio RadioEntity;
        private PodRequest Request;
        private long? PodId = null;
        private long? RequestId = null;

        public async static Task<RileyLinkRadioConnection> CreateInstance(IRadioPeripheral radioPeripheral, Radio radioEntity, PodRequest request)
        {
            var instance = new RileyLinkRadioConnection(radioPeripheral, radioEntity, request);
            await instance.Initialize();
            return instance;
        }
        private RileyLinkRadioConnection(IRadioPeripheral radioPeripheral, Radio radioEntity, PodRequest request)
        {
            RadioEntity = radioEntity;
            Request = request;
        }

        private async Task Initialize()
        {
            ConnectedSubscription = Peripheral.WhenConnected().Subscribe( async (_) =>
            {
                var rssi = await Peripheral.ReadRssi();
                using (var rcr = new RadioConnectionRepository())
                {
                    await rcr.Create(new RadioConnection
                    {
                        RadioId = RadioEntity.Id.Value,
                        PodId = Request?.PodId,
                        RequestId = Request?.Id,
                        EventType = RadioConnectionEvent.Connect,
                        Successful = true,
                        Rssi = rssi
                    });
                }
            });

            ConnectionFailedSubscription = Peripheral.WhenConnectionFailed().Subscribe( async (err) =>
            {
                using (var rcr = new RadioConnectionRepository())
                {
                    await rcr.Create(new RadioConnection
                    {
                        RadioId = RadioEntity.Id.Value,
                        PodId = Request?.PodId,
                        RequestId = Request?.Id,
                        EventType = RadioConnectionEvent.Connect,
                        Successful = false,
                        ErrorText = err.Message
                    });
                }
            });

            DisconnectedSubscription = Peripheral.WhenDisconnected().Subscribe( async (_) =>
            {
                using (var rcr = new RadioConnectionRepository())
                {
                    await rcr.Create(new RadioConnection
                    {
                        RadioId = RadioEntity.Id.Value,
                        PodId = Request?.PodId,
                        RequestId = Request?.Id,
                        EventType = RadioConnectionEvent.Disconnect,
                        Successful = true
                    });
                }
            });

        }

        public async Task<bool> Connect()
        {
            if (await Peripheral.IsConnected())
            {
                //TODO: 
                return true;
            }
            else
                return await Peripheral.Connect();
        }

        public async Task Disconnect()
        {
            await Peripheral.Disconnect();
        }

        public async Task<bool> PrepareForMessageExchange()
        {
            throw new NotImplementedException();
        }

        public Task<IMessage> ExchangeMessages(IMessage messageToSend, TxPower? TxLevel = null)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Peripheral?.Dispose();
            ConnectedSubscription?.Dispose();
            ConnectionFailedSubscription?.Dispose();
            DisconnectedSubscription?.Dispose();
        }
    }
}

using OmniCore.Model.Interfaces;
using OmniCore.Repository;
using OmniCore.Repository.Entities;
using OmniCore.Repository.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadioConnection : IRadioConnection
    {
        public IRadioPeripheralLease PeripheralLease { get;  }
        private IRadioPeripheral Peripheral { get => PeripheralLease.Peripheral; }
        private IDisposable ConnectedSubscription = null;
        private IDisposable ConnectionFailedSubscription = null;
        private IDisposable DisconnectedSubscription = null;
        private IDisposable DeviceChangedSubscription = null;
        private IDisposable DeviceLostSubscription = null;

        private Radio RadioEntity;
        private PodRequest Request;

        public RileyLinkRadioConnection(IRadioPeripheralLease radioPeripheralLease, Radio radioEntity, PodRequest request)
        {
            RadioEntity = radioEntity;
            Request = request;
            PeripheralLease = radioPeripheralLease;
            SubscribeToDeviceStates();
            SubscribeToConnectionStates();
        }

        public async Task<bool> Initialize(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IMessage> ExchangeMessages(IMessage messageToSend, CancellationToken cancellationToken, TxPower? TxLevel = null)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            ConnectedSubscription?.Dispose();
            ConnectionFailedSubscription?.Dispose();
            DisconnectedSubscription?.Dispose();
            DeviceChangedSubscription?.Dispose();
            DeviceLostSubscription?.Dispose();
            PeripheralLease?.Dispose();
        }

        private void SubscribeToDeviceStates()
        {
            Peripheral.WhenDeviceChanged().Subscribe(async (_) =>
            {
                ConnectedSubscription?.Dispose();
                ConnectionFailedSubscription?.Dispose();
                DisconnectedSubscription?.Dispose();
                SubscribeToConnectionStates();
                //TODO: reset gatt related mumbojumbo
            });

            Peripheral.WhenDeviceLost().Subscribe(async (_) =>
            {
                //TODO: request peripheral device replacement
            });
        }

        private void SubscribeToConnectionStates()
        {
            ConnectedSubscription = Peripheral.WhenConnected().Subscribe( async (_) =>
            {
                var rssi = await Peripheral.ReadRssi();
                using (var rcr = RepositoryProvider.Instance.RadioConnectionRepository)
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
                using (var rcr = RepositoryProvider.Instance.RadioConnectionRepository)
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
                using (var rcr = RepositoryProvider.Instance.RadioConnectionRepository)
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

        private async Task ConfigureRileyLink()
        {
        }
    }
}

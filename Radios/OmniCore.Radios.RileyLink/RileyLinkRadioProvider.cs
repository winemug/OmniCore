using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using OmniCore.Model.Entities;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadioProvider : IErosRadioProvider
    {
        public Guid ServiceUuid => Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");

        private readonly AsyncLock RadioDictionaryLock;
        private readonly Dictionary<Guid, IErosRadio> RadioDictionary;
        
        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly ICoreRepositoryService RepositoryService;
        
        public RileyLinkRadioProvider(
            ICoreRepositoryService repositoryService,
            ICoreContainer<IServerResolvable> container)
        {
            RepositoryService = repositoryService;
            Container = container;
            
            RadioDictionary = new Dictionary<Guid, IErosRadio>();
            RadioDictionaryLock = new AsyncLock();
        }
        public async Task<IErosRadio> GetRadio(IBlePeripheral peripheral,
            CancellationToken cancellationToken)
        {
            using var lockObj = await RadioDictionaryLock.LockAsync(cancellationToken);
            if (RadioDictionary.ContainsKey(peripheral.PeripheralUuid))
            {
                return RadioDictionary[peripheral.PeripheralUuid];
            }
            var radio = Container.Get<RileyLinkRadio>();
            radio.Entity = await GetEntity(peripheral, cancellationToken);
            radio.Peripheral = peripheral;
            RadioDictionary.Add(peripheral.PeripheralUuid, radio);
            
            return radio;
        }

        private async Task<RadioEntity> GetEntity(IBlePeripheral peripheral, CancellationToken cancellationToken)
        {
            using var context = await RepositoryService.GetWriterContext(cancellationToken);
            var re = await context.Radios.Where(r => !r.IsDeleted &&
                                      r.DeviceUuid == peripheral.PeripheralUuid).FirstOrDefaultAsync();
            if (re == null)
            {
                re = new RadioEntity
                {
                    DeviceName = peripheral.Name,
                    DeviceUuid = peripheral.PeripheralUuid,
                    ServiceUuid = peripheral.PrimaryServiceUuid
                };
                await context.Radios.AddAsync(re, cancellationToken);
                await context.Save(cancellationToken);
            }
            return re;
        }
    }
}
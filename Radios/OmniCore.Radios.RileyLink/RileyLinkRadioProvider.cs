using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using OmniCore.Model.Entities;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Interfaces.Platform.Common;
using OmniCore.Model.Interfaces.Platform.Server;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadioProvider : IErosRadioProvider
    {
        public Guid ServiceUuid => Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");

        private readonly AsyncLock RadioDictionaryLock;
        private readonly Dictionary<Guid, IErosRadio> RadioDictionary;
        
        private readonly ICoreContainer<IServerResolvable> Container;
        private readonly IRepositoryService RepositoryService;
        
        public RileyLinkRadioProvider(
            IRepositoryService repositoryService,
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
            var context = Container.Get<IRepositoryContext>();
            var re = await context.Radios.Where(r => !r.IsDeleted &&
                                      r.DeviceUuid == peripheral.PeripheralUuid).FirstOrDefaultAsync();
            if (re == null)
            {
                re = new RadioEntity
                {
                    DeviceName = await peripheral.Name.FirstAsync(),
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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using OmniCore.Model.Entities;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities.Extensions;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadioProvider : IErosRadioProvider
    {
        private readonly IContainer Container;
        private readonly ConcurrentDictionary<Guid, IErosRadio> RadioDictionary;
        private readonly IRepositoryService RepositoryService;

        public RileyLinkRadioProvider(
            IRepositoryService repositoryService,
            IContainer container)
        {
            RepositoryService = repositoryService;
            Container = container;

            RadioDictionary = new ConcurrentDictionary<Guid, IErosRadio>();
        }

        public Guid ServiceUuid => Guid.Parse("0235733b-99c5-4197-b856-69219c2a3845");

        public async Task<IErosRadio> GetRadio(IBlePeripheral peripheral,
            CancellationToken cancellationToken)
        {
            if (RadioDictionary.ContainsKey(peripheral.PeripheralUuid))
                return RadioDictionary[peripheral.PeripheralUuid];
                
            var radio = await Container.Get<RileyLinkRadio>();
            var entity = await GetEntity(peripheral, cancellationToken);
            return RadioDictionary.GetOrAdd(peripheral.PeripheralUuid, uuid =>
            {
                radio.Initialize(entity, this, peripheral);
                return radio;
            });
        }
        private async Task<RadioEntity> GetEntity(IBlePeripheral peripheral, CancellationToken cancellationToken)
        {
            using var context = await RepositoryService.GetContextReadWrite(cancellationToken);
            var re = await context.Radios.Where(r => !r.IsDeleted &&
                                                     r.DeviceUuid == peripheral.PeripheralUuid)
                        .FirstOrDefaultAsync(cancellationToken: cancellationToken);
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

        public void Dispose()
        {
            foreach (var radio in RadioDictionary.Values)
                radio.Dispose();
                
            RadioDictionary.Clear();
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using OmniCore.Model.Constants;
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
        private readonly Dictionary<Guid, IErosRadio> RadioDictionary;
        private readonly IRepositoryService RepositoryService;
        private readonly AsyncLock RadioDictionaryLock;

        public RileyLinkRadioProvider(
            IRepositoryService repositoryService,
            IContainer container)
        {
            RepositoryService = repositoryService;
            Container = container;
            RadioDictionary = new Dictionary<Guid, IErosRadio>();
            RadioDictionaryLock = new AsyncLock();
        }

        public Guid ServiceUuid => Uuids.RileyLinkServiceUuid;

        public async Task<IErosRadio> GetRadio(Guid uuid,
            CancellationToken cancellationToken)
        {
            using (var _ = await RadioDictionaryLock.LockAsync(cancellationToken))
            {

                if (!RadioDictionary.ContainsKey(uuid))
                {
                    var radio = await Container.Get<RileyLinkRadio>();
                    await radio.Initialize(uuid, cancellationToken);
                }
                return RadioDictionary[uuid];
            }
        }
        
        public async Task<IErosRadio> GetRadio(IBlePeripheral peripheral,
            CancellationToken cancellationToken)
        {
            using (var _ = await RadioDictionaryLock.LockAsync(cancellationToken))
            {
                if (!RadioDictionary.ContainsKey(peripheral.PeripheralUuid))
                {
                    var radio = await Container.Get<RileyLinkRadio>();
                    await radio.Initialize(peripheral, cancellationToken);
                    RadioDictionary[peripheral.PeripheralUuid] = radio;
                }
                return RadioDictionary[peripheral.PeripheralUuid];
            }
        }

        public void Dispose()
        {
            foreach (var radio in RadioDictionary.Values)
                radio.Dispose();
                
            RadioDictionary.Clear();
        }
    }
}
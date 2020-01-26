using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using OmniCore.Model.Entities;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Interfaces.Data
{
    public interface IRepositoryContext : IDisposable, IServerResolvable
    {
        DbSet<MedicationEntity> Medications { get; }
        DbSet<UserEntity> Users { get; }
        DbSet<RadioEntity> Radios { get; }
        DbSet<RadioEventEntity> RadioEvents { get; }
        DbSet<PodEntity> Pods { get; }
        DbSet<PodRequestEntity> PodRequests { get; }
        DbSet<MedicationDeliveryEntity> MedicationDeliveries { get; }
        DbSet<PodResponseEntity> PodResponses { get; }
        Task Save(CancellationToken cancellationToken);
    }
}

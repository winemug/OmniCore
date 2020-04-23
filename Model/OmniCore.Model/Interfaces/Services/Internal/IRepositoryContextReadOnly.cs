using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniCore.Model.Entities;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IRepositoryContextReadOnly : IDisposable
    {
        DbSet<MedicationEntity> Medications { get; }
        DbSet<UserEntity> Users { get; }
        DbSet<RadioEntity> Radios { get; }
        DbSet<RadioEventEntity> RadioEvents { get; }
        DbSet<PodEntity> Pods { get; }
        DbSet<PodRequestEntity> PodRequests { get; }
        DbSet<MedicationDeliveryEntity> MedicationDeliveries { get; }
        DbSet<PodResponseEntity> PodResponses { get; }
        void SetLock(IDisposable readerWriterLock, bool tracking);
    }
}
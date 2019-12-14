using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Repository
{
    public class EntitySet<T> : IEntitySet<T> where T : class, IEntity
    {
        public DbSet<T> DbSet { get; private set; }
        public EntitySet(DbSet<T> dbSet)
        {
            DbSet = dbSet;
        }
    }
}

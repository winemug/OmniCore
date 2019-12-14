using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OmniCore.Repository.Model;

namespace OmniCore.Repository
{
    public class RepositoryContext : DbContext
    {
        public DbSet<Pod> Pods { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=core.db");
            base.OnConfiguring(optionsBuilder);
        }
    }
}

using OmniCore.Repository.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository
{
    public class SignalStrengthRepository : SqliteRepository<SignalStrength>
    {
        public SignalStrengthRepository(SQLiteAsyncConnection connection) : base(connection)
        {
        }
    }
}

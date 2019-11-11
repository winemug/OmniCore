using OmniCore.Repository.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Repository
{
    public class RadioConnectionRepository : SqliteRepository<RadioConnection>
    {
        public RadioConnectionRepository(SQLiteAsyncConnection connection) : base(connection)
        {
        }
    }
}

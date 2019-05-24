using OmniCore.Model;
using System;
using System.Data.SQLite;
using System.IO;

namespace OmniCore.Persistence
{
    public class Database
    {
        public static Database Instance = new Database();

        private string ConnectionString;
        private Database()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "omnicore.ldb");
            ConnectionString = $"Data Source=:{dbPath}";
        }

        private async void Initialize()
        {
            try
            {
                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    await conn.OpenAsync();
                    using (var tran = conn.BeginTransaction())
                    {
                        using (var cmd = new SQLiteCommand())
                        {
                            cmd.Connection = conn;
                            cmd.Transaction = tran;

                            cmd.CommandText = @"";
                            await cmd.ExecuteNonQueryAsync();
                        }
                        tran.Commit();
                    }
                }
            }
            catch (SQLiteException sle)
            {
                Console.WriteLine($"Error: {sle}");
            }
        }
    }
}

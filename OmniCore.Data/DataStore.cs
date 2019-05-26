using OmniCore.Model;
using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Data
{
    public class DataStore : IDataStore
    {
        public static DataStore Instance = new DataStore();

        private string DbPath;
        private DataStore()
        {
            DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "omnicore.db3");
        }

        public async Task Initialize()
        {
            try
            {
                var conn = new SQLiteAsyncConnection(DbPath);
                await conn.RunInTransactionAsync( async (transaction) =>
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceName = "OmniCore.Data.Scripts.v0000.sql";

                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            var sbSql = new StringBuilder();
                            while (true)
                            {
                                var line = await reader.ReadLineAsync();
                                sbSql.Append(line);
                                if (line.TrimEnd().EndsWith(";"))
                                {
                                    break;
                                }
                            }
                            var sql = sbSql.ToString();
                            Debug.WriteLine($"SQL: {sql}");
                            transaction.Execute(sql);
                        }
                    }
                    transaction.Commit();
                });
            }
            catch (SQLiteException sle)
            {
                Console.WriteLine($"Error: {sle}");
            }
        }

        public async Task<bool> Load(IPod pod)
        {
            await Initialize();
            return false;
        }

        public async Task<bool> Save(IPod pod)
        {
            throw new NotImplementedException();
        }
    }
}

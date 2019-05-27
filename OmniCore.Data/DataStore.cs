using Mono.Data.Sqlite;
using OmniCore.Model;
using OmniCore.Model.Interfaces;
using System;
using System.Data;
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
        private string DbConnectionString;
        private bool Initialized;
        private DataStore()
        {
            DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "omnicore.db3");
            DbConnectionString = $"Data Source={DbPath}";
            Initialized = false;
        }

        public void Initialize()
        {
            if (Initialized)
                return;
            try
            {
                using (var conn = new SqliteConnection(DbConnectionString))
                {
                    conn.Open();
                    var transaction = conn.BeginTransaction();
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
                                var line = reader.ReadLine();
                                sbSql.Append(line);
                                if (line.TrimEnd().EndsWith(";"))
                                {
                                    break;
                                }
                            }
                            var sql = sbSql.ToString();
                            Debug.WriteLine($"SQL: {sql}");
                            using (var cmd = new SqliteCommand(sql, conn, transaction))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    transaction.Commit();
                }
                Initialized = true;
            }
            catch (SqliteException sle)
            {
                Console.WriteLine($"Error: {sle}");
                throw sle;
            }
        }

        public bool Load(IPod pod)
        {
            using (var conn = new SqliteConnection(DbConnectionString))
            using (var cmd = new SqliteCommand(conn))
            {
                conn.Open();
                if (pod.Id.HasValue)
                {
                    cmd.CommandText = "SELECT * FROM Pods WHERE Id=?";
                    cmd.Parameters.Add(new SqliteParameter(DbType.Int32, pod.Id));
                }
                else if (pod.Serial.HasValue && pod.Lot.HasValue)
                {
                    cmd.CommandText = "SELECT * FROM Pods WHERE Lot=? AND Serial=?";
                    cmd.Parameters.Add(new SqliteParameter(DbType.UInt32, pod.Lot));
                    cmd.Parameters.Add(new SqliteParameter(DbType.UInt32, pod.Serial));
                }
                else
                {
                    cmd.CommandText = "SELECT * FROM Pods JOIN ActivePods ON PodId = Id WHERE RadioAddress=?";
                    cmd.Parameters.Add(new SqliteParameter(DbType.UInt32, pod.RadioAddress));
                }
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ParsePod(reader, pod);
                    return true;
                }
            }
            return false;
        }

        public void Save(IPod pod)
        {
            using (var conn = new SqliteConnection(DbConnectionString))
            using (var cmd = new SqliteCommand(conn))
            {
                conn.Open();
                cmd.Parameters.Add(new SqliteParameter(DbType.UInt32, pod.Lot));
                cmd.Parameters.Add(new SqliteParameter(DbType.UInt32, pod.Serial));
                cmd.Parameters.Add(new SqliteParameter(DbType.UInt32, pod.RadioAddress));
                cmd.Parameters.Add(new SqliteParameter(DbType.String, pod.VersionPm));
                cmd.Parameters.Add(new SqliteParameter(DbType.String, pod.VersionPi));
                cmd.Parameters.Add(new SqliteParameter(DbType.String, pod.VersionUnknown));
                cmd.Parameters.Add(new SqliteParameter(DbType.Int32, pod.PacketSequence));
                cmd.Parameters.Add(new SqliteParameter(DbType.Int32, pod.MessageSequence));
                if (pod.Id.HasValue)
                {
                    cmd.CommandText = "UPDATE Pods SET Lot=?,Serial=?,RadioAddress=?,VersionPm=?,VersionPi=?,VersionUnknown=?,PacketSequence=?,MessageSequence=? WHERE Id=?";
                    cmd.Parameters.Add(new SqliteParameter(DbType.Int32, pod.Id.Value));
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    cmd.CommandText = "INSERT INTO Pods(Lot,Serial,RadioAddress,VersionPm,VersionPi,VersionUnknown,PacketSequence,MessageSequence) VALUES(?,?,?,?,?,?,?,?)";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "SELECT last_insert_rowid()";
                    pod.Id = (long)cmd.ExecuteScalar();
                }
            }
        }

        private void ParsePod(SqliteDataReader r, IPod p)
        {
            p.Id = (int)r["Id"];
            p.Lot = (uint?)r["Lot"];
            p.Serial = (uint?)r["Serial"];
            p.RadioAddress = (uint)r["RadioAddress"];
            p.VersionPm = r["VersionPm"] as string;
            p.VersionPi = r["VersionPi"] as string;
            p.VersionUnknown = r["VersionUnknown"] as string;
            p.PacketSequence = (int)r["PacketSequence"];
            p.MessageSequence = (int)r["MessageSequence"];
        }
    }
}

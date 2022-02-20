using System;
using System.Dynamic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using OmniCore.Services.Entities;
using OmniCore.Services.Interfaces;
using Unity;

namespace OmniCore.Services
{
    public class ConfigurationStore
    {
        private ClientConfiguration _cc;
        
        [Dependency]
        public IPlatformInfo PlatformInfo { get; set; }
        
        private string GetPath()
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(basePath, "omnicore.json");
        }
        
        public async Task<ClientConfiguration> GetConfigurationAsync()
        {
            if (_cc == null)
            {
                _cc = await Load(GetPath());
                _cc.Platform = PlatformInfo.Platform;
                _cc.HardwareVersion = PlatformInfo.HardwareVersion;
                _cc.SoftwareVersion = PlatformInfo.SoftwareVersion;
                _cc.OsVersion = PlatformInfo.OsVersion;
            }
            return _cc;
        }

        public async Task SetConfigurationAsync(ClientConfiguration cc)
        {
            if (cc == null)
                return;
            await Save(cc, GetPath());
            _cc = cc;
        }
        
        private async Task<ClientConfiguration> Load(string path)
        {
            if (!File.Exists(path))
                return new ClientConfiguration();

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return await JsonSerializer.DeserializeAsync<ClientConfiguration>(fs);
            }
        }

        private async Task Save(ClientConfiguration cc, string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                await JsonSerializer.SerializeAsync(fs, cc);
            }
        }
    }
}
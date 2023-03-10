using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using OmniCore.Services.Entities;
using OmniCore.Services.Interfaces;
using OmniCore.Services.Interfaces.Configuration;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Platform;

namespace OmniCore.Services;

public class ConfigurationStore : IConfigurationStore
{
    private ClientConfiguration _cc;

    public async Task<ClientConfiguration> GetConfigurationAsync()
    {
        if (_cc == null)
            _cc = await Load(GetPath());
        return _cc;
    }

    public async Task SetConfigurationAsync(ClientConfiguration cc)
    {
        if (cc == null)
            return;
        await Save(cc, GetPath());
        _cc = cc;
    }

    private string GetPath()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(basePath, "omnicore.json");
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
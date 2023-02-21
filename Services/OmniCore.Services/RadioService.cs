using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Plugin.BLE;

namespace OmniCore.Services
{
    public class RadioService
    {
        private List<Radio> _radios;

        public RadioService()
        {
        }

        public void Start()
        {
            Debug.WriteLine("starting radios");
            _radios = new List<Radio>()
            {
                new Radio(Guid.Parse("00000000-0000-0000-0000-bc33acb95371"), "ema"),
                //new Radio(Guid.Parse("00000000-0000-0000-0000-886b0ff897cf"), "mod"),
                //new Radio(Guid.Parse("00000000-0000-0000-0000-c2c42b149fe4"), "ora"),
            };
        }
        public void Stop()
        {
            Debug.WriteLine("stopping radios");
            foreach (var radio in _radios)
            {
                radio.Dispose();
            }
            _radios = null;
        }

        public async Task<RadioConnection> GetConnectionAsync(string name,
            CancellationToken cancellationToken = default)
        {
            var radio = _radios.Where(r => r.Name == name).FirstOrDefault();
            if (radio == null)
                return null;
            var allocationLockDisposable = await radio.LockAsync(cancellationToken);
            return new RadioConnection(radio, allocationLockDisposable);
        }
    }
}
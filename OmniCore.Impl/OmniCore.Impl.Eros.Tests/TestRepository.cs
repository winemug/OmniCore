using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Impl.Eros.Tests
{
    public class TestRepository : IPodRepository
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<IList<T>> GetActivePods<T>() where T : IPod, new()
        {
            throw new NotImplementedException();
        }

        public Task SavePod<T>(T pod) where T : IPod, new()
        {
            throw new NotImplementedException();
        }
    }
}

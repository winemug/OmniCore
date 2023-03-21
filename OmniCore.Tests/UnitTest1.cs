using Moq;
using OmniCore.Services;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Services.Interfaces.Pod;
using OmniCore.Services.Interfaces.Radio;

namespace OmniCore.Tests
{
    public class Tests
    {

        private IDataService _dataService;
        private IRadioConnection _radioConnection;
        private IPod _pod;
        
        [SetUp]
        public void Setup()
        {
            _dataService = new Mock<IDataService>().Object;
            _pod = new Pod(_dataService);
            _radioConnection = new MockRadioConnection(_pod);
        }

        private async Task<PodConnection> GetPodConnectionAsync()
        {
            var podLock = await _pod.LockAsync(CancellationToken.None);
            return new PodConnection(_pod, _radioConnection, podLock, _dataService);
        }
        
        [Test]
        public async Task Test1()
        {
            using (var conn = await GetPodConnectionAsync())
            {
                await conn.UpdateStatus();
            }
        }
    }
}
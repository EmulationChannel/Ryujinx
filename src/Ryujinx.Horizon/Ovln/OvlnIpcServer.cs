using Ryujinx.Horizon.Sdk.Sf.Hipc;
using Ryujinx.Horizon.Sdk.Sm;

namespace Ryujinx.Horizon.Ovln
{
    class OvlnIpcServer
    {
        private const int OvlnRcvMaxSessionsCount = 2;
        private const int OvlnSndMaxSessionsCount = 20;
        private const int TotalMaxSessionsCount = OvlnRcvMaxSessionsCount + OvlnSndMaxSessionsCount;

        private const int PointerBufferSize = 0;
        private const int MaxDomains = 21;
        private const int MaxDomainObjects = 60;
        private const int MaxPortsCount = 2;

        private static readonly ManagerOptions _options = new(PointerBufferSize, MaxDomains, MaxDomainObjects, false);

        private SmApi _sm;
        private ServerManager _serverManager;

        public void Initialize()
        {
            HeapAllocator allocator = new();

            _sm = new SmApi();
            _sm.Initialize().AbortOnFailure();

            _serverManager = new ServerManager(allocator, _sm, MaxPortsCount, _options, TotalMaxSessionsCount);
        }

        public void ServiceRequests()
        {
            _serverManager.ServiceRequests();
        }

        public void Shutdown()
        {
            _serverManager.Dispose();
            _sm.Dispose();
        }
    }
}

using Ryujinx.Common.Logging;
using Ryujinx.Horizon.Bcat.Types;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Bcat;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Bcat.Ipc
{
    partial class BcatService : IBcatService
    {
        public BcatService(BcatServicePermissionLevel permissionLevel) { }

        [CmifCommand(10100)]
        public Result RequestSyncDeliveryCache(out IDeliveryCacheProgressService deliveryCacheProgressService)
        {
            deliveryCacheProgressService = new DeliveryCacheProgressService();

            return Result.Success;
        }
        
        [CmifCommand(30300)]
        // RegisterSystemApplicationDeliveryTasks
        public Result RegisterSystemApplicationDeliveryTasks()
        {
            Logger.Stub?.PrintStub(LogClass.ServiceBcat);
            return Result.Success;
        }
    }
}

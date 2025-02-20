using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.News
{
    class INewsService : IpcService
    {
        public INewsService(ServiceCtx context) { }
        
        [CommandCmif(30100)]
        // GetSubscriptionStatus() -> u32
        public ResultCode GetSubscriptionStatus(ServiceCtx context)
        {
            // TODO: Implement this properly
            Logger.Stub?.PrintStub(LogClass.Service);
            context.ResponseData.Write((uint)0);
            return ResultCode.Success;
        }
        
        [CommandCmif(30200)]
        // IsSystemUpdateRequired() -> bool
        public ResultCode IsSystemUpdateRequired(ServiceCtx context)
        {
            // TODO: Implement this properly
            Logger.Stub?.PrintStub(LogClass.Service);
            context.ResponseData.Write(false);
            return ResultCode.Success;
        }
        
        [CommandCmif(40101)]
        // RequestAutoSubscription()
        public ResultCode RequestAutoSubscription(ServiceCtx context)
        {
            // TODO: Implement this properly
            Logger.Stub?.PrintStub(LogClass.Service);
            return ResultCode.Success;
        }
    }
}

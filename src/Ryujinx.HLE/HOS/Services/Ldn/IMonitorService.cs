using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Nfc.NfcManager;

namespace Ryujinx.HLE.HOS.Services.Ldn
{
    class IMonitorService : IpcService
    {
        public IMonitorService() { }
        
        [CommandCmif(0)]
        // GetStateForMonitor() -> u32
        public ResultCode GetStateForMonitor(ServiceCtx context)
        {
            State state = State.Initialized;
            context.ResponseData.Write((uint)state);
            
            Logger.Stub?.PrintStub(LogClass.ServiceLdn);
            return ResultCode.Success;
        }
        
        [CommandCmif(100)]
        // InitializeMonitor()
        public ResultCode InitializeMonitor(ServiceCtx context)
        {
            Logger.Info?.PrintStub(LogClass.ServiceLdn);
            return ResultCode.Success;
        }
        
        [CommandCmif(288)]
        // ???()
        public ResultCode Unknown288(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceLdn);
            return ResultCode.Success;
        }
    }
}

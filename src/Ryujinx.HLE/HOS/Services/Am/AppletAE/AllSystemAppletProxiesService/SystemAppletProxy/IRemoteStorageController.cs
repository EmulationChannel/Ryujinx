using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IRemoteStorageController : IpcService
    {
        [CommandCmif(14)]
        // GetDataNewnessByApplicationId()
        public ResultCode GetAutonomyTaskStatus(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);
            return ResultCode.Success;
        }

        [CommandCmif(18)] // [7.0.0+]
        // GetDataInfo()
        public ResultCode GetDataInfo(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);
            return ResultCode.Success;
        }
        
        [CommandCmif(22)] // [11.0.0+] 
        // GetLoadedDataInfo()
        public ResultCode GetLoadedDataInfo(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);
            return ResultCode.Success;
        }
    }
}

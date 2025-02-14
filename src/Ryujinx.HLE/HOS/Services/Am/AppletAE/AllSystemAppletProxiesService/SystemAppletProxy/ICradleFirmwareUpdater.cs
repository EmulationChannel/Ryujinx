using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class ICradleFirmwareUpdater : IpcService
    {
        [CommandCmif(1)]
        // Finish()
        public ResultCode Finish(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);
            return ResultCode.Success;
        }
        
        [CommandCmif(2)]
        // GetUpdateDeviceStatus()
        public ResultCode GetUpdateDeviceStatus(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);
            return ResultCode.Success;
        }
        
        [CommandCmif(3)]
        // GetUpdateProgress()
        public ResultCode GetUpdateProgress(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);
            return ResultCode.Success;
        }
    }
}

using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IWindowController : IpcService
    {
        private readonly ulong _pid;

        public IWindowController(ulong pid)
        {
            _pid = pid;
        }

        [CommandCmif(1)]
        // GetAppletResourceUserId() -> nn::applet::AppletResourceUserId
        public ResultCode GetAppletResourceUserId(ServiceCtx context)
        {
            ulong appletResourceUserId = _pid;

            context.ResponseData.Write(appletResourceUserId);

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // GetAppletResourceUserIdOfCallerApplet() -> nn::applet::AppletResourceUserId
        public ResultCode GetAppletResourceUserIdOfCallerApplet(ServiceCtx context)
        {
            ulong appletResourceUserId = _pid;
            appletResourceUserId = context.Device.System.WindowSystem.GetByAruId(_pid).CallerApplet.ProcessHandle.TitleId;
            context.ResponseData.Write(appletResourceUserId);
            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { appletResourceUserId });
            return ResultCode.Success;
        }
        
        [CommandCmif(10)]
        // AcquireForegroundRights()
        public ResultCode AcquireForegroundRights(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);
            context.Device.System.WindowSystem.PauseOldWindows(_pid);

            return ResultCode.Success;
        }
    }
}

using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IAppletCommonFunctions : IpcService
    {
        public IAppletCommonFunctions() { }
        
        [CommandCmif(51)]
        // GetHomeButtonDoubleClickEnabled() -> bool
        public ResultCode GetHomeButtonDoubleClickEnabled(ServiceCtx context)
        {
            context.ResponseData.Write(true);
            Logger.Info?.PrintStub(LogClass.ServiceAm);
            return ResultCode.Success;
        }
        
        [CommandCmif(70)]
        // SetCpuBoostRequestPriority(s32) -> void
        public ResultCode SetCpuBoostRequestPriority(ServiceCtx context)
        {
            Logger.Info?.PrintStub(LogClass.ServiceAm);
            return ResultCode.Success;
        }
        
        [CommandCmif(100)]
        // SetApplicationCoreUsageMode()
        public ResultCode SetApplicationCoreUsageMode(ServiceCtx context)
        {
            Logger.Info?.PrintStub(LogClass.ServiceAm);
            return ResultCode.Success;
        }
        
        [CommandCmif(300)] // 17.0.0+
        // GetCurrentApplicationId() -> nn::am::detail::IApplicationId
        public ResultCode GetCurrentApplicationId(ServiceCtx context)
        {
            Logger.Info?.PrintStub(LogClass.ServiceAm);
            context.ResponseData.Write(context.Device.System.WindowSystem.GetApplicationApplet().ProcessHandle.TitleId);
            return ResultCode.Success;
        }
    }
}

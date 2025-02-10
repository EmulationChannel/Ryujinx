using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IAppletCommonFunctions : IpcService
    {
        public IAppletCommonFunctions() { }
        
        [CommandCmif(70)]
        // SetCpuBoostRequestPriority(s32) -> void
        public ResultCode SetCpuBoostRequestPriority(ServiceCtx context)
        {
            Logger.Info?.PrintStub(LogClass.ServiceAm);
            // NOTE: Stubbed in original implementation.
            return ResultCode.Success;
        }
    }
}

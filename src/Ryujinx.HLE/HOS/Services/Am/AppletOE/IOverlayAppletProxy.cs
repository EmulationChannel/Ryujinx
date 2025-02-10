using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy;
using Ryujinx.HLE.HOS.Services.Am.AppletOE.ApplicationProxyService;

namespace Ryujinx.HLE.HOS.Services.Am
{
    class IOverlayAppletProxy : IApplicationProxy
    {
        public IOverlayAppletProxy(ulong pid) : base(pid)
        {
        }
        
        [CommandCmif(21)]
        // GetAppletCommonFunctions() -> object<nn::am::service::IAppletCommonFunctions>
        public ResultCode GetAppletCommonFunctions(ServiceCtx context)
        {
            MakeObject(context, new IAppletCommonFunctions());

            return ResultCode.Success;
        }
        [CommandCmif(23)]
        // GetGlobalStateController() -> object<nn::am::service::IGlobalStateController>
        public ResultCode GetGlobalStateController(ServiceCtx context)
        {
            MakeObject(context, new IGlobalStateController(context));

            return ResultCode.Success;
        }
    }
}

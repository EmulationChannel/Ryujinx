using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.LibraryAppletCreator;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IApplicationCreator : IpcService
    {
        public IApplicationCreator() { }
        
        [CommandCmif(0)]
        // CreateApplication(u64) -> IApplicationAccessor
        public ResultCode CreateApplication(ServiceCtx context)
        {
            ulong appID = context.RequestData.ReadUInt64();
            Logger.Stub?.Print(LogClass.Loader, $"Stubbed. IApplicationAccessor: 0x{appID:X8}");
            
            MakeObject(context,new IApplicationAccessor(appID,context));
            return ResultCode.Success;
        }
    }
}

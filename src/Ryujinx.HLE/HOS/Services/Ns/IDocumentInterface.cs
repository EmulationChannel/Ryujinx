using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    class IDocumentInterface : IpcService
    {
        [CommandCmif(23)]
        // ResolveApplicationContentPath()
        public ResultCode ResolveApplicationContentPath(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNs);
            return ResultCode.Success;
        }
        
        [CommandCmif(92)] // 5.0.0+
        // GetRunningApplicationProgramId() -> u64
        public ResultCode GetRunningApplicationProgramId(ServiceCtx context)
        {
            ulong appletResourceUserId = 0x0100000000001000;
            if (context.Device.System.WindowSystem.GetApplicationApplet() != null)
            {
                appletResourceUserId = context.Device.System.WindowSystem.GetApplicationApplet().ProcessHandle.TitleId;
            }

            context.ResponseData.Write(appletResourceUserId);
            return ResultCode.Success;
        }
    }
}

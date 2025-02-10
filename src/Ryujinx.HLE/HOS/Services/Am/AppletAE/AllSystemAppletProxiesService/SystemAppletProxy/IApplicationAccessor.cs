using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.Horizon.Common;
using System.Linq;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IApplicationAccessor : IpcService
    {
        public ulong TitleId { get; }
        public KEvent appletStateChangeEvent;
        public int appletStateChangeHandle;
        public AppletStateMgr stateMgr;
        public IApplicationAccessor(ulong titleId, ServiceCtx context)
        {
            TitleId = titleId;
            appletStateChangeEvent = new KEvent(context.Device.System.KernelContext);
            appletStateChangeHandle = -1;
            stateMgr = context.Device.System.AppletState;
        }

        [CommandCmif(0)]
        // GetAppletStateChangedEvent() -> handle<copy>
        public ResultCode GetAppletStateChangedEvent(ServiceCtx context)
        {
            if (appletStateChangeHandle != -1)
            {
                var result = context.Process.HandleTable.GenerateHandle(appletStateChangeEvent.ReadableEvent, out appletStateChangeHandle);

                if (result != Result.Success)
                {
                    return (ResultCode)result.ErrorCode;
                }
            }
            
            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(appletStateChangeHandle);
            
            return ResultCode.Success;
        }
        
        [CommandCmif(10)]
        // Start()
        public ResultCode Start(ServiceCtx context)
        {
            var apps = context.Device.Configuration.Titles;
            var app = apps.FirstOrDefault(x => x.AppId.Value == TitleId);

            string path = app.Path;
            bool isNsp = path.EndsWith(".nsp");
            context.Device.System.CreateNewAppletManager();
            if (isNsp)
            {
                context.Device.Processes.LoadNsp(path, TitleId);
            }
            else
            {
                context.Device.Processes.LoadXci(path, TitleId);
            }
            
            return ResultCode.Success;
        }
        
        [CommandCmif(101)]
        // RequestForApplicationToGetForeground()
        public ResultCode RequestForApplicationToGetForeground(ServiceCtx context)
        {
            return ResultCode.Success;
        }

    }
}

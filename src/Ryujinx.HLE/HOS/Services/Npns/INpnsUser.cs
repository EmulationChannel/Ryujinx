using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Npns
{
    [Service("npns:u")]
    class INpnsUser : IpcService
    {
        public KEvent receiveEvent;
        public int receiveEventHandle = 0;

        public INpnsUser(ServiceCtx context)
        {
            receiveEvent = new KEvent(context.Device.System.KernelContext);
        }

        [CommandCmif(5)]
        // GetReceiveEvent() -> handle(copy)
        public ResultCode GetReceiveEvent(ServiceCtx context)
        {
            if (receiveEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(receiveEvent.ReadableEvent, out receiveEventHandle) !=
                    Result.Success)
                {
                    throw new InvalidOperationException("Out of handles");
                }
            }
            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(receiveEventHandle);
            return ResultCode.Success;
        }
        
        [CommandCmif(8)] // 18.0.0+
        // ListenToByName()
        public ResultCode ListenToByName(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNpns);
            return ResultCode.Success;
        }
    }
}

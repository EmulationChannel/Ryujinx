using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Npns
{
    [Service("npns:s")]
    class INpnsSystem : IpcService
    {
        public KEvent ListenEvent;
        public int ListenHandle = 0;
        public INpnsSystem(ServiceCtx context)
        {
            ListenEvent = new KEvent(context.Device.System.KernelContext);
        }
        
        [CommandCmif(2)]
        // ListenTo(u64)
        public ResultCode ListenTo(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNpns);
            return ResultCode.Success;
        }
        
        [CommandCmif(5)]
        // GetReceiveEvent() -> handle<copy>
        public ResultCode GetReceiveEvent(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNpns);
            if (ListenHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(ListenEvent.ReadableEvent, out ListenHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }
            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(ListenHandle);
            return ResultCode.Success;
        }
        
        [CommandCmif(8)]
        // ListenToByName()
        public ResultCode ListenToByName(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNpns);
            return ResultCode.Success;
        }
    }
}

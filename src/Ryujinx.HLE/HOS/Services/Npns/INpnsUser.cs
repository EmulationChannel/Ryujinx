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
        public KEvent ListenEvent { get; }
        public int ListenHandle = 0;
        public INpnsUser(ServiceCtx context)
        {
            ListenEvent = new KEvent(context.Device.System.KernelContext);
        }
        
        [CommandCmif(2)]
        // ListenTo(u64)
        public ResultCode ListenTo(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAcc);
            return ResultCode.Success;
        }
        [CommandCmif(5)]
        // GetReceiveEvent() -> handle<copy>
        public ResultCode GetReceiveEvent(ServiceCtx context)
        {
            if (ListenHandle == 0)
            {
                if (Result.Success!=context.Process.HandleTable.GenerateHandle(ListenEvent.ReadableEvent, out ListenHandle))
                {
                    throw new InvalidOperationException();
                }
            }
            
            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(ListenHandle);
            return ResultCode.Success;
        }
    }
}

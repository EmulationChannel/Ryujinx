using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.News
{
    class IOverwriteEventHolder : IpcService
    {
        KEvent _overwriteEvent;
        int _overwriteEventHandle;
        public IOverwriteEventHolder(ServiceCtx context)
        {
            _overwriteEvent = new KEvent(context.Device.System.KernelContext);
            _overwriteEventHandle = -1;
        }
        
        [CommandCmif(0)]
        // Get() -> handle<copy>
        public ResultCode Get(ServiceCtx context)
        {
            if (_overwriteEventHandle == -1)
            {
                Result resultCode = context.Process.HandleTable.GenerateHandle(_overwriteEvent.ReadableEvent, out _overwriteEventHandle);

                if (resultCode != Result.Success)
                {
                    return (ResultCode)resultCode.ErrorCode;
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_overwriteEventHandle);

            Logger.Stub?.PrintStub(LogClass.Service);
            
            return ResultCode.Success;
        }
    }
}

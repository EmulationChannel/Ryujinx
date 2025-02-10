using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.News;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Notification
{
    class INotificationSystemEventAccessor : IpcService
    {
        KEvent _systemEvent;
        int _systemEventHandle;
        public INotificationSystemEventAccessor(ServiceCtx context)
        {
            _systemEvent = new KEvent(context.Device.System.KernelContext);
            _systemEventHandle = -1;
        }
        
        [CommandCmif(0)]
        // GetSystemEvent() -> handle<copy>
        public ResultCode GetSystemEvent(ServiceCtx context)
        {
            if (_systemEventHandle == -1)
            {
                Result resultCode = context.Process.HandleTable.GenerateHandle(_systemEvent.ReadableEvent, out _systemEventHandle);

                if (resultCode != Result.Success)
                {
                    return (ResultCode)resultCode.ErrorCode;
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_systemEventHandle);

            Logger.Stub?.PrintStub(LogClass.Service);
            
            return ResultCode.Success;
        }
    }
}

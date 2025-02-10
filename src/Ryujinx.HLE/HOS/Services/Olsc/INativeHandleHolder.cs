using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;

namespace Ryujinx.HLE.HOS.Services.Olsc
{
    class INativeHandleHolder : IpcService
    {
        KEvent _nativeHandle;
        int _handle;
        public INativeHandleHolder(ServiceCtx ctx)
        {
            _nativeHandle = new KEvent(ctx.Device.System.KernelContext);
            _handle = -1;
        }
        
        [CommandCmif(0)]
        // GetNativeHandle() -> handle<copy>
        public ResultCode GetNativeHandle(ServiceCtx context)
        {
            if (_handle == -1)
            {
                Result resultCode = context.Process.HandleTable.GenerateHandle(_nativeHandle.ReadableEvent, out _handle);

                if (resultCode != Result.Success)
                {
                    return (ResultCode)resultCode.ErrorCode;
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_handle);

            Logger.Stub?.PrintStub(LogClass.Application);
            
            return ResultCode.Success;
        }
    }
}

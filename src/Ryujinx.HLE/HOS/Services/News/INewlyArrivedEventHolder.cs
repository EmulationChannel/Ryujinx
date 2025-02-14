using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;

namespace Ryujinx.HLE.HOS.Services.News
{
    class INewlyArrivedEventHolder : IpcService
    {
        KEvent _newlyArrivedEvent;
        int _newlyArrivedEventHandle;
        
        public INewlyArrivedEventHolder(ServiceCtx context)
        {
            _newlyArrivedEvent = new KEvent(context.Device.System.KernelContext);
            _newlyArrivedEventHandle = -1;
        }
        
        [CommandCmif(0)]
        // Get() -> handle<copy>
        public ResultCode Get(ServiceCtx context)
        {
            if (_newlyArrivedEventHandle == -1)
            {
                Result resultCode = context.Process.HandleTable.GenerateHandle(_newlyArrivedEvent.ReadableEvent, out _newlyArrivedEventHandle);

                if (resultCode != Result.Success)
                {
                    return (ResultCode)resultCode.ErrorCode;
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_newlyArrivedEventHandle);

            Logger.Stub?.PrintStub(LogClass.Service);
            return ResultCode.Success;
        }
    }
}

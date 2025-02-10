using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
namespace Ryujinx.HLE.HOS.Services.Ovln
{
    class IReceiver : IpcService
    {
        private KEvent _receiveEvent;
        private int _receiveEventHandle;
        public IReceiver(ServiceCtx context)
        {
            _receiveEvent = new KEvent(context.Device.System.KernelContext);
            _receiveEventHandle = -1;
        }
        
        [CommandCmif(0)]
        // AddSource()
        public ResultCode AddSource(ServiceCtx context)
        {
            return ResultCode.Success;
        }
        [CommandCmif(2)]
        // GetReceiveEventHandle() -> handle<copy>
        public ResultCode GetReceiveEventHandle(ServiceCtx context)
        {
            if (_receiveEventHandle == -1)
            {
                Result resultCode = context.Process.HandleTable.GenerateHandle(_receiveEvent.ReadableEvent, out _receiveEventHandle);

                if (resultCode != Result.Success)
                {
                    return (ResultCode)resultCode.ErrorCode;
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_receiveEventHandle);

            Logger.Stub?.PrintStub(LogClass.Audio);
            return ResultCode.Success;
        }
    }
}

using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Ns.Types;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    [Service("ns:su")]
    class ISystemUpdateInterface : IpcService
    {
        KEvent _systemUpdateEvent;
        int _systemUpdateEventHandle;
        public ISystemUpdateInterface(ServiceCtx context)
        {
            _systemUpdateEvent = new KEvent(context.Device.System.KernelContext);
            _systemUpdateEventHandle = -1;
        }
        
        [CommandCmif(0)]
        // GetBackgroundNetworkUpdateState() -> u32
        public ResultCode GetBackgroundNetworkUpdateState(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.Service);
            context.ResponseData.Write((byte)BackgroundNetworkUpdateState.None);
            return ResultCode.Success;
        }
        
        [CommandCmif(9)]
        // GetSystemUpdateNotificationEventForContentDelivery() -> handle<copy>
        public ResultCode GetSystemUpdateNotificationEventForContentDelivery(ServiceCtx context)
        {
            if (_systemUpdateEventHandle == -1)
            {
                Result resultCode = context.Process.HandleTable.GenerateHandle(_systemUpdateEvent.ReadableEvent, out _systemUpdateEventHandle);

                if (resultCode != Result.Success)
                {
                    return (ResultCode)resultCode.ErrorCode;
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_systemUpdateEventHandle);

            Logger.Stub?.PrintStub(LogClass.Service);
            return ResultCode.Success;
        }
    }
}

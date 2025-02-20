using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IGlobalStateController : IpcService
    {
        KEvent _hdcpAuthenticationFailedEvent;
        int _hdcpAuthenticationFailedEventHandle;
        public IGlobalStateController(ServiceCtx context)
        {
            _hdcpAuthenticationFailedEvent = new KEvent(context.Device.System.KernelContext);
            _hdcpAuthenticationFailedEventHandle = -1;
        }
        
        [CommandCmif(14)]
        // ShouldSleepOnBoot() -> u8
        public ResultCode ShouldSleepOnBoot(ServiceCtx context)
        {
            context.ResponseData.Write(false);
            return ResultCode.Success;
        }
        
        [CommandCmif(15)]
        // GetHdcpAuthenticationFailedEvent() -> handle<copy>
        public ResultCode GetHdcpAuthenticationFailedEvent(ServiceCtx context)
        {
            if (_hdcpAuthenticationFailedEventHandle == -1)
            {
                Result resultCode = context.Process.HandleTable.GenerateHandle(_hdcpAuthenticationFailedEvent.ReadableEvent, out _hdcpAuthenticationFailedEventHandle);

                if (resultCode != Result.Success)
                {
                    return (ResultCode)resultCode.ErrorCode;
                }
            }
            
            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_hdcpAuthenticationFailedEventHandle);
            
            Logger.Stub?.PrintStub(LogClass.ServiceAm);
            return ResultCode.Success;
        }
        
        [CommandCmif(30)]
        // OpenCradleFirmwareUpdater() -> ICradleFirmwareUpdater. 
        public ResultCode OpenCradleFirmwareUpdater(ServiceCtx context)
        {
            MakeObject(context, new ICradleFirmwareUpdater());
            return ResultCode.Success;
        }
    }
}

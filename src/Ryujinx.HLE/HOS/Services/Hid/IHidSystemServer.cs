using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Hid.HidServer;
using Ryujinx.HLE.HOS.Services.Hid.Types;
using Ryujinx.Horizon.Common;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    [Service("hid:sys")]
    class IHidSystemServer : IpcService
    {
        KEvent _connectionTriggerTimeoutEvent;
        int _connectionTriggerTimeoutEventHandle;
        KEvent _deviceRegisteredEventForControllerSupport;
        int _deviceRegisteredEventForControllerSupportHandle;
        KEvent _joyDetachOnBluetoothOffEvent;
        int _joyDetachOnBluetoothOffEventHandle;

        public IHidSystemServer(ServiceCtx context)
        {
            _connectionTriggerTimeoutEvent = new KEvent(context.Device.System.KernelContext);
            _connectionTriggerTimeoutEventHandle = -1;
            _deviceRegisteredEventForControllerSupport = new KEvent(context.Device.System.KernelContext);
            _deviceRegisteredEventForControllerSupportHandle = -1;
            _joyDetachOnBluetoothOffEvent = new KEvent(context.Device.System.KernelContext);
            _joyDetachOnBluetoothOffEventHandle = -1;
        }

        [CommandCmif(303)]
        // ApplyNpadSystemCommonPolicy(u64)
        public ResultCode ApplyNpadSystemCommonPolicy(ServiceCtx context)
        {
            ulong commonPolicy = context.RequestData.ReadUInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { commonPolicy });

            return ResultCode.Success;
        }

        [CommandCmif(306)]
        // GetLastActiveNpad(u32) -> u8, u8
        public ResultCode GetLastActiveNpad(ServiceCtx context)
        {
            context.ResponseData.Write((byte)context.Device.Hid.Npads.GetLastActiveNpadId());

            return ResultCode.Success;
        }
        
        [CommandCmif(544)]
        // AcquireConnectionTriggerTimeoutEvent() -> handle<copy>
        public ResultCode AcquireConnectionTriggerTimeoutEvent(ServiceCtx context)
        {
            if (_connectionTriggerTimeoutEventHandle == -1)
            {
                Result resultCode = context.Process.HandleTable.GenerateHandle(_connectionTriggerTimeoutEvent.ReadableEvent, out _connectionTriggerTimeoutEventHandle);

                if (resultCode != Result.Success)
                {
                    return (ResultCode)resultCode.ErrorCode;
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_connectionTriggerTimeoutEventHandle);

            Logger.Stub?.PrintStub(LogClass.ServiceHid);

            return ResultCode.Success;
        }
        
        [CommandCmif(546)]
        // AcquireDeviceRegisteredEventForControllerSupport() -> handle<copy>
        public ResultCode AcquireDeviceRegisteredEventForControllerSupport(ServiceCtx context)
        {
            if (_deviceRegisteredEventForControllerSupportHandle == -1)
            {
                Result resultCode = context.Process.HandleTable.GenerateHandle(_deviceRegisteredEventForControllerSupport.ReadableEvent, out _deviceRegisteredEventForControllerSupportHandle);

                if (resultCode != Result.Success)
                {
                    return (ResultCode)resultCode.ErrorCode;
                }
            }
            
            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_deviceRegisteredEventForControllerSupportHandle);
            
            Logger.Stub?.PrintStub(LogClass.ServiceHid);

            return ResultCode.Success;
        }
        
        [CommandCmif(703)]
        // GetUniquePadIds() -> (u64, buffer<nn::hid::system::UniquePadId, 0xa>)
        public ResultCode GetUniquePadIds(ServiceCtx context)
        {
            // Todo: This is probably wrong and shFAcquireRadioEvent:ould be a list of UniquePadIds instead.
            context.ResponseData.Write(0); // Number of unique pad ids.
            Logger.Stub?.PrintStub(LogClass.ServiceHid);
            return ResultCode.Success;
        }
        
        [CommandCmif(751)]
        // AcquireJoyDetachOnBluetoothOffEventHandle(nn::applet::AppletResourceUserId, pid) -> handle<copy>
        public ResultCode AcquireJoyDetachOnBluetoothOffEventHandle(ServiceCtx context)
        {
            var appletResourceUserId = context.RequestData.ReadUInt32();
            var pid = context.RequestData.ReadUInt32();
            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { appletResourceUserId, pid });
            if (_joyDetachOnBluetoothOffEventHandle == -1)
            {
                Result resultCode = context.Process.HandleTable.GenerateHandle(_joyDetachOnBluetoothOffEvent.ReadableEvent, out _joyDetachOnBluetoothOffEventHandle);

                if (resultCode != Result.Success)
                {
                    return (ResultCode)resultCode.ErrorCode;
                }
            }
            
            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_joyDetachOnBluetoothOffEventHandle);
            
            Logger.Stub?.PrintStub(LogClass.ServiceHid);

            return ResultCode.Success;
        }
        
        [CommandCmif(850)]
        // IsUsbFullKeyControllerEnabled() -> b8
        public ResultCode IsUsbFullKeyControllerEnabled(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceHid);
            context.ResponseData.Write(false);
            return ResultCode.Success;
        }
        
        [CommandCmif(1000)]
        // InitializeFirmwareUpdate()
        public ResultCode InitializeFirmwareUpdate(ServiceCtx context)
        {
            Logger.Info?.PrintStub(LogClass.ServiceHid);
            return ResultCode.Success;
        }
        
        [CommandCmif(1135)]
        // InitializeUsbFirmwareUpdateWithoutMemory()
        public ResultCode InitializeUsbFirmwareUpdateWithoutMemory(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceHid);
            return ResultCode.Success;
        }
        
        [CommandCmif(1153)]
        // GetTouchScreenDefaultConfiguration()
        public ResultCode GetTouchScreenDefaultConfiguration(ServiceCtx context)
        {
            // TODO: Implement touch screen default configuration.
            Logger.Stub?.PrintStub(LogClass.ServiceHid);
            return ResultCode.Success;
        }

        [CommandCmif(307)]
        // GetNpadSystemExtStyle() -> u64
        public ResultCode GetNpadSystemExtStyle(ServiceCtx context)
        {
            foreach (PlayerIndex playerIndex in context.Device.Hid.Npads.GetSupportedPlayers())
            {
                if (HidUtils.GetNpadIdTypeFromIndex(playerIndex) > NpadIdType.Handheld)
                {
                    return ResultCode.InvalidNpadIdType;
                }
            }

            context.ResponseData.Write((ulong)context.Device.Hid.Npads.SupportedStyleSets);

            return ResultCode.Success;
        }

        [CommandCmif(314)] // 9.0.0+
        // GetAppletFooterUiType(u32) -> u8
        public ResultCode GetAppletFooterUiType(ServiceCtx context)
        {
            ResultCode resultCode = GetAppletFooterUiTypeImpl(context, out AppletFooterUiType appletFooterUiType);

            context.ResponseData.Write((byte)appletFooterUiType);

            return resultCode;
        }

        private ResultCode GetAppletFooterUiTypeImpl(ServiceCtx context, out AppletFooterUiType appletFooterUiType)
        {
            NpadIdType npadIdType = (NpadIdType)context.RequestData.ReadUInt32();
            PlayerIndex playerIndex = HidUtils.GetIndexFromNpadIdType(npadIdType);

            appletFooterUiType = context.Device.Hid.SharedMemory.Npads[(int)playerIndex].InternalState.AppletFooterUiType;
            return ResultCode.Success;
        }
    }
}

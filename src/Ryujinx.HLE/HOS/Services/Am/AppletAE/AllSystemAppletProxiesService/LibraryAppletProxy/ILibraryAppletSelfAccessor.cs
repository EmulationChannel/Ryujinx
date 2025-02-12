using Ryujinx.Common;
using Ryujinx.HLE.HOS.Applets;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.LibraryAppletProxy
{
    class ILibraryAppletSelfAccessor : IpcService
    {
        private readonly AppletStandalone _appletStandalone = new();

        public ILibraryAppletSelfAccessor(ServiceCtx context)
        {
            if (context.Device.Processes.ActiveApplication.ProgramId == 0x0100000000001009)
            {
                // Create MiiEdit data.
                _appletStandalone = new AppletStandalone()
                {
                    AppletId = AppletId.MiiEdit,
                    LibraryAppletMode = LibraryAppletMode.AllForeground,
                };

                byte[] miiEditInputData = new byte[0x100];
                miiEditInputData[0] = 0x03; // Hardcoded unknown value.

                _appletStandalone.InputData.Enqueue(miiEditInputData);
            }
            else if (context.Device.Processes.ActiveApplication.ProgramId == 0x010000000000100D)
            {
                _appletStandalone = new AppletStandalone()
                {
                    AppletId = AppletId.PhotoViewer,
                    LibraryAppletMode = LibraryAppletMode.AllForeground,
                };

                CommonArguments arguments = new CommonArguments();
                ReadOnlySpan<byte> data = MemoryMarshal.Cast<CommonArguments, byte>(MemoryMarshal.CreateReadOnlySpan(ref arguments, 1));
                byte[] argumentsBytes = data.ToArray();
                _appletStandalone.InputData.Enqueue(argumentsBytes);
                byte[] optionBytes = BitConverter.GetBytes(1);
                _appletStandalone.InputData.Enqueue(optionBytes);
            }
            else
            {
                throw new NotImplementedException($"{context.Device.Processes.ActiveApplication.ProgramId} applet is not implemented.");
            }
        }

        [CommandCmif(0)]
        // PopInData() -> object<nn::am::service::IStorage>
        public ResultCode PopInData(ServiceCtx context)
        {
            byte[] appletData = _appletStandalone.InputData.Dequeue();

            if (appletData.Length == 0)
            {
                return ResultCode.NotAvailable;
            }

            MakeObject(context, new IStorage(appletData));

            return ResultCode.Success;
        }

        [CommandCmif(11)]
        // GetLibraryAppletInfo() -> nn::am::service::LibraryAppletInfo
        public ResultCode GetLibraryAppletInfo(ServiceCtx context)
        {
            LibraryAppletInfo libraryAppletInfo = new()
            {
                AppletId = _appletStandalone.AppletId,
                LibraryAppletMode = _appletStandalone.LibraryAppletMode,
            };

            context.ResponseData.WriteStruct(libraryAppletInfo);

            return ResultCode.Success;
        }

        [CommandCmif(14)]
        // GetCallerAppletIdentityInfo() -> nn::am::service::AppletIdentityInfo
        public ResultCode GetCallerAppletIdentityInfo(ServiceCtx context)
        {
            AppletIdentifyInfo appletIdentifyInfo = new()
            {
                AppletId = AppletId.QLaunch,
                TitleId = 0x0100000000001000,
            };

            context.ResponseData.WriteStruct(appletIdentifyInfo);

            return ResultCode.Success;
        }
    }
}

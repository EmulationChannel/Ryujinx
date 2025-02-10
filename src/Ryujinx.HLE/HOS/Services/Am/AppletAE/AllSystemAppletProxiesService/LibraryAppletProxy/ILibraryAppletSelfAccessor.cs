using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Memory;
using Ryujinx.HLE.HOS.Applets;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Loaders.Processes;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.LibraryAppletProxy
{
    class ILibraryAppletSelfAccessor : IpcService
    {
        private readonly AppletStandalone _appletStandalone = new();
 private RealApplet _realApplet;
        public ILibraryAppletSelfAccessor(ServiceCtx context)
        {
            if (context.Device.Processes.ActiveApplication.RealAppletInstance != null)
            {
                _realApplet = context.Device.Processes.ActiveApplication.RealAppletInstance;
                switch (context.Device.Processes.ActiveApplication.ProgramId)
                {
                    case 0x0100000000001006:
                    case 0x010000000000100F:
                        CommonArguments commonArguments = new()
                        {
                            Version = 1,
                            StructureSize = (uint)Marshal.SizeOf(typeof(CommonArguments)),
                            AppletVersion = 0x1,
                        };
                        MemoryStream stream = MemoryStreamManager.Shared.GetStream();
                        BinaryWriter writer = new(stream);
                        writer.WriteStruct(commonArguments);
                        _realApplet.NormalSession.Push(stream.ToArray());
                        break;
                }
            }
            else if (context.Device.Processes.ActiveApplication.ProgramId == 0x0100000000001009)
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
                // Create PhotoViewer data.
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
            byte[] appletData;

            if (_realApplet != null)
            {
                if (!_realApplet.NormalSession.TryPop(out appletData))
                {
                    return ResultCode.NotAvailable;
                }
            }
            else
            {
                appletData = _appletStandalone.InputData.Dequeue();
            }

            if (appletData.Length == 0)
            {
                return ResultCode.NotAvailable;
            }

            MakeObject(context, new IStorage(appletData));

            return ResultCode.Success;
        }
        
        [CommandCmif(1)]
        public ResultCode PushOutData(ServiceCtx context)
        {
            if (_realApplet != null)
            {
                IStorage data = GetObject<IStorage>(context, 0);
                _realApplet.NormalSession.Push(data.Data);
                _realApplet.InvokeAppletStateChanged();
            }

            return ResultCode.Success;
        }

        [CommandCmif(3)]
        public ResultCode PushInteractiveData(ServiceCtx context)
        {
            if (_realApplet != null)
            {
                IStorage data = GetObject<IStorage>(context, 0);
                _realApplet.InteractiveSession.Push(data.Data);
                _realApplet.InvokeAppletStateChanged();
            }

            return ResultCode.Success;
        }
        
        [CommandCmif(6)]
        public ResultCode GetPopInDataEvent(ServiceCtx context)
        {
           context.Process.HandleTable.GenerateHandle(_realApplet.PopInteractiveEvent.ReadableEvent, out int handle);
           context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

           return ResultCode.Success;
        }
        
        [CommandCmif(10)]
        public ResultCode ExitProcessAndReturn(ServiceCtx context)
        {
            ProcessResult result = context.Device.Processes.ActiveApplication;
            if (result.RealAppletInstance != null)
            {
                result.RealAppletInstance.Terminate(context, this);
            }
            else
            {
                context.Process.Terminate();
            }
            
            return ResultCode.Success;
        }
        
        [CommandCmif(11)]
        // GetLibraryAppletInfo() -> nn::am::service::LibraryAppletInfo
        public ResultCode GetLibraryAppletInfo(ServiceCtx context)
        {
            LibraryAppletInfo libraryAppletInfo = new();
            
            if (_realApplet != null)
            {
                libraryAppletInfo.AppletId = _realApplet.AppletId;
                libraryAppletInfo.LibraryAppletMode = LibraryAppletMode.PartialForeground;
            }
            else
            {
                libraryAppletInfo.AppletId = _appletStandalone.AppletId;
                libraryAppletInfo.LibraryAppletMode = _appletStandalone.LibraryAppletMode;
            }

            context.ResponseData.WriteStruct(libraryAppletInfo);

            return ResultCode.Success;
        }
        
        [CommandCmif(12)]
        // GetMainAppletIdentityInfo() -> nn::am::service::AppletIdentityInfo
        public ResultCode GetMainAppletIdentityInfo(ServiceCtx context)
        {
            AppletIdentifyInfo appletIdentifyInfo = new()
            {
                AppletId = AppletId.QLaunch,
                TitleId = 0x0100000000001000,
            };

            context.ResponseData.WriteStruct(appletIdentifyInfo);

            return ResultCode.Success;
        }

        [CommandCmif(13)]
        // CanUseApplicationCore() -> bool
        public ResultCode CanUseApplicationCore(ServiceCtx context)
        {
            context.ResponseData.Write(false);

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

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
        
        [CommandCmif(19)]
        // GetDesirableKeyboardLayout() -> nn::settings::KeyboardLayout
        public ResultCode GetDesirableKeyboardLayout(ServiceCtx context)
        {
            context.ResponseData.Write((ulong)KeyboardLayout.Default);

            return ResultCode.Success;
        }
        
        [CommandCmif(30)]
        // UnpopInData(nn::am::service::IStorage)
        public ResultCode UnpopInData(ServiceCtx context)
        {
            IStorage data = GetObject<IStorage>(context, 0);

            if (_realApplet != null)
            {
                _realApplet.NormalSession.Push(data.Data);
            }
            else
            {
                _appletStandalone.InputData.Enqueue(data.Data);
            }

            return ResultCode.Success;
        }

        [CommandCmif(50)]
        // ReportVisibleError(nn::err::ErrorCode)
        public ResultCode ReportVisibleError(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);
            return ResultCode.Success;
        }
    }
}

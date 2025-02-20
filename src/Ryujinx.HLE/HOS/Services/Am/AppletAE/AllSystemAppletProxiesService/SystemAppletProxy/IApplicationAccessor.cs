using LibHac.Ns;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Utilities;
using Ryujinx.HLE.HOS.Applets;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.HLE.HOS.Services.Ns.Types;
using Ryujinx.HLE.Loaders.Processes;
using Ryujinx.Horizon.Common;
using System;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IApplicationAccessor : IpcService
    {
        private readonly KernelContext _kernelContext;
        private readonly ulong _callerPid;
        private readonly ulong _applicationId;
        private readonly string _contentPath;

        private readonly KEvent _stateChangedEvent;
        private int _stateChangedEventHandle;
        public RealApplet applet;

        public IApplicationAccessor(ulong pid, ulong applicationId, string contentPath, Horizon system)
        {
            _callerPid = pid;
            _kernelContext = system.KernelContext;
            _applicationId = applicationId;
            _contentPath = contentPath;

            _stateChangedEvent = new KEvent(system.KernelContext);
        }


        [CommandCmif(0)]
        // GetAppletStateChangedEvent() -> handle<copy>
        public ResultCode GetAppletStateChangedEvent(ServiceCtx context)
        {
            if (_stateChangedEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_stateChangedEvent.ReadableEvent, out _stateChangedEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_stateChangedEventHandle);

            return ResultCode.Success;
        }


        [CommandCmif(10)]
        // Start()
        public ResultCode Start(ServiceCtx context)
        {
            _stateChangedEvent.ReadableEvent.Signal();

            Logger.Info?.Print(LogClass.ServiceAm, $"Application 0x{_applicationId:X}:{_contentPath} start requested.");
            ProcessResult processResult = null;
            bool isApplet = false;
            if (_contentPath.EndsWith("nsp"))
            {
                context.Device.Processes.LoadNsp(_contentPath,_applicationId, out processResult);
            }
            else if (_contentPath.EndsWith("xci"))
            {
                context.Device.Processes.LoadXci(_contentPath,_applicationId, out processResult);
            }
            else
            {
                context.Device.Processes.LoadNca(_contentPath, out processResult);
                isApplet = true;
            }

            ulong caller = 0;
            if (context.Device.System.WindowSystem.GetFirstApplet() != null)
            {
                caller = context.Device.System.WindowSystem.GetFirstApplet().ProcessHandle.Pid;
            }
            applet = context.Device.System.WindowSystem.TrackProcess(processResult.ProcessId, caller, !isApplet);
            applet.AppletState.SetFocusHandlingMode(true);
            return ResultCode.Success;
        }

        [CommandCmif(20)]
        // RequestExit()
        public ResultCode RequestExit(ServiceCtx context)
        {
            applet?.ProcessHandle?.SetActivity(false);
            applet?.AppletState?.OnExitRequested();
            applet?.ProcessHandle?.Terminate();
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }
        
        
        [CommandCmif(101)]
        // RequestForApplicationToGetForeground()
        public ResultCode RequestForApplicationToGetForeground(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);
            applet.AppletState.SetFocusForce(true);
            if (applet.ProcessHandle.IsPaused)
            {
                applet.ProcessHandle.SetActivity(false);
            }
            context.Device.System.WindowSystem.RequestApplicationToGetForeground();
            
            return ResultCode.Success;
        }
        
        [CommandCmif(121)]
        // PushLaunchParameter(u32) -> IStorage
        public ResultCode PushLaunchParameter(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);
            MakeObject(context, new IStorage(new byte[0],false));
            return ResultCode.Success;
        }
        
        [CommandCmif(122)]
        // GetApplicationControlProperty() -> NACP
        public ResultCode GetApplicationControlProperty(ServiceCtx context)
        {
            ulong titleId = context.Device.System.WindowSystem.GetApplicationApplet().ProcessHandle.TitleId;
            ApplicationControlProperty nacp = context.Device.Processes.ActiveApplication.ApplicationControlProperties;
            ulong position = context.Request.ReceiveBuff[0].Position;
            foreach (RyuApplicationData ryuApplicationData in context.Device.Configuration.Titles)
            {
                if (ryuApplicationData.AppId.Value != titleId)
                {
                    continue;
                }

                nacp = ryuApplicationData.Nacp;
                nacp.Title[1] = ryuApplicationData.Nacp.Title[0];
                break;
            }
            context.Memory.Write(position, SpanHelpers.AsByteSpan(ref nacp).ToArray());
            return ResultCode.Success;
        }
        
        [CommandCmif(130)]
        // SetUsers()
        public ResultCode SetUsers(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);
            bool enable = context.RequestData.ReadBoolean();
            return ResultCode.Success;
        }
        
        [CommandCmif(131)]
        // CheckRightsEnvironmentAvailable() -> bool
        public ResultCode CheckRightsEnvironmentAvailable(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);
            context.ResponseData.Write(true);
            return ResultCode.Success;
        }
        
        [CommandCmif(132)]
        // GetNsRightsEnvironmentHandle() -> u32
        public ResultCode GetNsRightsEnvironmentHandle(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);
            context.ResponseData.Write(0xdeadbeef);
            return ResultCode.Success;
        }
    }
}

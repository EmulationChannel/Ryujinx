using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.Fs.Shim;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem;
using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy;
using Ryujinx.HLE.HOS.Services.Apm;
using Ryujinx.HLE.HOS.Services.Caps;
using Ryujinx.HLE.HOS.Services.Mii;
using Ryujinx.HLE.HOS.Services.Nfc.AmiiboDecryption;
using Ryujinx.HLE.HOS.Services.Nfc.Nfp;
using Ryujinx.HLE.HOS.Services.Nfc.Nfp.NfpManager;
using Ryujinx.HLE.HOS.Services.Nv;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrl;
using Ryujinx.HLE.HOS.Services.Pcv.Bpc;
using Ryujinx.HLE.HOS.Services.Sdb.Pl;
using Ryujinx.HLE.HOS.Services.Settings;
using Ryujinx.HLE.HOS.Services.Sm;
using Ryujinx.HLE.HOS.Services.SurfaceFlinger;
using Ryujinx.HLE.HOS.Services.Time.Clock;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.HLE.Loaders.Processes;
using Ryujinx.Horizon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Ryujinx.HLE.HOS
{
    using TimeServiceManager = Services.Time.TimeManager;

    public class Horizon : IDisposable
    {
        internal const int HidSize = 0x40000;
        internal const int FontSize = 0x1100000;
        internal const int IirsSize = 0x8000;
        internal const int TimeSize = 0x1000;
        internal const int AppletCaptureBufferSize = 0x384000;

        internal KernelContext KernelContext { get; }

        internal Switch Device { get; private set; }

        internal ITickSource TickSource { get; }

        internal SurfaceFlinger SurfaceFlinger { get; private set; }

        public SystemStateMgr State { get; private set; }

        internal PerformanceState PerformanceState { get; private set; }

        internal AppletStateMgr IntialAppletState { get; private set; }

        internal AppletStateMgr AppletState
        {
            get
            {
                if (Device.Processes?.ActiveApplication?.RealAppletInstance != null)
                {
                    Logger.Info?.Print(LogClass.Application, "Real applet instance found");
                    return Device.Processes.ActiveApplication.RealAppletInstance.AppletState;
                }

                return IntialAppletState;
            }
            set
            {
                if (value != null)
                {
                    IntialAppletState = value;
                }
            }
        }

        internal List<NfpDevice> NfpDevices { get; private set; }

        internal ServerBaseManager LibHacServerManagerMain = new ServerBaseManager();

       private T GetServerProperty<T>(Func<ServerBaseManager, T> selector)
        {
            return IsApplet() 
                ? selector(Device.Processes.ActiveApplication.RealAppletInstance.LibHacServerManager)
                : selector(LibHacServerManagerMain);
        }

        private void SetServerProperty<T>(Action<ServerBaseManager, T> setter, T value)
        {
            if (IsApplet())
            {
                setter(Device.Processes.ActiveApplication.RealAppletInstance.LibHacServerManager, value);
            }
            else
            {
                setter(LibHacServerManagerMain, value);
            }
        }

        internal SmRegistry SmRegistry
        {
            get => GetServerProperty(server => server.SmRegistry);
            set => SetServerProperty((server, v) => server.SmRegistry = v, value);
        }

        internal ServerBase SmServer
        {
            get => GetServerProperty(server => server.SmServer);
            set => SetServerProperty((server, v) => server.SmServer = v, value);
        }

        internal ServerBase BsdServer
        {
            get => GetServerProperty(server => server.BsdServer);
            set => SetServerProperty((server, v) => server.BsdServer = v, value);
        }

        internal ServerBase FsServer
        {
            get => GetServerProperty(server => server.FsServer);
            set => SetServerProperty((server, v) => server.FsServer = v, value);
        }

        internal ServerBase HidServer
        {
            get => GetServerProperty(server => server.HidServer);
            set => SetServerProperty((server, v) => server.HidServer = v, value);
        }

        internal ServerBase NvDrvServer
        {
            get => GetServerProperty(server => server.NvDrvServer);
            set => SetServerProperty((server, v) => server.NvDrvServer = v, value);
        }

        internal ServerBase TimeServer
        {
            get => GetServerProperty(server => server.TimeServer);
            set => SetServerProperty((server, v) => server.TimeServer = v, value);
        }

        internal ServerBase ViServer
        {
            get => GetServerProperty(server => server.ViServer);
            set => SetServerProperty((server, v) => server.ViServer = v, value);
        }

        internal ServerBase ViServerM
        {
            get => GetServerProperty(server => server.ViServerM);
            set => SetServerProperty((server, v) => server.ViServerM = v, value);
        }

        internal ServerBase ViServerS
        {
            get => GetServerProperty(server => server.ViServerS);
            set => SetServerProperty((server, v) => server.ViServerS = v, value);
        }

        internal ServerBase LdnServer
        {
            get => GetServerProperty(server => server.LdnServer);
            set => SetServerProperty((server, v) => server.LdnServer = v, value);
        }

        internal KSharedMemory HidSharedMem { get; private set; }
        internal KSharedMemory FontSharedMem { get; private set; }
        internal KSharedMemory IirsSharedMem { get; private set; }

        internal KTransferMemory AppletCaptureBufferTransfer { get; private set; }

        internal SharedFontManager SharedFontManager { get; private set; }
        internal AccountManager AccountManager { get; private set; }
        internal ContentManager ContentManager { get; private set; }
        internal CaptureManager CaptureManager { get; private set; }

        internal KEvent VsyncEvent { get; private set; }

        internal KEvent DisplayResolutionChangeEvent { get; private set; }
        
        internal KEvent GeneralChannelEvent { get; private set; }
        internal Queue<byte[]> GeneralChannelData { get; private set; } = new();

        public KeySet KeySet => Device.FileSystem.KeySet;

        private bool _isDisposed;

        public bool EnablePtc { get; set; }

        public IntegrityCheckLevel FsIntegrityCheckLevel { get; set; }

        public int GlobalAccessLogMode { get; set; }

        internal SharedMemoryStorage HidStorage { get; private set; }

        internal NvHostSyncpt HostSyncpoint { get; private set; }

        internal LibHacHorizonManager LibHacHorizonManager { get; private set; }

        internal ServiceTable ServiceTableMain { get; private set; }
        internal ServiceTable ServiceTable
        {
            get
            {
                if (IsApplet())
                {
                    return Device.Processes.ActiveApplication.RealAppletInstance.LibHacServerManager.ServiceTable;
                }
                else
                {
                    return ServiceTableMain;
                }
            }
            set
            {
                if (IsApplet())
                {
                    Device.Processes.ActiveApplication.RealAppletInstance.LibHacServerManager.ServiceTable = value;
                }
                else
                {
                    ServiceTableMain = value;
                }
            }
        }

        public bool IsApplet()
        {
            if (Device?.Processes?.ActiveApplication?.RealAppletInstance != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public bool IsPaused { get; private set; }
        
        public Horizon(Switch device)
        {
            TickSource = new TickSource(KernelConstants.CounterFrequency);

            KernelContext = new KernelContext(
                TickSource,
                device,
                device.Memory,
                device.Configuration.MemoryConfiguration.ToKernelMemorySize(),
                device.Configuration.MemoryConfiguration.ToKernelMemoryArrange());

            Device = device;

            State = new SystemStateMgr();

            PerformanceState = new PerformanceState();

            NfpDevices = [];

            // Note: This is not really correct, but with HLE of services, the only memory
            // region used that is used is Application, so we can use the other ones for anything.
            KMemoryRegionManager region = KernelContext.MemoryManager.MemoryRegions[(int)MemoryRegion.NvServices];

            ulong hidPa = region.Address;
            ulong fontPa = region.Address + HidSize;
            ulong iirsPa = region.Address + HidSize + FontSize;
            ulong timePa = region.Address + HidSize + FontSize + IirsSize;
            ulong appletCaptureBufferPa = region.Address + HidSize + FontSize + IirsSize + TimeSize;

            KPageList hidPageList = new();
            KPageList fontPageList = new();
            KPageList iirsPageList = new();
            KPageList timePageList = new();
            KPageList appletCaptureBufferPageList = new();

            hidPageList.AddRange(hidPa, HidSize / KPageTableBase.PageSize);
            fontPageList.AddRange(fontPa, FontSize / KPageTableBase.PageSize);
            iirsPageList.AddRange(iirsPa, IirsSize / KPageTableBase.PageSize);
            timePageList.AddRange(timePa, TimeSize / KPageTableBase.PageSize);
            appletCaptureBufferPageList.AddRange(appletCaptureBufferPa, AppletCaptureBufferSize / KPageTableBase.PageSize);

            SharedMemoryStorage hidStorage = new(KernelContext, hidPageList);
            SharedMemoryStorage fontStorage = new(KernelContext, fontPageList);
            SharedMemoryStorage iirsStorage = new(KernelContext, iirsPageList);
            SharedMemoryStorage timeStorage = new(KernelContext, timePageList);
            SharedMemoryStorage appletCaptureBufferStorage = new(KernelContext, appletCaptureBufferPageList);

            HidStorage = hidStorage;

            HidSharedMem = new KSharedMemory(KernelContext, hidStorage, 0, 0, KMemoryPermission.Read);
            FontSharedMem = new KSharedMemory(KernelContext, fontStorage, 0, 0, KMemoryPermission.Read);
            IirsSharedMem = new KSharedMemory(KernelContext, iirsStorage, 0, 0, KMemoryPermission.Read);

            KSharedMemory timeSharedMemory = new(KernelContext, timeStorage, 0, 0, KMemoryPermission.Read);

            TimeServiceManager.Instance.Initialize(device, this, timeSharedMemory, timeStorage, TimeSize);

            AppletCaptureBufferTransfer = new KTransferMemory(KernelContext, appletCaptureBufferStorage);

            AppletState = new AppletStateMgr(this);

            AppletState.SetFocus(true);

            VsyncEvent = new KEvent(KernelContext);

            DisplayResolutionChangeEvent = new KEvent(KernelContext);
            GeneralChannelEvent = new KEvent(KernelContext);

            SharedFontManager = new SharedFontManager(device, fontStorage);
            AccountManager = device.Configuration.AccountManager;
            ContentManager = device.Configuration.ContentManager;
            CaptureManager = new CaptureManager(device);

            LibHacHorizonManager = device.Configuration.LibHacHorizonManager;

            // We hardcode a clock source id to avoid it changing between each start.
            // TODO: use set:sys (and get external clock source id from settings)
            // TODO: use "time!standard_steady_clock_rtc_update_interval_minutes" and implement a worker thread to be accurate.
            UInt128 clockSourceId = new(0x36a0328702ce8bc1, 0x1608eaba02333284);
            IRtcManager.GetExternalRtcValue(out ulong rtcValue);

            // We assume the rtc is system time.
            TimeSpanType systemTime = TimeSpanType.FromSeconds((long)rtcValue);

            // Configure and setup internal offset
            TimeSpanType internalOffset = TimeSpanType.FromSeconds(device.Configuration.SystemTimeOffset);

            TimeSpanType systemTimeOffset = new(systemTime.NanoSeconds + internalOffset.NanoSeconds);

            if (systemTime.IsDaylightSavingTime() && !systemTimeOffset.IsDaylightSavingTime())
            {
                internalOffset = internalOffset.AddSeconds(3600L);
            }
            else if (!systemTime.IsDaylightSavingTime() && systemTimeOffset.IsDaylightSavingTime())
            {
                internalOffset = internalOffset.AddSeconds(-3600L);
            }

            systemTime = new TimeSpanType(systemTime.NanoSeconds + internalOffset.NanoSeconds);

            // First init the standard steady clock
            TimeServiceManager.Instance.SetupStandardSteadyClock(TickSource, clockSourceId, TimeSpanType.Zero, TimeSpanType.Zero, TimeSpanType.Zero, false);
            TimeServiceManager.Instance.SetupStandardLocalSystemClock(TickSource, new SystemClockContext(), systemTime.ToSeconds());
            TimeServiceManager.Instance.StandardLocalSystemClock.GetClockContext(TickSource, out SystemClockContext localSytemClockContext);

            if (NxSettings.Settings.TryGetValue("time!standard_network_clock_sufficient_accuracy_minutes", out object standardNetworkClockSufficientAccuracyMinutes))
            {
                TimeSpanType standardNetworkClockSufficientAccuracy = new((int)standardNetworkClockSufficientAccuracyMinutes * 60000000000);

                // The network system clock needs a valid system clock, as such we setup this system clock using the local system clock.
                TimeServiceManager.Instance.SetupStandardNetworkSystemClock(localSytemClockContext, standardNetworkClockSufficientAccuracy);
            }

            TimeServiceManager.Instance.SetupStandardUserSystemClock(TickSource, true, localSytemClockContext.SteadyTimePoint);

            // FIXME: TimeZone should be init here but it's actually done in ContentManager

            TimeServiceManager.Instance.SetupEphemeralNetworkSystemClock();

            DatabaseImpl.Instance.InitializeDatabase(TickSource, LibHacHorizonManager.SdbClient);

            HostSyncpoint = new NvHostSyncpt(device);

            SurfaceFlinger = new SurfaceFlinger(device);
        }

        private void StopAndDisposeService(ServerBase server)
        {
            if (server != null)
            {
                try
                {
                    server.Stop();
                    Logger.Info?.Print(LogClass.Application,$"{server.Name} successfully stopped.");
                }
                catch (Exception ex)
                {
                    Logger.Info?.Print(LogClass.Application,$"Error while stopping {server.Name}: {ex.Message}");
                }
            }
        }

        public void DeinitializeServices()
        {
            // Stop and clean up the services in reverse order of initialization for safety.
            StopAndDisposeService(LdnServer);
            StopAndDisposeService(ViServerS);
            StopAndDisposeService(ViServerM);
            StopAndDisposeService(ViServer);
            StopAndDisposeService(TimeServer);
            StopAndDisposeService(NvDrvServer);
            StopAndDisposeService(HidServer);
            StopAndDisposeService(FsServer);
            StopAndDisposeService(BsdServer);
    
            if (SmServer != null)
            {
                SmServer.Stop();
                SmServer = null;
            }

            SmRegistry = null;

            ServiceTable?.Dispose();
            ServiceTable = null;
        }
        
        public void InitializeServices()
        {
            SmRegistry = new SmRegistry();
            SmServer = new ServerBase(KernelContext, "SmServer", () => new IUserInterface(KernelContext, SmRegistry));

            // Wait until SM server thread is done with initialization,
            // only then doing connections to SM is safe.
            SmServer.InitDone.WaitOne();

            BsdServer = new ServerBase(KernelContext, "BsdServer");
            FsServer = new ServerBase(KernelContext, "FsServer");
            HidServer = new ServerBase(KernelContext, "HidServer");
            NvDrvServer = new ServerBase(KernelContext, "NvservicesServer");
            TimeServer = new ServerBase(KernelContext, "TimeServer");
            ViServer = new ServerBase(KernelContext, "ViServerU");
            ViServerM = new ServerBase(KernelContext, "ViServerM");
            ViServerS = new ServerBase(KernelContext, "ViServerS");
            LdnServer = new ServerBase(KernelContext, "LdnServer");

            StartNewServices();
        }

        private void StartNewServices()
        {
            HorizonFsClient fsClient = new(this);

            ServiceTable = new ServiceTable();
            IEnumerable<ServiceEntry> services = ServiceTable.GetServices(new HorizonOptions
                (Device.Configuration.IgnoreMissingServices,
                LibHacHorizonManager.BcatClient,
                fsClient,
                AccountManager,
                Device.AudioDeviceDriver,
                TickSource));

            foreach (ServiceEntry service in services)
            {
                const ProcessCreationFlags Flags =
                    ProcessCreationFlags.EnableAslr |
                    ProcessCreationFlags.AddressSpace64Bit |
                    ProcessCreationFlags.Is64Bit |
                    ProcessCreationFlags.PoolPartitionSystem;

                ProcessCreationInfo creationInfo = new("Service", 1, 0, 0x8000000, 1, Flags, 0, 0);

                uint[] defaultCapabilities =
                [
                    (((uint)KScheduler.CpuCoresCount - 1) << 24) + (((uint)KScheduler.CpuCoresCount - 1) << 16) + 0x63F7u,
                    0x1FFFFFCF,
                    0x207FFFEF,
                    0x47E0060F,
                    0x0048BFFF,
                    0x01007FFF
                ];

                // TODO:
                // - Pass enough information (capabilities, process creation info, etc) on ServiceEntry for proper initialization.
                // - Have the ThreadStart function take the syscall, address space and thread context parameters instead of passing them here.
                KernelStatic.StartInitialProcess(KernelContext, creationInfo, defaultCapabilities, 44, () =>
                {
                    service.Start(KernelContext.Syscall, KernelStatic.GetCurrentProcess().CpuMemory, KernelStatic.GetCurrentThread().ThreadContext);
                });
            }
        }

        public bool LoadKip(string kipPath)
        {
            using SharedRef<IStorage> kipFile = new(new LocalStorage(kipPath, FileAccess.Read));

            return ProcessLoaderHelper.LoadKip(KernelContext, new KipExecutable(in kipFile));
        }

        public void ChangeDockedModeState(bool newState)
        {
            if (newState != State.DockedMode)
            {
                State.DockedMode = newState;
                PerformanceState.PerformanceMode = State.DockedMode ? PerformanceMode.Boost : PerformanceMode.Default;

                AppletState.Messages.Enqueue(AppletMessage.OperationModeChanged);
                AppletState.Messages.Enqueue(AppletMessage.PerformanceModeChanged);
                AppletState.MessageEvent.ReadableEvent.Signal();

                SignalDisplayResolutionChange();

                Device.Configuration.RefreshInputConfig?.Invoke();
            }
        }

        public void ReturnFocus()
        {
            AppletState.SetFocus(true);
        }

        public void SimulateWakeUpMessage()
        {
            AppletState.Messages.Enqueue(AppletMessage.Resume);
            AppletState.MessageEvent.ReadableEvent.Signal();

            // 0x534D4153 0x00000001 0x00000002 0x00000001
            PushToGeneralChannel(new byte[] {
                0x53, 0x41, 0x4D, 0x53, 0x01, 0x00, 0x00, 0x00,
                0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            });
        }

        public void PushToGeneralChannel(byte[] data)
        {
            if (data.Length > 0)
            {
                GeneralChannelData.Enqueue(data);
                GeneralChannelEvent.ReadableEvent.Signal();
            }
        }
        
        public void ScanAmiibo(int nfpDeviceId, string amiiboId, bool useRandomUuid)
        {
            if (VirtualAmiibo.ApplicationBytes.Length > 0)
            {
                VirtualAmiibo.ApplicationBytes = [];
                VirtualAmiibo.InputBin = string.Empty;
            }
            if (NfpDevices[nfpDeviceId].State == NfpDeviceState.SearchingForTag)
            {
                NfpDevices[nfpDeviceId].State = NfpDeviceState.TagFound;
                NfpDevices[nfpDeviceId].AmiiboId = amiiboId;
                NfpDevices[nfpDeviceId].UseRandomUuid = useRandomUuid;
            }
        }
        public void ScanAmiiboFromBin(string path)
        {
            VirtualAmiibo.InputBin = path;
            if (VirtualAmiibo.ApplicationBytes.Length > 0)
            {
                VirtualAmiibo.ApplicationBytes = [];
            }
            byte[] encryptedData = File.ReadAllBytes(path);
            VirtualAmiiboFile newFile = AmiiboBinReader.ReadBinFile(encryptedData);
            if (SearchingForAmiibo(out int nfpDeviceId))
            {
                NfpDevices[nfpDeviceId].State = NfpDeviceState.TagFound;
                NfpDevices[nfpDeviceId].AmiiboId = newFile.AmiiboId;
                NfpDevices[nfpDeviceId].UseRandomUuid = false;
            }
        }

        public bool SearchingForAmiibo(out int nfpDeviceId)
        {
            nfpDeviceId = default;

            for (int i = 0; i < NfpDevices.Count; i++)
            {
                if (NfpDevices[i].State == NfpDeviceState.SearchingForTag)
                {
                    nfpDeviceId = i;

                    return true;
                }
            }

            return false;
        }

        public void SignalDisplayResolutionChange()
        {
            DisplayResolutionChangeEvent.ReadableEvent.Signal();
        }

        public void SignalVsync()
        {
            VsyncEvent.ReadableEvent.Signal();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed && disposing)
            {
                _isDisposed = true;

                // "Soft" stops AudioRenderer and AudioManager to avoid some sound between resume and stop.
                if (IsPaused)
                {
                    TogglePauseEmulation(false);
                }

                KProcess terminationProcess = new(KernelContext);
                KThread terminationThread = new(KernelContext);

                terminationThread.Initialize(0, 0, 0, 3, 0, terminationProcess, ThreadType.Kernel, () =>
                {
                    // Force all threads to exit.
                    lock (KernelContext.Processes)
                    {
                        // Terminate application.
                        foreach (KProcess process in KernelContext.Processes.Values.Where(x => x.IsApplication))
                        {
                            process.Terminate();
                            process.DecrementReferenceCount();
                        }

                        // The application existed, now surface flinger can exit too.
                        SurfaceFlinger.Dispose();

                        // Terminate HLE services (must be done after the application is already terminated,
                        // otherwise the application will receive errors due to service termination).
                        foreach (KProcess process in KernelContext.Processes.Values.Where(x => !x.IsApplication))
                        {
                            process.Terminate();
                            process.DecrementReferenceCount();
                        }

                        KernelContext.Processes.Clear();
                    }

                    // Exit ourself now!
                    KernelStatic.GetCurrentThread().Exit();
                });

                terminationThread.Start();

                // Wait until the thread is actually started.
                while (terminationThread.HostThread.ThreadState == ThreadState.Unstarted)
                {
                    Thread.Sleep(10);
                }

                // Wait until the termination thread is done terminating all the other threads.
                terminationThread.HostThread.Join();

                // Destroy nvservices channels as KThread could be waiting on some user events.
                // This is safe as KThread that are likely to call ioctls are going to be terminated by the post handler hook on the SVC facade.
                INvDrvServices.Destroy();
                
                foreach (var client in LibHacHorizonManager.ApplicationClients)
                {
                    LibHacHorizonManager.PmClient.Fs.UnregisterProgram(client.Value.Os.GetCurrentProcessId().Value).ThrowIfFailure();
                }
                LibHacHorizonManager.ApplicationClients.Clear();
                
                KernelContext.Dispose();
            }
        }

        public void TogglePauseEmulation(bool pause)
        {
            lock (KernelContext.Processes)
            {
                foreach (KProcess process in KernelContext.Processes.Values)
                {
                    if (process.IsApplication)
                    {
                        // Only game process should be paused.
                        process.SetActivity(pause);
                    }
                }

                if (pause && !IsPaused)
                {
                    Device.AudioDeviceDriver.GetPauseEvent().Reset();
                    TickSource.Suspend();
                }
                else if (!pause && IsPaused)
                {
                    Device.AudioDeviceDriver.GetPauseEvent().Set();
                    TickSource.Resume();
                }
            }
            IsPaused = pause;
        }

        public void CreateNewAppletManager()
        {
            AppletState = new AppletStateMgr(this);
            AppletState.SetFocus(true);
        }
        
        internal void SetFromAppletStateMgr(AppletStateMgr state)
        {
            AppletState = state;
        }
    }
}

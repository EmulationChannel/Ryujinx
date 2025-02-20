using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Services.Am.AppletAE;
using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy;
using System.Collections.Generic;
using System.Linq;
using Ryujinx.Horizon.Sdk.Applet;
using Ryujinx.Common;
using Ryujinx.HLE.HOS.Applets.Types;
using System.Collections;
using System.Threading;

namespace Ryujinx.HLE.HOS.Applets
{
    public class WindowSystem
    {
        private Horizon _system;
        private readonly object _lock = new();
        private EventObserver _eventObserver = null;

        // Foreground roots.
        RealApplet _homeMenu = null;
        RealApplet _overlayDisp = null;
        RealApplet _application = null;
        // Removed single application field to allow multiple applications.

        // Home menu state.
        private bool _homeMenuForegroundLocked = false;
        private RealApplet _foregroundRequestedApplet = null;

        // aruid -> applet map.
        private Dictionary<ulong, RealApplet> _applets = new();
        private List<RealApplet> _rootApplets = new();

        internal ButtonPressTracker ButtonPressTracker { get; }

        public WindowSystem(Horizon system)
        {
            _system = system;
            ButtonPressTracker = new ButtonPressTracker(system);
        }

        void Dispose()
        {
            // SetWindowSystem(null);
        }

        internal void SetEventObserver(EventObserver eventObserver)
        {
            _eventObserver = eventObserver;
            // SetWindowSystem(this);
        }

        internal void Update()
        {
            lock (_lock)
            {
                PruneTerminatedAppletsLocked();

                if (LockHomeMenuIntoForegroundLocked())
                {
                    return;
                }

                // If no foreground applet is explicitly requested, choose the last root applet.
                if (_foregroundRequestedApplet == null && _rootApplets.Count != 0)
                {
                    _foregroundRequestedApplet = _rootApplets.Last();
                }

                foreach (var applet in _rootApplets)
                {
                    UpdateAppletStateLocked(applet, _foregroundRequestedApplet == applet);
                }
            }
        }

        /// <summary>
        /// Tracks a new process as an applet.
        /// </summary>
        internal RealApplet TrackProcess(ulong pid, ulong callerPid, bool isApplication)
        {
            lock (_lock)
            {
                if (_applets.TryGetValue(pid, out var applet))
                {
                    Logger.Info?.Print(LogClass.ServiceAm, $"TrackProcess() called on existing applet {pid} - caller {callerPid}");
                    return applet;
                }

                Logger.Info?.Print(LogClass.ServiceAm, $"Tracking process {pid} as {(isApplication ? "application" : "applet")} - caller {callerPid}");
                if (_system.KernelContext.Processes.TryGetValue(pid, out var _process))
                {
                    applet = new RealApplet(pid, isApplication, _system);

                    if (callerPid == 0)
                    {
                        _rootApplets.Add(applet);
                    }
                    else
                    {
                        var callerApplet = _applets[callerPid];
                        applet.CallerApplet = callerApplet;
                        callerApplet.RegisterChild(applet);
                    }

                    TrackApplet(applet, isApplication);
                    return applet;
                }

                return null;
            }
        }

        /// <summary>
        /// Registers the applet in the global tracking data structures.
        /// </summary>
        private void TrackApplet(RealApplet applet, bool isApplication)
        {
            if (_applets.ContainsKey(applet.AppletResourceUserId))
            {
                return;
            }

            if (applet.AppletId == RealAppletId.SystemAppletMenu)
            {
                _homeMenu = applet;
                _foregroundRequestedApplet = applet;
            }
            else if (applet.AppletId == RealAppletId.OverlayApplet)
            {
                _overlayDisp = applet;
            }
            else if (isApplication)
            {
                _application = applet;
            }

            _applets[applet.AppletResourceUserId] = applet;
            _eventObserver.TrackAppletProcess(applet);

            if (_applets.Count == 1 || applet.AppletId == RealAppletId.SystemAppletMenu || applet.AppletId == RealAppletId.OverlayApplet)
            {
                SetupFirstApplet(applet);
            }

            // _foregroundRequestedApplet = applet;
            // applet.AppletState.SetFocusState(FocusState.InFocus);

            _eventObserver.RequestUpdate();
        }

        /// <summary>
        /// Performs initial setup for the first tracked applet.
        /// </summary>
        private void SetupFirstApplet(RealApplet applet)
        {
            if (applet.AppletId == RealAppletId.SystemAppletMenu)
            {
                applet.AppletState.SetOutOfFocusSuspendingEnabled(false);
                RequestHomeMenuToGetForeground();
            }
            else if (applet.AppletId == RealAppletId.OverlayApplet)
            {
                applet.AppletState.SetOutOfFocusSuspendingEnabled(false);
                applet.AppletState.SetFocusState(FocusState.OutOfFocus);
            }
            else
            {
                applet.AppletState.SetFocusState(FocusState.InFocus);
                _foregroundRequestedApplet = applet;
                RequestApplicationToGetForeground();
            }

            applet.UpdateSuspensionStateLocked(true);
        }

        internal RealApplet GetByAruId(ulong aruid)
        {
            if (_applets.TryGetValue(aruid, out RealApplet applet))
            {
                return applet;
            }

            return null;
        }

        /// <summary>
        /// Returns the current foreground application.
        /// If none is explicitly set, the first tracked application is returned.
        /// </summary>
        internal RealApplet GetMainApplet()
        {
            lock (_lock)
            {
                if (_foregroundRequestedApplet != null && _foregroundRequestedApplet.IsApplication)
                {
                    return _foregroundRequestedApplet;
                }

                return _rootApplets.FirstOrDefault(applet => applet.IsApplication);
            }
        }

        internal void RequestHomeMenuToGetForeground()
        {
            _foregroundRequestedApplet = _homeMenu;
            _eventObserver.RequestUpdate();
        }

        internal void RequestApplicationToGetForeground()
        {
            // lock (_lock)
            {
                _foregroundRequestedApplet = _application;
            }

            _eventObserver.RequestUpdate();
        }

        internal void RequestLockHomeMenuIntoForeground()
        {
            // lock (_lock)
            {
                _homeMenuForegroundLocked = true;
            }

            _eventObserver.RequestUpdate();
        }

        internal void RequestUnlockHomeMenuFromForeground()
        {
            // lock (_lock)
            {
                _homeMenuForegroundLocked = false;
            }

            _eventObserver.RequestUpdate();
        }

        internal void RequestAppletVisibilityState(RealApplet applet, bool isVisible)
        {
            lock (applet.Lock)
            {
                applet.WindowVisible = isVisible;
            }

            _eventObserver.RequestUpdate();
        }
        
        internal void OnOperationModeChanged()
        {
            foreach (var (_, applet) in _applets)
            {
                lock (applet.Lock)
                {
                    applet.AppletState.OnOperationAndPerformanceModeChanged();
                }
            }
        }

        internal void OnExitRequested()
        {
            foreach (var (_, applet) in _applets)
            {
                lock (applet.Lock)
                {
                    applet.AppletState.OnExitRequested();
                }
            }
        }

        internal void OnSystemButtonPress(SystemButtonType type)
        {
            switch (type)
            {
                case SystemButtonType.PerformHomeButtonShortPressing:
                    SendButtonAppletMessageLocked(AppletMessage.DetectShortPressingHomeButton);
                    break;
                case SystemButtonType.PerformHomeButtonLongPressing:
                    SendButtonAppletMessageLocked(AppletMessage.DetectLongPressingHomeButton);
                    break;
                case SystemButtonType.PerformCaptureButtonShortPressing:
                    SendButtonAppletMessageLocked(AppletMessage.DetectShortPressingCaptureButton);
                    break;
            }
        }

        private void SendButtonAppletMessageLocked(AppletMessage message)
        {
            if (message == AppletMessage.DetectShortPressingHomeButton)
            {
                foreach (var applet in _applets.Values)
                {
                    if (applet != _homeMenu && applet != _overlayDisp && _foregroundRequestedApplet==applet)
                    {
                        applet.ProcessHandle.SetActivity(true);
                    }
                }
            }
            
            if (_homeMenu != null)
            {
                lock (_homeMenu.Lock)
                {
                    _homeMenu.AppletState.PushUnorderedMessage(message);
                }
            }

            if (_overlayDisp != null)
            {
                lock (_overlayDisp.Lock)
                {
                    _overlayDisp.AppletState.PushUnorderedMessage(message);
                }
            }
        }

        /// <summary>
        /// Removes terminated applets from tracking.
        /// </summary>
        private void PruneTerminatedAppletsLocked()
        {
            // We need to iterate over a copy of the dictionary keys because we might remove items.
            foreach (var (aruid, applet) in _applets.ToList())
            {
                lock (applet.Lock)
                {
                    if (applet.ProcessHandle.State != ProcessState.Exited)
                    {
                        continue;
                    }

                    // If the applet has child applets still, terminate them first.
                    if (applet.ChildApplets.Count != 0)
                    {
                        TerminateChildAppletsLocked(applet);
                        continue;
                    }

                    // If this applet was started by another, remove it from its caller’s child list.
                    if (applet.CallerApplet != null)
                    {
                        applet.CallerApplet.ChildApplets.Remove(applet);
                        applet.CallerApplet = null;
                    }

                    if (applet == _foregroundRequestedApplet)
                    {
                        _foregroundRequestedApplet = null;
                    }

                    if (applet == _homeMenu)
                    {
                        _homeMenu = null;
                        _foregroundRequestedApplet = null;
                    }

                    // For application applets, clear the foreground reference if necessary and
                    // notify the home menu that an application has exited.
                    if (applet.IsApplication)
                    {
                        if (_foregroundRequestedApplet == applet)
                        {
                            _foregroundRequestedApplet = null;
                        }

                        if (_homeMenu != null)
                        {
                            _homeMenu.AppletState.PushUnorderedMessage(AppletMessage.ApplicationExited);
                        }
                    }

                    applet.OnProcessTerminatedLocked();

                    _eventObserver.RequestUpdate();
                    _applets.Remove(aruid);
                    _rootApplets.Remove(applet);
                }
            }
        }

        /// <summary>
        /// Terminates any child applets of the specified parent.
        /// </summary>
        private void TerminateChildAppletsLocked(RealApplet parent)
        {
            foreach (var child in parent.ChildApplets)
            {
                if (child.ProcessHandle.State != ProcessState.Exited)
                {
                    child.ProcessHandle.Terminate();
                    child.TerminateResult = (ResultCode)Services.Am.ResultCode.LibraryAppletTerminated;
                }
            }
        }

        /// <summary>
        /// If the home menu is locked into the foreground, ensure it remains in front.
        /// </summary>
        private bool LockHomeMenuIntoForegroundLocked()
        {
            if (_homeMenu == null || !_homeMenuForegroundLocked)
            {
                _homeMenuForegroundLocked = false;
                return false;
            }

            lock (_homeMenu.Lock)
            {
                TerminateChildAppletsLocked(_homeMenu);

                if (_homeMenu.ChildApplets.Count == 0)
                {
                    _homeMenu.WindowVisible = true;
                    _foregroundRequestedApplet = _homeMenu;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Updates the state of the specified applet and its children.
        /// </summary>
        private void UpdateAppletStateLocked(RealApplet applet, bool isForeground)
        {
            if (applet == null)
            {
                return;
            }

            lock (applet.Lock)
            {
                var inheritedForeground = applet.IsProcessRunning && isForeground;
                var visibleState = inheritedForeground ? ActivityState.ForegroundVisible : ActivityState.BackgroundVisible;
                var obscuredState = inheritedForeground ? ActivityState.ForegroundObscured : ActivityState.BackgroundObscured;

                var hasObscuringChildApplets = applet.ChildApplets.Any(child =>
                {
                    lock (child.Lock)
                    {
                        var mode = child.LibraryAppletMode;
                        if (child.IsProcessRunning && child.WindowVisible &&
                            (mode == LibraryAppletMode.AllForeground || mode == LibraryAppletMode.AllForegroundInitiallyHidden))
                        {
                            return true;
                        }
                    }

                    return false;
                });

                // TODO: Update visibility state if needed.

                applet.SetInteractibleLocked(isForeground && applet.WindowVisible);

                var isObscured = hasObscuringChildApplets || !applet.WindowVisible;
                var state = applet.AppletState.ActivityState;

                if (isObscured && state != obscuredState)
                {
                    applet.AppletState.ActivityState = obscuredState;
                    applet.UpdateSuspensionStateLocked(true);
                }
                else if (!isObscured && state != visibleState)
                {
                    applet.AppletState.ActivityState = visibleState;
                    applet.UpdateSuspensionStateLocked(true);
                }

                Logger.Info?.Print(LogClass.ServiceAm,
                    $"Updating applet state for {applet.AppletId}: visible={applet.WindowVisible}, foreground={isForeground}, obscured={isObscured}, reqFState={applet.AppletState.RequestedFocusState}, ackFState={applet.AppletState.AcknowledgedFocusState}, runnable={applet.AppletState.IsRunnable()}");
                
                // Recurse into child applets.
                foreach (var child in applet.ChildApplets)
                {
                    if (child == _foregroundRequestedApplet)
                    {
                        UpdateAppletStateLocked(child, true);
                        _foregroundRequestedApplet.SetInteractibleLocked(true);
                    }
                    else
                    {
                        UpdateAppletStateLocked(child, isForeground);
                        _foregroundRequestedApplet.SetInteractibleLocked(isForeground);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the process identifier of the currently focused applet.
        /// </summary>
        public ulong GetFocusedApp()
        {
            if (_foregroundRequestedApplet == null)
            {
                return _homeMenu == null ? 0 : _homeMenu.ProcessHandle.Pid;
            }
            return _foregroundRequestedApplet.ProcessHandle.Pid;
        }

        internal RealApplet GetFirstApplet()
        {
            lock (_lock)
            {
                if (_applets.Count == 0)
                {
                    return null;
                }
                ulong oldestPID = _applets.Keys.Min();
                return _applets[oldestPID];
            }
        }

        internal bool IsFocusedApplet(RealApplet applet)
        {
            return _foregroundRequestedApplet == applet;
        }

        internal RealApplet GetApplicationApplet()
        {
            if (_application != null)
            {
                return _application;
            }
            else
            {
                if (_homeMenu != null)
                {
                    return _homeMenu;
                }
                else
                {
                    return _overlayDisp;
                }
            }
        }

        public void RemoveProcess(ulong processHandlePid)
        {
            lock (_lock)
            {
                if (_applets.TryGetValue(processHandlePid, out RealApplet applet))
                {
                    _applets.Remove(processHandlePid);
                    _rootApplets.Remove(applet);
                    _eventObserver.RequestUpdate();
                }
            }
        }

        public bool IsLaunchedAsReal(ulong pid)
        {
            RealApplet applet = null;
            lock (_lock)
            {
                if (_applets.TryGetValue(pid, out applet))
                {
                    if (_applets.Count > 2)
                    {
                        return true;
                    }
                    else
                    {
                        if (applet==_homeMenu||applet==_overlayDisp)
                        {
                            return false;
                        }
                    }
                }
            }

            return false;
        }

        public bool HasAnyApplets()
        {
            return _applets.Count > 0;
        }

        internal RealApplet[] GetApplets()
        {
            return _applets.Values.ToArray();
        }

        public void PauseOldWindows(ulong pid)
        {
            RealApplet applet = GetByAruId(pid);
            if (applet?.CallerApplet != null&&applet?.CallerApplet!=_homeMenu&&applet?.CallerApplet!=_overlayDisp)
            {
                applet.CallerApplet.ProcessHandle.SetActivity(true);
            }
        }

        public void TrackNewProcess(ulong processProcessId, ulong i, bool b)
        {
            TrackProcess(processProcessId, i, b);
        }

        internal RealApplet GetOverlayMenu()
        {
            return _overlayDisp;
        }
        
        internal RealApplet GetHomeMenu()
        {
            return _homeMenu;
        }
    }

    internal class ButtonPressTracker
    {
        private Horizon _system;
        bool _homeButtonPressed = false;
        long _homeButtonPressedTimeStart = 0;
        bool _captureButtonPressed = false;
        long _captureButtonPressedTime = 0;

        public ButtonPressTracker(Horizon system)
        {
            _system = system;
        }

        public void Update()
        {
            // TODO: properly implement this
            ref var shared = ref _system.Device.Hid.SharedMemory;
            bool homeDown = shared.HomeButton.GetCurrentEntryRef().Buttons != 0;
            bool captureDown = shared.CaptureButton.GetCurrentEntryRef().Buttons != 0;

            int homeButtonPressDuration = 0;
            int captureButtonPressDuration = 0;

            if (_homeButtonPressed && !homeDown)
            {
                _homeButtonPressed = false;
                homeButtonPressDuration = (int)(PerformanceCounter.ElapsedMilliseconds - _homeButtonPressedTimeStart);
            }
            else if (!_homeButtonPressed && homeDown)
            {
                _homeButtonPressed = true;
                _homeButtonPressedTimeStart = PerformanceCounter.ElapsedMilliseconds;
            }

            if (_captureButtonPressed && !captureDown)
            {
                _captureButtonPressed = false;
                captureButtonPressDuration = (int)(PerformanceCounter.ElapsedMilliseconds - _captureButtonPressedTime);
            }
            else if (!_captureButtonPressed && captureDown)
            {
                _captureButtonPressed = true;
                _captureButtonPressedTime = PerformanceCounter.ElapsedMilliseconds;
            }

            if (homeButtonPressDuration > 500)
            {
                _system.WindowSystem.OnSystemButtonPress(SystemButtonType.PerformHomeButtonLongPressing);
            }
            else if (homeButtonPressDuration > 20)
            {
                _system.WindowSystem.OnSystemButtonPress(SystemButtonType.PerformHomeButtonShortPressing);
            }

            if (captureButtonPressDuration > 500)
            {
                _system.WindowSystem.OnSystemButtonPress(SystemButtonType.PerformCaptureButtonLongPressing);
            }
            else if (captureButtonPressDuration > 20)
            {
                _system.WindowSystem.OnSystemButtonPress(SystemButtonType.PerformCaptureButtonShortPressing);
            }
        }
    }
}

using System;
using System.Collections.Concurrent;
using Ryujinx.HLE.HOS.Applets;
using Ryujinx.HLE.HOS.Applets.Types;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Am.AppletAE;
using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy;

namespace Ryujinx.HLE.HOS.SystemState
{
    class AppletStateMgr
    {
        #region Public Properties and Fields

        /// <summary>
        /// Queue used for unordered messages.
        /// </summary>
        public ConcurrentQueue<AppletMessage> Messages { get; }

        public bool ForcedSuspend { get; set; }
        public FocusState AcknowledgedFocusState { get; private set; } = FocusState.Background;
        public FocusState RequestedFocusState { get; private set; } = FocusState.Background;

        public bool ResumeNotificationEnabled { get; set; } = false;
        public SuspendMode SuspendMode { get; set; } = SuspendMode.NoOverride;
        public ActivityState ActivityState { get; set; } = ActivityState.ForegroundVisible;

        public KEvent MessageEvent { get; }
        public KEvent OperationModeChangedEvent { get; }
        public KEvent LaunchableEvent { get; }

        public IdDictionary AppletResourceUserIds { get; }
        public IdDictionary IndirectLayerHandles { get; }

        /// <summary>
        /// Indicates that an exit has been requested.
        /// </summary>
        public bool HasRequestedExit => _hasRequestedExit;

        #endregion

        #region Private Fields

        // Flags used for pending notifications.
        private bool _hasRequestedExit = false;
        private bool _hasAcknowledgedExit = false;
        private bool _hasResume = false;
        private bool _hasFocusStateChanged = false;
        private bool _hasRequestedRequestToPrepareSleep = false;
        private bool _hasAcknowledgedRequestToPrepareSleep = false;
        private bool _requestedRequestToDisplayState = false;
        private bool _acknowledgedRequestToDisplayState = false;
        private bool _hasOperationModeChanged = false;
        private bool _hasPerformanceModeChanged = false;
        private bool _hasSdCardRemoved = false;
        private bool _hasSleepRequiredByHighTemperature = false;
        private bool _hasSleepRequiredByLowBattery = false;
        private bool _hasAutoPowerDown = false;
        private bool _hasAlbumScreenShotTaken = false;
        private bool _hasAlbumRecordingSaved = false;

        // Controls whether notifications for particular events are enabled.
        private bool _focusStateChangedNotificationEnabled = true;
        private bool _operationModeChangedNotificationEnabled = true;
        private bool _performanceModeChangedNotificationEnabled = true;

        // Internal event state for message signaling.
        private bool _eventSignaled = false;

        // Indicates how the applet handles focus and suspension.
        private FocusHandlingMode _focusHandlingMode = FocusHandlingMode.NoSuspend;

        #endregion

        #region Properties with Custom Logic

        public bool FocusStateChangedNotificationEnabled
        {
            get => _focusStateChangedNotificationEnabled;
            set
            {
                _focusStateChangedNotificationEnabled = value;
                SignalEventIfNeeded();
            }
        }

        public bool OperationModeChangedNotificationEnabled
        {
            get => _operationModeChangedNotificationEnabled;
            set
            {
                _operationModeChangedNotificationEnabled = value;
                SignalEventIfNeeded();
            }
        }

        public bool PerformanceModeChangedNotificationEnabled
        {
            get => _performanceModeChangedNotificationEnabled;
            set
            {
                _performanceModeChangedNotificationEnabled = value;
                SignalEventIfNeeded();
            }
        }

        #endregion

        #region Constructor

        // Note: The constructor no longer takes an "isApplication" parameter.
        public AppletStateMgr(Horizon system)
        {
            Messages = new ConcurrentQueue<AppletMessage>();

            MessageEvent = new KEvent(system.KernelContext);
            OperationModeChangedEvent = new KEvent(system.KernelContext);
            LaunchableEvent = new KEvent(system.KernelContext);

            AppletResourceUserIds = new IdDictionary();
            IndirectLayerHandles = new IdDictionary();
        }

        #endregion

        #region Public Methods

        public void SetFocusState(FocusState state)
        {
            if (RequestedFocusState != state)
            {
                RequestedFocusState = state;
                _hasFocusStateChanged = true;
                SignalEventIfNeeded();
            }
        }

        public FocusState GetAndClearFocusState()
        {
            AcknowledgedFocusState = RequestedFocusState;
            return AcknowledgedFocusState;
        }

        public void PushUnorderedMessage(AppletMessage message)
        {
            Messages.Enqueue(message);
            SignalEventIfNeeded();
        }

        /// <summary>
        /// Attempts to pop the next pending message. If additional messages remain in the queue,
        /// signals the event so that consumers can continue processing.
        /// </summary>
        public bool PopMessage(out AppletMessage message)
        {
            message = GetNextMessage();
            SignalEventIfNeeded();
            return message != AppletMessage.None;
        }

        public void OnOperationAndPerformanceModeChanged()
        {
            if (_operationModeChangedNotificationEnabled)
            {
                _hasOperationModeChanged = true;
            }
            if (_performanceModeChangedNotificationEnabled)
            {
                _hasPerformanceModeChanged = true;
            }
            OperationModeChangedEvent.ReadableEvent.Signal();
            SignalEventIfNeeded();
        }

        public void OnExitRequested()
        {
            _hasRequestedExit = true;
            SignalEventIfNeeded();
        }

        public void SetFocusHandlingMode(bool suspend)
        {
            // Adjust the focus handling mode based on the desired suspend state.
            _focusHandlingMode = _focusHandlingMode switch
            {
                FocusHandlingMode.AlwaysSuspend or FocusHandlingMode.SuspendHomeSleep when !suspend => FocusHandlingMode.NoSuspend,
                FocusHandlingMode.NoSuspend when suspend => FocusHandlingMode.SuspendHomeSleep,
                _ => _focusHandlingMode,
            };
            SignalEventIfNeeded();
        }

        public void RequestResumeNotification()
        {
            // Note: There is a known bug in AM whereby concurrent resume notifications
            // may cause the first notification to be lost.
            if (ResumeNotificationEnabled)
            {
                _hasResume = true;
                SignalEventIfNeeded();
            }
        }

        public void SetOutOfFocusSuspendingEnabled(bool enabled)
        {
            _focusHandlingMode = _focusHandlingMode switch
            {
                FocusHandlingMode.AlwaysSuspend when !enabled => FocusHandlingMode.SuspendHomeSleep,
                FocusHandlingMode.SuspendHomeSleep or FocusHandlingMode.NoSuspend when enabled => FocusHandlingMode.AlwaysSuspend,
                _ => _focusHandlingMode,
            };
            SignalEventIfNeeded();
        }

        public void RemoveForceResumeIfPossible()
        {
            if (SuspendMode != SuspendMode.ForceResume)
            {
                return;
            }

            // If the activity is already resumed, we can remove the forced state.
            if (ActivityState == ActivityState.ForegroundVisible ||
                ActivityState == ActivityState.ForegroundObscured)
            {
                SuspendMode = SuspendMode.NoOverride;
                return;
            }

            // Without a separate application flag, simply remove forced resume.
            SuspendMode = SuspendMode.NoOverride;
        }

        public bool IsRunnable()
        {
            if (ForcedSuspend)
            {
                return false;
            }

            switch (SuspendMode)
            {
                case SuspendMode.ForceResume:
                    return _hasRequestedExit; // During forced resume, only exit requests make it runnable.
                case SuspendMode.ForceSuspend:
                    return false;
            }

            if (_hasRequestedExit)
            {
                return true;
            }

            if (ActivityState == ActivityState.ForegroundVisible)
            {
                return true;
            }

            if (ActivityState == ActivityState.ForegroundObscured)
            {
                return _focusHandlingMode switch
                {
                    FocusHandlingMode.AlwaysSuspend => false,
                    FocusHandlingMode.SuspendHomeSleep => true,
                    FocusHandlingMode.NoSuspend => true,
                    _ => false,
                };
            }

            // When not in the foreground, run only if suspension is disabled.
            return _focusHandlingMode == FocusHandlingMode.NoSuspend;
        }

        public FocusState GetFocusStateWhileForegroundObscured() =>
            _focusHandlingMode switch
            {
                FocusHandlingMode.AlwaysSuspend => FocusState.InFocus,
                FocusHandlingMode.SuspendHomeSleep => FocusState.OutOfFocus,
                FocusHandlingMode.NoSuspend => FocusState.OutOfFocus,
                _ => throw new IndexOutOfRangeException()
            };

        public FocusState GetFocusStateWhileBackground(bool isObscured) =>
            _focusHandlingMode switch
            {
                FocusHandlingMode.AlwaysSuspend => FocusState.InFocus,
                FocusHandlingMode.SuspendHomeSleep => isObscured ? FocusState.OutOfFocus : FocusState.InFocus,
                // Without an application flag, default to Background.
                FocusHandlingMode.NoSuspend => FocusState.Background,
                _ => throw new IndexOutOfRangeException(),
            };

        public bool UpdateRequestedFocusState()
        {
            FocusState newState;

            if (SuspendMode == SuspendMode.NoOverride)
            {
                newState = ActivityState switch
                {
                    ActivityState.ForegroundVisible => FocusState.InFocus,
                    ActivityState.ForegroundObscured => GetFocusStateWhileForegroundObscured(),
                    ActivityState.BackgroundVisible => GetFocusStateWhileBackground(false),
                    ActivityState.BackgroundObscured => GetFocusStateWhileBackground(true),
                    _ => throw new IndexOutOfRangeException(),
                };
            }
            else
            {
                newState = GetFocusStateWhileBackground(false);
            }

            if (newState != RequestedFocusState)
            {
                RequestedFocusState = newState;
                _hasFocusStateChanged = true;
                SignalEventIfNeeded();
                return true;
            }

            return false;
        }
        
        public void SetFocusForce(bool isFocused, bool shouldSuspend = false)
        {
            Messages.Clear();
            SetFocusHandlingMode(shouldSuspend);
            RequestedFocusState = isFocused ? FocusState.InFocus : FocusState.OutOfFocus;
            Messages.Enqueue(AppletMessage.FocusStateChanged);
            if (isFocused)
            {
                Messages.Enqueue(AppletMessage.ChangeIntoForeground);
            }
            else
            {
                Messages.Enqueue(AppletMessage.ChangeIntoBackground);
            }
            MessageEvent.ReadableEvent.Signal();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks various flags and the message queue in order to return the next pending message.
        /// Flags are cleared as soon as their corresponding message is returned.
        /// </summary>
        private AppletMessage GetNextMessage()
        {
            if (_hasResume)
            {
                _hasResume = false;
                return AppletMessage.Resume;
            }

            if (_hasRequestedExit != _hasAcknowledgedExit)
            {
                _hasAcknowledgedExit = _hasRequestedExit;
                return AppletMessage.Exit;
            }

            // Unify focus state change handling: if the acknowledged focus state does not match the requested one,
            // update it and return the appropriate foreground/background message.
            if (_focusStateChangedNotificationEnabled && RequestedFocusState != AcknowledgedFocusState)
            {
                AcknowledgedFocusState = RequestedFocusState;
                return RequestedFocusState switch
                {
                    FocusState.InFocus  => AppletMessage.ChangeIntoForeground,
                    FocusState.OutOfFocus => AppletMessage.ChangeIntoBackground,
                    _                   => AppletMessage.FocusStateChanged,
                };
            }

            if (_hasRequestedRequestToPrepareSleep != _hasAcknowledgedRequestToPrepareSleep)
            {
                _hasAcknowledgedRequestToPrepareSleep = true;
                return AppletMessage.RequestToPrepareSleep;
            }

            if (_requestedRequestToDisplayState != _acknowledgedRequestToDisplayState)
            {
                _acknowledgedRequestToDisplayState = _requestedRequestToDisplayState;
                return AppletMessage.RequestToDisplay;
            }

            if (_hasOperationModeChanged)
            {
                _hasOperationModeChanged = false;
                return AppletMessage.OperationModeChanged;
            }

            if (_hasPerformanceModeChanged)
            {
                _hasPerformanceModeChanged = false;
                return AppletMessage.PerformanceModeChanged;
            }

            if (_hasSdCardRemoved)
            {
                _hasSdCardRemoved = false;
                return AppletMessage.SdCardRemoved;
            }

            if (_hasSleepRequiredByHighTemperature)
            {
                _hasSleepRequiredByHighTemperature = false;
                return AppletMessage.SleepRequiredByHighTemperature;
            }

            if (_hasSleepRequiredByLowBattery)
            {
                _hasSleepRequiredByLowBattery = false;
                return AppletMessage.SleepRequiredByLowBattery;
            }

            if (_hasAutoPowerDown)
            {
                _hasAutoPowerDown = false;
                return AppletMessage.AutoPowerDown;
            }

            if (_hasAlbumScreenShotTaken)
            {
                _hasAlbumScreenShotTaken = false;
                return AppletMessage.AlbumScreenShotTaken;
            }

            if (_hasAlbumRecordingSaved)
            {
                _hasAlbumRecordingSaved = false;
                return AppletMessage.AlbumRecordingSaved;
            }

            return Messages.TryDequeue(out var message) ? message : AppletMessage.None;
        }

        /// <summary>
        /// Determines whether the internal event should be signaled based on the state flags and message queue.
        /// </summary>
        private bool ShouldSignalEvent()
        {
            bool focusStateChanged = _focusStateChangedNotificationEnabled &&
                                     (RequestedFocusState != AcknowledgedFocusState);

            return !Messages.IsEmpty ||
                   focusStateChanged ||
                   _hasResume ||
                   (_hasRequestedExit != _hasAcknowledgedExit) ||
                   (_hasRequestedRequestToPrepareSleep != _hasAcknowledgedRequestToPrepareSleep) ||
                   _hasOperationModeChanged ||
                   _hasPerformanceModeChanged ||
                   _hasSdCardRemoved ||
                   _hasSleepRequiredByHighTemperature ||
                   _hasSleepRequiredByLowBattery ||
                   _hasAutoPowerDown ||
                   (_requestedRequestToDisplayState != _acknowledgedRequestToDisplayState) ||
                   _hasAlbumScreenShotTaken ||
                   _hasAlbumRecordingSaved;
        }

        /// <summary>
        /// Signals (or clears) the MessageEvent depending on whether there is any pending work.
        /// </summary>
        public void SignalEventIfNeeded()
        {
            bool shouldSignal = ShouldSignalEvent();

            if (_eventSignaled != shouldSignal)
            {
                if (shouldSignal)
                {
                    MessageEvent.ReadableEvent.Signal();
                }
                else
                {
                    MessageEvent.ReadableEvent.Clear();
                }
                _eventSignaled = shouldSignal;
            }
        }

        #endregion
    }
}

﻿using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common;
using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Button;
using System;
using Ryujinx.HLE.HOS.Kernel.Threading;
namespace Ryujinx.HLE.HOS.Services.Hid
{
    public enum ButtonDeviceType
    {
        HomeButton,
        CaptureButton,
    }
    public class ButtonDevice : BaseDevice
    {
        private ButtonDeviceType _type;
        private KEvent _event;
        public ButtonDevice(Switch device, bool active, ButtonDeviceType type) : base(device, active)
        {
            _type = type;
            _event = new KEvent(device.System.KernelContext);
        }
        internal ref KEvent GetEvent()
        {
            return ref _event;
        }
        private ref RingLifo<ButtonState> GetButtonStateLifo()
        {
            switch (_type)
            {
                case ButtonDeviceType.HomeButton:
                    return ref _device.Hid.SharedMemory.HomeButton;
                default:
                    return ref _device.Hid.SharedMemory.CaptureButton;
            }
        }
        public void Update(bool state)
        {
            ref RingLifo<ButtonState> lifo = ref GetButtonStateLifo();
            if (!Active)
            {
                lifo.Clear();
                return;
            }
            ref ButtonState previousEntry = ref lifo.GetCurrentEntryRef();
            ButtonState newState = new()
            {
                SamplingNumber = previousEntry.SamplingNumber + 1,
                Buttons = state ? 1UL : 0UL
            };
            lifo.Write(ref newState);
            _event.ReadableEvent.Signal();
        }
    }
}

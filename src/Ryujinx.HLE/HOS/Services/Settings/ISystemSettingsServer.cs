using LibHac;
using LibHac.Account;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Time.Clock;
using Ryujinx.HLE.HOS.SystemState;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Settings
{
    [Service("set:sys")]
    class ISystemSettingsServer : IpcService
    {
        public ISystemSettingsServer(ServiceCtx context) { }

        [CommandCmif(3)]
        // GetFirmwareVersion() -> buffer<nn::settings::system::FirmwareVersion, 0x1a, 0x100>
        public ResultCode GetFirmwareVersion(ServiceCtx context)
        {
            return GetFirmwareVersion2(context);
        }

        [CommandCmif(4)]
        // GetFirmwareVersion2() -> buffer<nn::settings::system::FirmwareVersion, 0x1a, 0x100>
        public ResultCode GetFirmwareVersion2(ServiceCtx context)
        {
            ulong replyPos = context.Request.RecvListBuff[0].Position;

            context.Response.PtrBuff[0] = context.Response.PtrBuff[0].WithSize(0x100L);

            byte[] firmwareData = GetFirmwareData(context.Device);

            if (firmwareData != null)
            {
                context.Memory.Write(replyPos, firmwareData);

                return ResultCode.Success;
            }

            const byte MajorFwVersion = 0x03;
            const byte MinorFwVersion = 0x00;
            const byte MicroFwVersion = 0x00;
            const byte Unknown = 0x00; //Build?

            const int RevisionNumber = 0x0A;

            const string Platform = "NX";
            const string UnknownHex = "7fbde2b0bba4d14107bf836e4643043d9f6c8e47";
            const string Version = "3.0.0";
            const string Build = "NintendoSDK Firmware for NX 3.0.0-10.0";

            // http://switchbrew.org/index.php?title=System_Version_Title
            using MemoryStream ms = new(0x100);

            BinaryWriter writer = new(ms);

            writer.Write(MajorFwVersion);
            writer.Write(MinorFwVersion);
            writer.Write(MicroFwVersion);
            writer.Write(Unknown);

            writer.Write(RevisionNumber);

            writer.Write(Encoding.ASCII.GetBytes(Platform));

            ms.Seek(0x28, SeekOrigin.Begin);

            writer.Write(Encoding.ASCII.GetBytes(UnknownHex));

            ms.Seek(0x68, SeekOrigin.Begin);

            writer.Write(Encoding.ASCII.GetBytes(Version));

            ms.Seek(0x80, SeekOrigin.Begin);

            writer.Write(Encoding.ASCII.GetBytes(Build));

            context.Memory.Write(replyPos, ms.ToArray());

            return ResultCode.Success;
        }

        [CommandCmif(17)]
        // GetAccountSettings() -> AccountSettings
        public ResultCode GetAccountSettings(ServiceCtx context)
        {
            AccountSettings accountSettings = new AccountSettings
            {
                Flags = 0x295F0
            };
            context.ResponseData.WriteStruct(accountSettings);
            Logger.Stub?.PrintStub(LogClass.ServiceSet);
            return ResultCode.Success;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0x4, Pack = 1)]
        public struct AccountSettings
        {
            public uint Flags;
        }
        
        [CommandCmif(21)]
        // GetEulaVersions(buffer type-0x6>) -> (s32)
        public ResultCode GetEulaVersions(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceSet);
            ulong position = context.Request.ReceiveBuff[0].Position;
            ulong size = context.Request.ReceiveBuff[0].Size;
            byte[] data = new byte[size];
            EulaVersion eulaVersion = new EulaVersion
            {
                Version =  0x10000,
                RegionCode = (byte)context.Device.Configuration.Region,
                ClockType = 0x29580,
                Padding = 0
            };
            byte[] eulaVersionBuffer = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref eulaVersion, 1)).ToArray();
            // Write the EulaVersion struct to the buffer
            context.Memory.Write(position, eulaVersionBuffer);
            context.ResponseData.Write(1);
            return ResultCode.Success;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct EulaVersion
        {
            public uint Version;
            public uint RegionCode;
            public uint ClockType;
            public int Padding;
        }
        
        [CommandCmif(23)]
        // GetColorSetId() -> i32
        public ResultCode GetColorSetId(ServiceCtx context)
        {
            context.ResponseData.Write((int)context.Device.System.State.ThemeColor);

            return ResultCode.Success;
        }

        [CommandCmif(24)]
        // GetColorSetId() -> i32
        public ResultCode SetColorSetId(ServiceCtx context)
        {
            int colorSetId = context.RequestData.ReadInt32();

            context.Device.System.State.ThemeColor = (ColorSet)colorSetId;

            return ResultCode.Success;
        }
        
        [CommandCmif(29)]
        // GetNotificationSettings() -> NotificationSettings
        public ResultCode GetNotificationSettings(ServiceCtx context)
        {
            NotificationSettings notificationSettings = new NotificationSettings
            {
                Flags = 0x1F,
                Volume = 0x1,
                HeadTime = 0x0,
                TailTime = 0x0
            };

            byte[] notificationSettingsBuffer = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref notificationSettings, 1)).ToArray();

            context.ResponseData.Write(notificationSettingsBuffer);

            return ResultCode.Success;
        }
        
        [StructLayout(LayoutKind.Sequential, Size = 0x18, Pack = 1)]
        public struct NotificationSettings
        {
            public uint Flags;             // 0x0 - 0x4
            public uint Volume;            // 0x4 - 0x4
            public ulong HeadTime;         // 0x8 - 0x8
            public ulong TailTime;         // 0x10 - 0x8
        }
        
        [CommandCmif(31)]
        // GetAccountNotificationSettings() -> AccountNotificationSettings
        public ResultCode GetAccountNotificationSettings(ServiceCtx context)
        {
            AccountNotificationSettings accountNotificationSettings = new AccountNotificationSettings
            {
                Uid = new Uid(0, 0),
                Flags = 0x1F,
                FriendPresenceOverlayPermission = 0x1,
                FriendInvitationOverlayPermission = 0x1,
                Reserved1 = 0,
                Reserved2 = 0
            };

            context.ResponseData.WriteStruct(accountNotificationSettings);

            return ResultCode.Success;
        }
        
        [StructLayout(LayoutKind.Sequential, Size = 0x18, Pack = 1)]
        public struct AccountNotificationSettings
        {
            public Uid Uid;                             // 0x0 - 0x10: Unique ID (16 bytes)

            public uint Flags;                             // 0x10 - 0x4: Notification Flags

            public byte FriendPresenceOverlayPermission;   // 0x14 - 0x1: Friend presence overlay permission

            public byte FriendInvitationOverlayPermission; // 0x15 - 0x1: Friend invitation overlay permission
            
            public byte Reserved1;                        // 0x16 - 0x2: Reserved bytes
            public byte Reserved2;                        // 0x16 - 0x2: Reserved bytes
        }
        
        [CommandCmif(37)]
        // GetSettingsItemValueSize(buffer<nn::settings::SettingsName, 0x19>, buffer<nn::settings::SettingsItemKey, 0x19>) -> u64
        public ResultCode GetSettingsItemValueSize(ServiceCtx context)
        {
            ulong classPos = context.Request.PtrBuff[0].Position;
            ulong classSize = context.Request.PtrBuff[0].Size;

            ulong namePos = context.Request.PtrBuff[1].Position;
            ulong nameSize = context.Request.PtrBuff[1].Size;

            byte[] classBuffer = new byte[classSize];

            context.Memory.Read(classPos, classBuffer);

            byte[] nameBuffer = new byte[nameSize];

            context.Memory.Read(namePos, nameBuffer);

            string askedSetting = Encoding.ASCII.GetString(classBuffer).Trim('\0') + "!" + Encoding.ASCII.GetString(nameBuffer).Trim('\0');

            NxSettings.Settings.TryGetValue(askedSetting, out object nxSetting);

            if (nxSetting != null)
            {
                ulong settingSize;

                if (nxSetting is string stringValue)
                {
                    settingSize = (ulong)stringValue.Length + 1;
                }
                else if (nxSetting is int)
                {
                    settingSize = sizeof(int);
                }
                else if (nxSetting is bool)
                {
                    settingSize = 1;
                }
                else
                {
                    throw new NotImplementedException(nxSetting.GetType().Name);
                }

                context.ResponseData.Write(settingSize);
            }

            return ResultCode.Success;
        }

        [CommandCmif(38)]
        // GetSettingsItemValue(buffer<nn::settings::SettingsName, 0x19, 0x48>, buffer<nn::settings::SettingsItemKey, 0x19, 0x48>) -> (u64, buffer<unknown, 6, 0>)
        public ResultCode GetSettingsItemValue(ServiceCtx context)
        {
            ulong classPos = context.Request.PtrBuff[0].Position;
            ulong classSize = context.Request.PtrBuff[0].Size;

            ulong namePos = context.Request.PtrBuff[1].Position;
            ulong nameSize = context.Request.PtrBuff[1].Size;

            ulong replyPos = context.Request.ReceiveBuff[0].Position;
            ulong replySize = context.Request.ReceiveBuff[0].Size;

            byte[] classBuffer = new byte[classSize];

            context.Memory.Read(classPos, classBuffer);

            byte[] nameBuffer = new byte[nameSize];

            context.Memory.Read(namePos, nameBuffer);

            string askedSetting = Encoding.ASCII.GetString(classBuffer).Trim('\0') + "!" + Encoding.ASCII.GetString(nameBuffer).Trim('\0');

            NxSettings.Settings.TryGetValue(askedSetting, out object nxSetting);

            if (nxSetting != null)
            {
                byte[] settingBuffer = new byte[replySize];

                if (nxSetting is string stringValue)
                {
                    if ((ulong)(stringValue.Length + 1) > replySize)
                    {
                        Logger.Error?.Print(LogClass.ServiceSet, $"{askedSetting} String value size is too big!");
                    }
                    else
                    {
                        settingBuffer = Encoding.ASCII.GetBytes(stringValue + "\0");
                    }
                }

                if (nxSetting is int intValue)
                {
                    settingBuffer = BitConverter.GetBytes(intValue);
                }
                else if (nxSetting is bool boolValue)
                {
                    settingBuffer[0] = boolValue ? (byte)1 : (byte)0;
                }
                else
                {
                    throw new NotImplementedException(nxSetting.GetType().Name);
                }

                context.Memory.Write(replyPos, settingBuffer);

                Logger.Debug?.Print(LogClass.ServiceSet, $"{askedSetting} set value: {nxSetting} as {nxSetting.GetType()}");
            }
            else
            {
                Logger.Error?.Print(LogClass.ServiceSet, $"{askedSetting} not found!");
            }

            return ResultCode.Success;
        }

        [CommandCmif(39)]
        // GetTvSettings() -> TvSettings
        public ResultCode GetTvSettings(ServiceCtx context)
        {
            TvSettings tvSettings = new TvSettings
            {
                Flags = 0x1F,
                TvResolution = 0x1,
                HdmiContentType = 0x1,
                RgbRange = 0x1,
                CmuMode = 0x1,
                TvUnderscan = 0x1,
                TvGamma = 0x1,
                ContrastRatio = 0x1
            };

            byte[] tvSettingsBuffer = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref tvSettings, 1)).ToArray();

            context.ResponseData.Write(tvSettingsBuffer);

            return ResultCode.Success;
        }
        
        [StructLayout(LayoutKind.Sequential, Size = 0x20)]
        public struct TvSettings
        {
            public uint Flags;              // Offset 0x0, Size 0x4
            public uint TvResolution;       // Offset 0x4, Size 0x4
            public uint HdmiContentType;    // Offset 0x8, Size 0x4
            public uint RgbRange;           // Offset 0xC, Size 0x4
            public uint CmuMode;            // Offset 0x10, Size 0x4
            public uint TvUnderscan;        // Offset 0x14, Size 0x4
            public uint TvGamma;            // Offset 0x18, Size 0x4
            public uint ContrastRatio;      // Offset 0x1C, Size 0x4
        }
        
        [CommandCmif(47)]
        // GetQuestFlag() -> bool
        public ResultCode GetQuestFlag(ServiceCtx context)
        {
            // NOTE: Gets a flag determining whether the console is a kiosk unit (codenamed "Quest"). Used by qlaunch to determine whether to launch Retail Interactive Display Menu. 
            context.ResponseData.Write(true);

            Logger.Stub?.PrintStub(LogClass.ServiceSet);

            return ResultCode.Success;
        }
        
        [CommandCmif(60)]
        // IsUserSystemClockAutomaticCorrectionEnabled() -> bool
        public ResultCode IsUserSystemClockAutomaticCorrectionEnabled(ServiceCtx context)
        {
            // NOTE: When set to true, is automatically synced with the internet.
            context.ResponseData.Write(true);

            Logger.Stub?.PrintStub(LogClass.ServiceSet);

            return ResultCode.Success;
        }

        [CommandCmif(62)]
        // GetDebugModeFlag() -> bool
        public ResultCode GetDebugModeFlag(ServiceCtx context)
        {
            context.ResponseData.Write(false);

            Logger.Stub?.PrintStub(LogClass.ServiceSet);

            return ResultCode.Success;
        }
        
        [CommandCmif(63)]
        // GetPrimaryAlbumStorage() -> PrimaryAlbumStorage
        public ResultCode GetPrimaryAlbumStorage(ServiceCtx context)
        {
            context.ResponseData.Write((uint)PrimaryAlbumStorage.SdCard);

            Logger.Stub?.PrintStub(LogClass.ServiceSet);

            return ResultCode.Success;
        }
        
        public enum PrimaryAlbumStorage : uint
        {
            Nand = 0,    // 0 - Nand storage
            SdCard = 1   // 1 - SD card storage
        }
        
        [CommandCmif(71)]
        // GetSleepSettings() -> SleepSettings
        public ResultCode GetSleepSettings(ServiceCtx context)
        {
            SleepSettings sleepSettings = new SleepSettings
            {
                Flags = 0x1F,
                HandheldSleepPlan = 0x1,
                ConsoleSleepPlan = 0x1
            };

            byte[] sleepSettingsBuffer = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref sleepSettings, 1)).ToArray();

            context.ResponseData.Write(sleepSettingsBuffer);

            return ResultCode.Success;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0xC)] // Total size: 0xC bytes
        public struct SleepSettings
        {
            public uint Flags;               // Offset: 0x0, Size: 0x4
            public uint HandheldSleepPlan;   // Offset: 0x4, Size: 0x4
            public uint ConsoleSleepPlan;    // Offset: 0x8, Size: 0x4
        }
        
        [CommandCmif(75)]
        // GetInitialLaunchSettings() -> InitialLaunchSettings
        public ResultCode GetInitialLaunchSettings(ServiceCtx context)
        {
            InitialLaunchSettings launchSettings = new InitialLaunchSettings
            {
                Flags = new InitialLaunchFlag
                {
                    InitialLaunchCompletionFlag = true,
                    InitialLaunchUserAdditionFlag = false,
                    InitialLaunchTimestampFlag = true
                }.Raw,
                TimeStamp = SteadyClockTimePoint.GetRandom()
            };
            byte[] launchSettingsBuffer = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref launchSettings, 1)).ToArray();
            
            context.ResponseData.Write(launchSettingsBuffer);

            Logger.Stub?.PrintStub(LogClass.ServiceSet);

            return ResultCode.Success;
        }
        
        // This is "nn::settings::system::InitialLaunchSettings". This struct is 8-byte aligned.
        [StructLayout(LayoutKind.Explicit, Size = 0x20)]
        public struct InitialLaunchSettings
        {
            [FieldOffset(0x0)]
            public uint Flags;

            [FieldOffset(0x4)]
            private uint Padding; // Adds the necessary 4 bytes of padding.

            [FieldOffset(0x8)]
            public SteadyClockTimePoint TimeStamp;
        }
        
        public struct InitialLaunchFlag
        {
            private uint _raw;

            public uint Raw
            {
                get => _raw;
                set => _raw = value;
            }

            public bool InitialLaunchCompletionFlag
            {
                get => (_raw & (1 << 0)) != 0;
                set
                {
                    if (value) _raw |= (1u << 0);
                    else _raw &= ~(1u << 0);
                }
            }

            public bool InitialLaunchUserAdditionFlag
            {
                get => (_raw & (1 << 8)) != 0;
                set
                {
                    if (value) _raw |= (1u << 8);
                    else _raw &= ~(1u << 8);
                }
            }

            public bool InitialLaunchTimestampFlag
            {
                get => (_raw & (1 << 16)) != 0;
                set
                {
                    if (value) _raw |= (1u << 16);
                    else _raw &= ~(1u << 16);
                }
            }
        }
        
        [CommandCmif(77)]
        // GetDeviceNickName() -> buffer<nn::settings::system::DeviceNickName, 0x16>
        public ResultCode GetDeviceNickName(ServiceCtx context)
        {
            ulong deviceNickNameBufferPosition = context.Request.ReceiveBuff[0].Position;
            ulong deviceNickNameBufferSize = context.Request.ReceiveBuff[0].Size;

            if (deviceNickNameBufferPosition == 0)
            {
                return ResultCode.NullDeviceNicknameBuffer;
            }

            if (deviceNickNameBufferSize != 0x80)
            {
                Logger.Warning?.Print(LogClass.ServiceSet, "Wrong buffer size");
            }

            context.Memory.Write(deviceNickNameBufferPosition, Encoding.ASCII.GetBytes(context.Device.System.State.DeviceNickName + '\0'));

            return ResultCode.Success;
        }

        [CommandCmif(78)]
        // SetDeviceNickName(buffer<nn::settings::system::DeviceNickName, 0x15>)
        public ResultCode SetDeviceNickName(ServiceCtx context)
        {
            ulong deviceNickNameBufferPosition = context.Request.SendBuff[0].Position;
            ulong deviceNickNameBufferSize = context.Request.SendBuff[0].Size;

            byte[] deviceNickNameBuffer = new byte[deviceNickNameBufferSize];

            context.Memory.Read(deviceNickNameBufferPosition, deviceNickNameBuffer);

            context.Device.System.State.DeviceNickName = Encoding.ASCII.GetString(deviceNickNameBuffer);

            return ResultCode.Success;
        }
        
        [CommandCmif(79)]
        // GetProductModel() -> s32
        public ResultCode GetProductModel(ServiceCtx context)
        {
            context.ResponseData.Write(1);

            Logger.Stub?.PrintStub(LogClass.ServiceSet);

            return ResultCode.Success;
        }
        
        [CommandCmif(90)]
        // GetMiiAuthorId() -> nn::util::Uuid
        public ResultCode GetMiiAuthorId(ServiceCtx context)
        {
            // NOTE: If miiAuthorId is null ResultCode.NullMiiAuthorIdBuffer is returned.
            //       Doesn't occur in our case.

            context.ResponseData.Write(Mii.Helper.GetDeviceId());

            return ResultCode.Success;
        }
        
        [CommandCmif(95)]
        // GetAutoUpdateEnableFlag() -> bool
        public ResultCode GetAutoUpdateEnableFlag(ServiceCtx context)
        {
            context.ResponseData.Write(false);

            Logger.Stub?.PrintStub(LogClass.ServiceSet);

            return ResultCode.Success;
        }
        
        [CommandCmif(99)]
        // GetBatteryPercentageFlag() -> u8
        public ResultCode GetBatteryPercentageFlag(ServiceCtx context)
        {
            context.ResponseData.Write(100);

            Logger.Stub?.PrintStub(LogClass.ServiceSet);

            return ResultCode.Success;
        }
        
        [CommandCmif(124)]
        // GetErrorReportSharePermission() -> s32
        public ResultCode GetErrorReportSharePermission(ServiceCtx context)
        {
            context.ResponseData.Write(1);

            Logger.Stub?.PrintStub(LogClass.ServiceSet);

            return ResultCode.Success;
        }

        [CommandCmif(126)]
        // GetAppletLaunchFlags() -> u32
        public ResultCode GetAppletLaunchFlags(ServiceCtx context)
        {
            // NOTE: I do not know what this is used for but it is used by qlaunch.
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceSet);

            return ResultCode.Success;
        }

        [CommandCmif(136)]
        // GetKeyboardLayout() -> s32
        public ResultCode GetKeyboardLayout(ServiceCtx context)
        {
            context.ResponseData.Write((int)KeyboardLayout.Default);

            Logger.Stub?.PrintStub(LogClass.ServiceSet);

            return ResultCode.Success;
        }
        
        [CommandCmif(170)]
        // GetChineseTraditionalInputMethod() -> s32
        public ResultCode GetChineseTraditionalInputMethod(ServiceCtx context)
        {
            context.ResponseData.Write(1);

            Logger.Stub?.PrintStub(LogClass.ServiceSet);

            return ResultCode.Success;
        }
        
        [CommandCmif(201)]
        // GetFieldTestingFlag() -> bool
        public ResultCode GetFieldTestingFlag(ServiceCtx context)
        {
            context.ResponseData.Write(false);

            Logger.Stub?.PrintStub(LogClass.ServiceSet);

            return ResultCode.Success;
        }
        
        public byte[] GetFirmwareData(Switch device)
        {
            const ulong SystemVersionTitleId = 0x0100000000000809;

            string contentPath = device.System.ContentManager.GetInstalledContentPath(SystemVersionTitleId, StorageId.BuiltInSystem, NcaContentType.Data);

            if (string.IsNullOrWhiteSpace(contentPath))
            {
                return null;
            }

            string firmwareTitlePath = FileSystem.VirtualFileSystem.SwitchPathToSystemPath(contentPath);

            using IStorage firmwareStorage = new LocalStorage(firmwareTitlePath, FileAccess.Read);
            Nca firmwareContent = new(device.System.KeySet, firmwareStorage);

            if (!firmwareContent.CanOpenSection(NcaSectionType.Data))
            {
                return null;
            }

            IFileSystem firmwareRomFs = firmwareContent.OpenFileSystem(NcaSectionType.Data, device.System.FsIntegrityCheckLevel);

            using var firmwareFile = new UniqueRef<IFile>();

            Result result = firmwareRomFs.OpenFile(ref firmwareFile.Ref, "/file".ToU8Span(), OpenMode.Read);
            if (result.IsFailure())
            {
                return null;
            }

            result = firmwareFile.Get.GetSize(out long fileSize);
            if (result.IsFailure())
            {
                return null;
            }

            byte[] data = new byte[fileSize];

            result = firmwareFile.Get.Read(out _, 0, data);
            if (result.IsFailure())
            {
                return null;
            }

            return data;
        }
    }
}

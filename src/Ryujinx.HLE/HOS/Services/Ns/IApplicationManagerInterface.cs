using LibHac.Ns;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Utilities;
using Ryujinx.Horizon.Common;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Ns.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ApplicationId = LibHac.ApplicationId;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    [Service("ns:am")]
    class IApplicationManagerInterface : IpcService
    {
        private KEvent _applicationRecordUpdateSystemEvent;
        private int _applicationRecordUpdateSystemEventHandle;

        private KEvent _sdCardMountStatusChangedEvent;
        private int _sdCardMountStatusChangedEventHandle;

        private KEvent _gameCardUpdateDetectionEvent;
        private int _gameCardUpdateDetectionEventHandle;

        private KEvent _gameCardMountFailureEvent;
        private int _gameCardMountFailureEventHandle;        
        
        private KEvent _gameCardWakeEvent;
        private int _gameCardWakeEventHandle;

        public IApplicationManagerInterface(ServiceCtx context)
        {
            _applicationRecordUpdateSystemEvent = new KEvent(context.Device.System.KernelContext);
            _sdCardMountStatusChangedEvent = new KEvent(context.Device.System.KernelContext);
            _gameCardUpdateDetectionEvent = new KEvent(context.Device.System.KernelContext);
            _gameCardMountFailureEvent = new KEvent(context.Device.System.KernelContext);            
            _gameCardWakeEvent = new KEvent(context.Device.System.KernelContext);
        }


        [CommandCmif(0)]
        // ListApplicationRecord(s32) -> (s32, buffer<ApplicationRecord[], 6>)
        // entry_offset -> (out_entrycount, ApplicationRecord[])
        public ResultCode ListApplicationRecord(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);
            int entryOffset = context.RequestData.ReadInt32();
            ulong position = context.Request.ReceiveBuff[0].Position;
            List<ApplicationRecord> records = new();
            int sID = 24;
            foreach (RyuApplicationData title in context.Device.Configuration.Titles)
            {
                records.Add(new ApplicationRecord()
                {
                    AppId = title.AppId,
                    Type = ApplicationRecordType.Installed,
                    Unknown = 0,
                    Unknown2 = (byte)sID++
                });
            }
            
            records.Sort((x, y) => (int)(x.AppId.Value - y.AppId.Value));
            if (entryOffset > 0)
            {
                records = records.Skip(entryOffset - 1).ToList();
            }

            context.ResponseData.Write(records.Count);
            foreach (var record in records)
            {
                context.Memory.Write(position, StructToBytes(record));
                position += (ulong)Marshal.SizeOf<ApplicationRecord>();
            }

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // GetApplicationRecordUpdateSystemEvent() -> handle<copy>
        public ResultCode GetApplicationRecordUpdateSystemEvent(ServiceCtx context)
        {
            if (_applicationRecordUpdateSystemEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_applicationRecordUpdateSystemEvent.ReadableEvent, out _applicationRecordUpdateSystemEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_applicationRecordUpdateSystemEventHandle);

            return ResultCode.Success;
        }
        
        [CommandCmif(3)]
        // GetApplicationView(buffer<unknown, 5>) -> buffer<unknown, 6>
        public ResultCode GetApplicationViewDeperecated(ServiceCtx context)
        {
            ulong inPosition = context.Request.SendBuff[0].Position;
            ulong inSize = context.Request.SendBuff[0].Size;
            ulong outputPosition = context.Request.ReceiveBuff[0].Position;
            ulong outputSize = context.Request.ReceiveBuff[0].Size;

            List<ApplicationId> applicationIds = new();
            for (ulong i = 0; i < inSize / sizeof(ulong); i++)
            {
                ulong position = inPosition + (i * sizeof(ulong));
                applicationIds.Add(new(context.Memory.Read<ulong>(position)));
            }

            List<ApplicationView> views = new();

            foreach (ApplicationId applicationId in applicationIds)
            {
                views.Add(new()
                {
                    AppId = applicationId,
                    Unknown1 = 0x70000,
                    Flags = 0x401f17,
                    Unknown2 = new byte[0x40]
                });
            }

            context.ResponseData.Write(views.Count);
            foreach (var view in views)
            {
                context.Memory.Write(outputPosition, StructToBytes(view));
                outputPosition += (ulong)Marshal.SizeOf<ApplicationView>();
            }

            return ResultCode.Success;
        }

        [CommandCmif(44)]
        // GetSdCardMountStatusChangedEvent() -> handle<copy>
        public ResultCode GetSdCardMountStatusChangedEvent(ServiceCtx context)
        {
            if (_sdCardMountStatusChangedEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_sdCardMountStatusChangedEvent.ReadableEvent, out _sdCardMountStatusChangedEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_sdCardMountStatusChangedEventHandle);

            return ResultCode.Success;
        }

        const long storageFreeAndTotalSpaceSize = 6999999999999L;
        [CommandCmif(47)]
        // GetTotalSpaceSize(u8 storage_id) -> u64
        public ResultCode GetTotalSpaceSize(ServiceCtx context)
        {
            long storageId = context.RequestData.ReadByte();
            
            
            context.ResponseData.Write(storageFreeAndTotalSpaceSize);

            return ResultCode.Success;
        }
        
        [CommandCmif(48)]
        // GetFreeSpaceSize(u8 storage_id) -> u64
        public ResultCode GetFreeSpaceSize(ServiceCtx context)
        {
            long storageId = context.RequestData.ReadByte();

            context.ResponseData.Write(storageFreeAndTotalSpaceSize);

            return ResultCode.Success;
        }
        
        [CommandCmif(52)]
        // GetGameCardUpdateDetectionEvent() -> handle<copy>
        public ResultCode GetGameCardUpdateDetectionEvent(ServiceCtx context)
        {
            if (_gameCardUpdateDetectionEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_gameCardUpdateDetectionEvent.ReadableEvent, out _gameCardUpdateDetectionEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_gameCardUpdateDetectionEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(55)]
        // GetApplicationDesiredLanguage(u8) -> u8
        public ResultCode GetApplicationDesiredLanguage(ServiceCtx context)
        {
            byte source = context.RequestData.ReadByte();
            byte language = 0;

            Logger.Stub?.PrintStub(LogClass.ServiceNs, new { source, language });

            context.ResponseData.Write(language);

            return ResultCode.Success;
        }

        [CommandCmif(70)]
        // ResumeAll()
        public ResultCode ResumeAll(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNs);

            return ResultCode.Success;
        }

        [CommandCmif(505)]
        // GetGameCardMountFailureEvent() -> handle<copy>
        public ResultCode GetGameCardMountFailureEvent(ServiceCtx context)
        {
            if (_gameCardMountFailureEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_gameCardMountFailureEvent.ReadableEvent, out _gameCardMountFailureEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_gameCardMountFailureEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(400)]
        // GetApplicationControlData(u8, u64) -> (unknown<4>, buffer<unknown, 6>)
        public ResultCode GetApplicationControlData(ServiceCtx context)
        {
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            byte source = (byte)context.RequestData.ReadInt64();
            ulong titleId = context.RequestData.ReadUInt64();
#pragma warning restore IDE0059

            ulong position = context.Request.ReceiveBuff[0].Position;

            var title = context.Device.Configuration.Titles.FirstOrDefault(x => x.AppId.Value == titleId);

            ApplicationControlProperty nacp = title.Nacp;

            context.Memory.Write(position, SpanHelpers.AsByteSpan(ref nacp).ToArray());
            if (title.Icon?.Length > 0)
            {
                context.Memory.Write(position + 0x4000, title.Icon);

                context.ResponseData.Write(0x4000 + title.Icon.Length);
            }

            return ResultCode.Success;
        }
        
        [CommandCmif(403)]
        // GetMaxApplicationControlCacheCount() -> u32
        public ResultCode GetMaxApplicationControlCacheCount(ServiceCtx context)
        {
            // TODO: Implement this method properly.
            Logger.Stub?.PrintStub(LogClass.Service);
            return ResultCode.Success;
        }
        
        [CommandCmif(511)]
        // GetGameCardWakenReadyEvent() -> handle<copy>
        public ResultCode GetGameCardWakenReadyEvent(ServiceCtx context)
        {
            if (_gameCardWakeEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_gameCardWakeEvent.ReadableEvent, out _gameCardWakeEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_gameCardWakeEventHandle);
            _gameCardWakeEvent.ReadableEvent.Signal();

            return ResultCode.Success;
        } 
        
        [CommandCmif(512)]
        // IsGameCardApplicationRunning() -> bool
        public ResultCode IsGameCardApplicationRunning(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNs);
            context.ResponseData.Write(true);
            return ResultCode.Success;
        }

        [CommandCmif(1701)]
        // GetApplicationView(buffer<unknown, 5>) -> buffer<unknown, 6>
        public ResultCode GetApplicationView(ServiceCtx context)
        {
            ulong inPosition = context.Request.SendBuff[0].Position;
            ulong inSize = context.Request.SendBuff[0].Size;
            ulong outputPosition = context.Request.ReceiveBuff[0].Position;
            ulong outputSize = context.Request.ReceiveBuff[0].Size;

            List<ApplicationId> applicationIds = new();
            for (ulong i = 0; i < inSize / sizeof(ulong); i++)
            {
                ulong position = inPosition + (i * sizeof(ulong));
                applicationIds.Add(new(context.Memory.Read<ulong>(position)));
            }

            List<ApplicationView> views = new();

            foreach (ApplicationId applicationId in applicationIds)
            {
                views.Add(new()
                {
                    AppId = applicationId,
                    Unknown1 = 0x70000,
                    Flags = 0x401f17,
                    Unknown2 = new byte[0x40]
                });
            }

            context.ResponseData.Write(views.Count);
            foreach (var view in views)
            {
                context.Memory.Write(outputPosition, StructToBytes(view));
                outputPosition += (ulong)Marshal.SizeOf<ApplicationView>();
            }

            return ResultCode.Success;
        }
        [CommandCmif(1704)]
        // GetApplicationView(buffer<unknown, 5>) -> buffer<unknown, 6>
        public ResultCode GetApplicationViewWithPromotionInfo(ServiceCtx context)
        {
            ulong inPosition = context.Request.SendBuff[0].Position;
            ulong inSize = context.Request.SendBuff[0].Size;
            ulong outputPosition = context.Request.ReceiveBuff[0].Position;
            ulong outputSize = context.Request.ReceiveBuff[0].Size;

            List<ApplicationId> applicationIds = new();
            for (ulong i = 0; i < inSize / sizeof(ulong); i++)
            {
                ulong position = inPosition + (i * sizeof(ulong));
                applicationIds.Add(new(context.Memory.Read<ulong>(position)));
            }

            List<ApplicationView> views = new();

            foreach (ApplicationId applicationId in applicationIds)
            {
                views.Add(new()
                {
                    AppId = applicationId,
                    Unknown1 = 0x70000,
                    Flags = 0x401f17,
                    Unknown2 = new byte[0x40]
                });
            }
            
            List<ApplicationViewWithPromotionInfo> promotions = new();
            foreach(ApplicationView view in views)
            {
                promotions.Add(new()
                {
                    AppView = view,
                    Promotion = new()
                });
            }

            context.ResponseData.Write(views.Count);
            foreach (var promotion in promotions)
            {
                context.Memory.Write(outputPosition, StructToBytes(promotion));
                outputPosition += (ulong)Marshal.SizeOf<ApplicationViewWithPromotionInfo>();
            }

            return ResultCode.Success;
        }
    }
}

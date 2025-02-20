using LibHac.Common.FixedArrays;
using LibHac.FsSystem;
using Ryujinx.Common;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Memory;
using Ryujinx.HLE.HOS.Services.Caps.Types;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Caps
{
    [Service("caps:a")]
    class IAlbumAccessorService : IpcService
    {
        public Dictionary<AlbumFileDateTime,string> AlbumFiles { get; set; }
        public IAlbumAccessorService(ServiceCtx context) { }
        
        [CommandCmif(1)]
        [CommandCmif(101)]
        // GetAlbumFileList(unknown<u8>) -> (unknown<8>, buffer<unknown, 6>)
        public ResultCode GetAlbumFileList(ServiceCtx context)
        {
            int storageId = context.RequestData.ReadInt32();
            // 0 = Nand or 1 = Sd Card
            if (storageId == 1)
            {
                return ResultCode.Success;
            }
            Logger.Info?.Print(LogClass.ServiceCaps, $"Initializing album files with storage ID {storageId}.");
            Logger.Stub?.PrintStub(LogClass.ServiceCaps);
            string path = Path.Combine(AppDataManager.BaseDirPath, "screenshots");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            int count = 0;
            var buffer = context.Request.ReceiveBuff[0];
            ulong position = buffer.Position;
            AlbumFiles = new Dictionary<AlbumFileDateTime, string>();
            int limit = 10000;
            Span<AlbumEntry> entries = stackalloc AlbumEntry[limit];
            Logger.Info?.Print(LogClass.Application, $"Limitting to {entries.Length} photos.");
            foreach (string file in System.IO.Directory.GetFiles(path))
            {
                if (count+1 >= entries.Length)
                {
                    Logger.Warning?.Print(LogClass.ServiceCaps,$"Too many screenshots. Limiting to {entries.Length}.");
                    break;
                }
                if (System.IO.Path.GetFileName(file).EndsWith(".png") || System.IO.Path.GetFileName(file).EndsWith(".jpg"))
                {
                    Logger.Stub?.Print(LogClass.Application, $"Adding screenshot {System.IO.Path.GetFileName(file)}");
                    AlbumEntry album_entry = new AlbumEntry();
                    album_entry.EntrySize = (ulong) System.IO.Path.GetFileName(file).Length;
                    album_entry.FileId = new AlbumFileId();
                    album_entry.FileId.ApplicationId = 0x0;
                    album_entry.FileId.Time = FromDateTime(System.IO.File.GetLastWriteTimeUtc(file), (byte)count);
                    if (AlbumFiles.ContainsKey(album_entry.FileId.Time))
                    {
                        Logger.Warning?.Print(LogClass.ServiceCaps,$"Duplicate photo found {System.IO.Path.GetFileName(file)}. Skipping.");
                        continue;
                    }
                    album_entry.FileId.Storage = (byte)AlbumStorage.Sd;
                    album_entry.FileId.Contents = 0;
                    album_entry.FileId.Field19_0 = 0;
                    album_entry.FileId.Field19_1 = 0;
                    album_entry.FileId.Reserved = 0;
                    entries[count] = album_entry;
                    count++;
                    AlbumFiles.Add(album_entry.FileId.Time, file);
                }
            }
            byte[] entryArray = MemoryMarshal.Cast<AlbumEntry, byte>(MemoryMarshal.CreateReadOnlySpan(ref entries[0], count)).ToArray();
            context.Memory.Write(buffer.Position, entryArray);
            Logger.Info?.Print(LogClass.ServiceCaps, $"GetAlbumFileCount(): returning {count}");
            context.ResponseData.Write(count);
            return ResultCode.Success;
        }
        
        [CommandCmif(2)]
        // LoadAlbumScreenShotImage(unknown<0x18>) -> (unknown<8>, buffer<unknown, 6>)
        public ResultCode LoadAlbumScreenShotImage(ServiceCtx context)
        {
            var fileId = context.RequestData.ReadStruct<AlbumFileId>();
            return LoadImage(1280, 720, context, fileId);
        }
        
        [CommandCmif(5)]
        // IsAlbumMounted() -> bool
        public ResultCode IsAlbumMounted(ServiceCtx context)
        {
            // TODO: Implement this properly.
            Logger.Stub?.PrintStub(LogClass.ServiceCaps);
            context.ResponseData.Write(true);
            return ResultCode.Success;
        }
        
       [CommandCmif(8)]
       // LoadAlbumScreenShotImageEx0
       public ResultCode LoadAlbumScreenShotImageEx0(ServiceCtx context)
       {
           var fileId = context.RequestData.ReadStruct<AlbumFileId>();
           return LoadImage(1280, 720, context, fileId);
       }
       
       [CommandCmif(14)]
       public ResultCode LoadAlbumScreenShotThumbnail(ServiceCtx context)
       {
           var fileId = context.RequestData.ReadStruct<AlbumFileId>();
           return LoadImage(320, 180, context, fileId);
       }
        
        public ResultCode LoadImageEx(int width, int height, ServiceCtx context, AlbumFileId fileId)
        {
            var outputBuffer = context.Request.ReceiveBuff[0];
            var inputBuffer = context.Request.ReceiveBuff[1];
            
            var output = new LoadAlbumScreenShotImageOutput
            {
                width = width,
                height = height,
                attribute = new ScreenShotAttribute{
                    Unknown0x00 = {},
                    AlbumImageOrientation = AlbumImageOrientation.Degrees0,
                    Unknown0x08 = {},
                    Unknown0x10 = {}
                }
            };
            
            string imagePath = AlbumFiles[fileId.Time];

            ScaleBytes(width, height, imagePath,  out Span<byte> scaledBytes);
            
            ReadOnlySpan<byte> data = MemoryMarshal.Cast<LoadAlbumScreenShotImageOutput, byte>(MemoryMarshal.CreateReadOnlySpan(ref output, 1));
            byte[] outputBytes = data.ToArray();
            
            context.Memory.Write(outputBuffer.Position, outputBytes);
            context.Memory.Write(inputBuffer.Position, scaledBytes);
            return ResultCode.Success;
        }
        
        public ResultCode LoadImage(int width, int height, ServiceCtx context, AlbumFileId fileId)
        {
            var outputBuffer = context.Request.ReceiveBuff[0];
            
            var output = new LoadAlbumScreenShotImageOutput
            {
                width = width,
                height = height,
                attribute = new ScreenShotAttribute{
                    Unknown0x00 = {},
                    AlbumImageOrientation = AlbumImageOrientation.Degrees0,
                    Unknown0x08 = {},
                    Unknown0x10 = {}
                }
            };
            
            string imagePath = AlbumFiles[fileId.Time];
            
            ScaleBytes(width, height, imagePath,  out Span<byte> scaledBytes);
            
            ReadOnlySpan<byte> data = MemoryMarshal.Cast<LoadAlbumScreenShotImageOutput, byte>(MemoryMarshal.CreateReadOnlySpan(ref output, 1));
            byte[] outputBytes = data.ToArray();
            
            context.ResponseData.Write(outputBytes);
            context.Memory.Write(outputBuffer.Position, scaledBytes);
            return ResultCode.Success;
        }
        
        public ResultCode LoadImageEx1(int width, int height, ServiceCtx context, AlbumFileId fileId)
        {
            var outputImageSettings = context.Request.ReceiveBuff[0];
            var outputData = context.Request.ReceiveBuff[1];
            var buff3 = context.Request.ReceiveBuff[2];
            Logger.Info?.Print(LogClass.ServiceCaps, $"Loading thumbnail for {fileId.Time.UniqueId}");
             
            var output = new LoadAlbumScreenShotImageOutput
            {
                width = width,
                height = height,
                attribute = new ScreenShotAttribute{
                    Unknown0x00 = {},
                    AlbumImageOrientation = AlbumImageOrientation.Degrees0,
                    Unknown0x08 = {},
                    Unknown0x10 = {}
                }
            };
            
            string imagePath = AlbumFiles[fileId.Time];
            
            ScaleBytes(width, height, imagePath, out Span<byte> scaledBytes);
            
            ReadOnlySpan<byte> data = MemoryMarshal.Cast<LoadAlbumScreenShotImageOutput, byte>(MemoryMarshal.CreateReadOnlySpan(ref output, 1));
            byte[] outputBytes = data.ToArray();
            
            context.Memory.Write(outputImageSettings.Position, outputBytes);
            context.Memory.Write(outputData.Position, scaledBytes);
            
            return ResultCode.Success;
        }
        
        public void ScaleBytes(int width, int height, string imagePath, out Span<byte> output_bytes)
        {
            using (SKBitmap bitmap = SKBitmap.Decode(imagePath))
            {
                if (bitmap == null)
                    throw new ArgumentException("Unable to decode the input image.");

                // STBI_rgb_alpha
                SKImageInfo targetInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
                using (SKBitmap scaledBitmap = new SKBitmap(targetInfo))
                {
                    using (SKCanvas canvas = new SKCanvas(scaledBitmap))
                    {
                        canvas.Clear(SKColors.Transparent); // Clear canvas to avoid artifacts
                
                        var paint = new SKPaint
                        {
                            FilterQuality = SKFilterQuality.High, // High-quality scaling
                            IsAntialias = true, // Smooth edges
                        };
                
                        // Draw the scaled image
                        canvas.DrawBitmap(bitmap, new SKRect(0, 0, width, height), paint);
                    }
                    
                    output_bytes = scaledBitmap.Bytes;
                }
            }
        }
        
        [CommandCmif(18)]
        // GetAppletProgramIdTable(buffer<unknown, 70>) -> bool
        public ResultCode GetAppletProgramIdTable(ServiceCtx context)
        {
            ulong tableBufPos = context.Request.ReceiveBuff[0].Position;
            ulong tableBufSize = context.Request.ReceiveBuff[0].Size;

            if (tableBufPos == 0)
            {
                return ResultCode.NullOutputBuffer;
            }

            context.Memory.Write(tableBufPos, 0x0100000000001000UL);
            context.Memory.Write(tableBufPos + 8, 0x0100000000001fffUL);

            context.ResponseData.Write(true);

            return ResultCode.Success;
        }
        
        [CommandCmif(401)]
        // GetAutoSavingStorage() -> bool
        public ResultCode GetAutoSavingStorage(ServiceCtx context)
        {
            // TODO: Implement this properly.
            Logger.Stub?.PrintStub(LogClass.ServiceCaps);
            context.ResponseData.Write(false);
            return ResultCode.Success;
        }
        
        [CommandCmif(1001)]
        // LoadAlbumScreenShotThumbnailImageEx0(unknown<0x38>) -> (unknown<0x50>, buffer<unknown, 0x46>, buffer<unknown, 6>)
        public ResultCode LoadAlbumScreenShotThumbnailImageEx0(ServiceCtx context)
        {
            var fileId = context.RequestData.ReadStruct<AlbumFileId>();
            return LoadImageEx(320, 180, context, fileId);
        }
        
        [CommandCmif(1002)]
        // LoadAlbumScreenShotImageEx1(unknown<0x38>) -> (buffer<unknown, 0x16>, buffer<unknown, 0x46>, buffer<unknown, 6>)
        public ResultCode LoadAlbumScreenShotImageEx1(ServiceCtx context)
        {
            var fileId = context.RequestData.ReadStruct<AlbumFileId>();
            
            return LoadImageEx1(1280, 720, context, fileId);
        }
        
        [CommandCmif(1003)]
        public ResultCode LoadAlbumScreenShotThumbnailImageEx1(ServiceCtx context)
        {
            var fileId = context.RequestData.ReadStruct<AlbumFileId>();
            return LoadImageEx1(320, 180, context, fileId);
        }
        
        [CommandCmif(50011)] // 19.0.0+
        // GetAlbumAccessResultForDebug()
        public ResultCode GetAlbumAccessResultForDebug(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceCaps);
            return ResultCode.Success;
        }

        public void GetWidthAndHeightFromInputBuffer(AlbumFileId id, out int width, out int height)
        {
            string path = AlbumFiles[id.Time];
            width = 0;
            height = 0;
            using (SKBitmap bitmap = SKBitmap.Decode(path))
            {
                if (bitmap != null)
                {
                    width = bitmap.Width;
                    height = bitmap.Height;
                }
            }
        }

        public static AlbumFileDateTime FromDateTime(DateTime dateTime, byte id)
        {
            return new AlbumFileDateTime
            {
                Year = (ushort)dateTime.Year,
                Month = (byte)dateTime.Month,
                Day = (byte)dateTime.Day,
                Hour = (byte)dateTime.Hour,
                Minute = (byte)dateTime.Minute,
                Second = (byte)dateTime.Second,
                UniqueId = id
            };
        }
    }
}

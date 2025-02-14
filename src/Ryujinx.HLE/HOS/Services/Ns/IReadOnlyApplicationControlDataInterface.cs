using LibHac.Common;
using LibHac.Ns;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Ns.Types;
using System;
using System.Linq;
using ApplicationId = LibHac.ApplicationId;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    class IReadOnlyApplicationControlDataInterface : IpcService
    {
        public IReadOnlyApplicationControlDataInterface(ServiceCtx context) { }
        
        [CommandCmif(0)]
        // GetApplicationControlData(u8, u64) -> (unknown<4>, buffer<unknown, 6>)
        public ResultCode GetApplicationControlData(ServiceCtx context)
        {
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            byte source = (byte)context.RequestData.ReadInt64();
            ApplicationId titleId = context.RequestData.ReadStruct<ApplicationId>();
#pragma warning restore IDE0059

            ulong position = context.Request.ReceiveBuff[0].Position;
            
            ApplicationControlProperty nacp = context.Device.Processes.ActiveApplication.ApplicationControlProperties;
            foreach (RyuApplicationData ryuApplicationData in context.Device.Configuration.Titles)
            {
                if (ryuApplicationData.AppId != titleId)
                {
                    continue;
                }

                nacp = ryuApplicationData.Nacp;
                // NOTE: this is hacky but it works and prevents a crash
                nacp.Title[1] = ryuApplicationData.Nacp.Title[0];
                if (ryuApplicationData.Icon?.Length > 0)
                {
                    context.Memory.Write(position + 0x4000, ryuApplicationData.Icon);
                    context.ResponseData.Write(0x4000 + ryuApplicationData.Icon.Length);
                }
                break;
            }

            context.Memory.Write(position, SpanHelpers.AsByteSpan(ref nacp).ToArray());

            return ResultCode.Success;
        }
        
        [CommandCmif(1)]
        // GetApplicationDesiredLanguage() -> u32
        public ResultCode GetApplicationDesiredLanguage(ServiceCtx context)
        {
            context.ResponseData.Write((uint)context.Device.Configuration.Region);
            return ResultCode.Success;
        }
    }
}

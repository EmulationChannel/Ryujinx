using LibHac.Common;
using LibHac.Ns;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Ns.Types;
using Ryujinx.HLE.HOS.SystemState;
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
        // GetApplicationDesiredLanguage(u8) -> u8
        public ResultCode GetApplicationDesiredLanguage(ServiceCtx context)
        {
            byte sourceBitmask = (byte)context.RequestData.ReadInt64();
            int desiredLangIndex = GetNacpLangEntryIndex(context.Device.System.State.DesiredSystemLanguage);
            byte filteredBitmask = (byte)(sourceBitmask & (1 << desiredLangIndex));

            if (filteredBitmask == 0)
            {
                filteredBitmask = sourceBitmask;
            }

            byte langEntryIndex = GetLangEntryIndex(filteredBitmask);

            context.ResponseData.Write(langEntryIndex);
            return ResultCode.Success;
        }

        public static byte GetLangEntryIndex(byte bitmask)
        {
            for (byte i = 0; i < 16; i++)
            {
                if ((bitmask & (1 << i)) != 0)
                {
                    return i;
                }
            }
            return 0xFF;
        }

        public static int GetNacpLangEntryIndex(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.AmericanEnglish: return 0;
                case SystemLanguage.BritishEnglish: return 1;
                case SystemLanguage.Japanese: return 2;
                case SystemLanguage.French: return 3;
                case SystemLanguage.German: return 4;
                case SystemLanguage.LatinAmericanSpanish: return 5;
                case SystemLanguage.Spanish: return 6;
                case SystemLanguage.Italian: return 7;
                case SystemLanguage.Dutch: return 8;
                case SystemLanguage.CanadianFrench: return 9;
                case SystemLanguage.Portuguese: return 10;
                case SystemLanguage.Russian: return 11;
                case SystemLanguage.Korean: return 12;
                case SystemLanguage.TraditionalChinese: return 13;
                case SystemLanguage.SimplifiedChinese: return 14;
                default: return 0;
            }
        }
    }
}

using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    class IReadOnlyApplicationRecordInterface : IpcService
    {
        [CommandCmif(2)] // 10.0.0+
        // IsDataCorruptedResult() -> bool
        public ResultCode IsDataCorrupted(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNs);
            context.ResponseData.Write(false);
            return ResultCode.Success;
        }
    }
}

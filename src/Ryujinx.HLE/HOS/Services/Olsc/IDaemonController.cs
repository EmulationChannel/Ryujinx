using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Olsc
{
    class IDaemonController : IpcService
    {
        [CommandCmif(0)]
        // GetApplicationAutoTransferSetting()
        public ResultCode GetApplicationAutoTransferSetting(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceOlsc);
            return ResultCode.Success;
        }
        
        [CommandCmif(2)]
        // GetGlobalAutoUploadSetting()
        public ResultCode GetGlobalAutoUploadSetting(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceOlsc);
            return ResultCode.Success;
        }
        
        [CommandCmif(5)] // [11.0.0+]
        // GetGlobalAutoDownloadSetting()
        public ResultCode GetGlobalAutoUploadOrDownloadSetting(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceOlsc);
            return ResultCode.Success;
        }
        
        [CommandCmif(12)]
        // GetAutonomyTaskStatus()
        public ResultCode GetAutonomyTaskStatus(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceOlsc);
            return ResultCode.Success;
        }
    }
}

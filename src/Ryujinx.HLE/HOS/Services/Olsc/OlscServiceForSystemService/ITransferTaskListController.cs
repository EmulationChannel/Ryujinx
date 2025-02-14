using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Olsc.OlscServiceForSystemService
{
    class ITransferTaskListController : IpcService
    {
        public ITransferTaskListController(ServiceCtx context) { }

        [CommandCmif(5)]
        // GetNativeHandleHolder() -> object<nn::olsc::srv::INativeHandleHolder>
        public ResultCode GetNativeHandleHolder(ServiceCtx context)
        {
            MakeObject(context, new INativeHandleHolder(context));

            Logger.Stub?.PrintStub(LogClass.ServiceOlsc);

            return ResultCode.Success;
        }
        
        [CommandCmif(8)]
        // StopNextTransferTaskExecution()
        public ResultCode StopNextTransferTaskExecution(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceOlsc);
            return ResultCode.Success;
        }

        [CommandCmif(9)]
        // GetNativeHandleHolderEx() -> object<nn::olsc::srv::INativeHandleHolder>
        public ResultCode GetNativeHandleHolderEx(ServiceCtx context)
        {
            MakeObject(context, new INativeHandleHolder(context));

            Logger.Stub?.PrintStub(LogClass.ServiceOlsc);

            return ResultCode.Success;
        }
        
        [CommandCmif(24)]
        // GetCurrentTransferTaskInfo()
        public ResultCode GetCurrentTransferTaskInfo(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceOlsc);
            return ResultCode.Success;
        }
        
        [CommandCmif(25)]
        // FindTransferTaskInfo()
        public ResultCode FindTransferTaskInfo(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceOlsc);
            return ResultCode.Success;
        }
    }
}

namespace Ryujinx.HLE.HOS.Services.Olsc
{
    class ITransferTaskListController : IpcService
    {
        public ITransferTaskListController() { }
        
        [CommandCmif(5)]
        // GetTransferTaskEndEventNativeHandleHolder() -> nn::olsc::INativeHandleHolder
        public ResultCode GetTransferTaskEndEventNativeHandleHolder(ServiceCtx context)
        {
            MakeObject(context, new INativeHandleHolder(context));

            return ResultCode.Success;
        }
        
        [CommandCmif(9)]
        // GetTransferTaskStartEventNativeHandleHolder() -> nn::olsc::INativeHandleHolder
        public ResultCode GetTransferTaskStartEventNativeHandleHolder(ServiceCtx context)
        {
            MakeObject(context, new INativeHandleHolder(context));

            return ResultCode.Success;
        }
    }
}

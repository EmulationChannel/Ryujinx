namespace Ryujinx.HLE.HOS.Services.Olsc
{
    [Service("olsc:s")] // 4.0.0+
    class IOlscServiceForSystemService : IpcService
    {
        public IOlscServiceForSystemService(ServiceCtx context) { }
        
        [CommandCmif(10000)]
        // GetOlscServiceForSystemService() -> object<nn::olsc::IOlscServiceForSystemService>
        public ResultCode GetOlscServiceForSystemService(ServiceCtx context)
        {
            MakeObject(context, new IOlscServiceForSystemService(context));

            return ResultCode.Success;
        }
        
        [CommandCmif(0)]
        // GetTransferTaskListController() -> object<nn::olsc::ITransferTaskListController>
        public ResultCode GetTransferTaskListController(ServiceCtx context)
        {
            MakeObject(context, new ITransferTaskListController());

            return ResultCode.Success;
        }
    }
}

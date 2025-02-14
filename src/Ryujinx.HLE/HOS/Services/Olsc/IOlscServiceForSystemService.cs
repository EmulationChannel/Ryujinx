using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy;
using Ryujinx.HLE.HOS.Services.Olsc.OlscServiceForSystemService;

namespace Ryujinx.HLE.HOS.Services.Olsc
{
    [Service("olsc:s")] // 4.0.0+
    class IOlscServiceForSystemService : IpcService
    {
        public IOlscServiceForSystemService(ServiceCtx context) { }
        
        [CommandCmif(0)]
        // GetTransferTaskListController() -> object<nn::olsc::ITransferTaskListController>
        public ResultCode GetTransferTaskListController(ServiceCtx context)
        {
            MakeObject(context, new ITransferTaskListController(context));

            return ResultCode.Success;
        }
        
        [CommandCmif(1)]
        // GetRemoteStorageController() -> object<nn::olsc::IRemoteStorageController. >
        public ResultCode GetRemoteStorageController(ServiceCtx context)
        {
            MakeObject(context, new IRemoteStorageController());

            return ResultCode.Success;
        }
        
        [CommandCmif(2)]
        // GetDaemonController() -> object<nn::olsc::IDaemonController>
        public ResultCode GetDaemonController(ServiceCtx context)
        {
            MakeObject(context, new IDaemonController());

            return ResultCode.Success;
        }
        
        [CommandCmif(200)]
        // GetDataTransferPolicy(u64) -> (u8, u8)
        public ResultCode GetDataTransferPolicy(ServiceCtx context)
        {
            context.ResponseData.Write(0);
            context.ResponseData.Write(0);
            return ResultCode.Success;
        }
        
        [CommandCmif(10000)]
        // GetOlscServiceForSystemService() -> object<nn::olsc::IOlscServiceForSystemService>
        public ResultCode GetOlscServiceForSystemService(ServiceCtx context)
        {
            MakeObject(context, new IOlscServiceForSystemService(context));

            return ResultCode.Success;
        }
    }
}

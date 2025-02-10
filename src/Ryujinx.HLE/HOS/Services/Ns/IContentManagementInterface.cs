namespace Ryujinx.HLE.HOS.Services.Ns
{
    class IContentManagementInterface : IpcService
    {
        public IContentManagementInterface(ServiceCtx context) { }
        
        [CommandCmif(43)]
        // CheckSdCardMountStatus()
        public ResultCode CheckSdCardMountStatus(ServiceCtx context)
        {
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
        
    }
}

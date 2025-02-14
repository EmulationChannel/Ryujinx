namespace Ryujinx.HLE.HOS.Services.Ns
{
    [Service("ns:am2")]
    [Service("ns:ec")]
    [Service("ns:rid")]
    [Service("ns:rt")]
    [Service("ns:web")]
    class IServiceGetterInterface : IpcService
    {
        public IServiceGetterInterface(ServiceCtx context) { }
        

        [CommandCmif(7988)]
        // GetDynamicRightsInterface() -> object<nn::ns::detail::GetDynamicRightsInterface>
        public ResultCode GetDynamicRightsInterface(ServiceCtx context)
        {
            MakeObject(context, new IDynamicRightsInterface());

            return ResultCode.Success;
        }
        
        [CommandCmif(7989)]
        // GetReadOnlyApplicationControlDataInterface() -> object<nn::ns::detail::IReadOnlyApplicationControlDataInterface>
        public ResultCode GetReadOnlyApplicationControlDataInterface(ServiceCtx context)
        {
            MakeObject(context, new IReadOnlyApplicationControlDataInterface(context));

            return ResultCode.Success;
        }
        
        [CommandCmif(7991)] // 5.0.0+
        //  GetReadOnlyApplicationRecordInterface() -> object<nn::ns::detail::IReadOnlyApplicationRecordInterface>
        public ResultCode GetReadOnlyApplicationRecordInterface(ServiceCtx context)
        {
            MakeObject(context, new IReadOnlyApplicationRecordInterface());
            
            return ResultCode.Success;
        }
        
        [CommandCmif(7996)]
        // GetApplicationManagerInterface() -> object<nn::ns::detail::IApplicationManagerInterface>
        public ResultCode GetApplicationManagerInterface(ServiceCtx context)
        {
            MakeObject(context, new IApplicationManagerInterface(context));

            return ResultCode.Success;
        }
        
        [CommandCmif(7997)]
        // GetDownloadTaskInterface() -> object<nn::ns::detail::IDownloadTaskInterface>
        public ResultCode GetDownloadTaskInterface(ServiceCtx context)
        {
            MakeObject(context, new IDownloadTaskInterface(context));

            return ResultCode.Success;
        }
        
        [CommandCmif(7998)]
        // GetContentManagementInterface() -> object<nn::ns::detail::IContentManagementInterface>
        public ResultCode GetContentManagementInterface(ServiceCtx context)
        {
            MakeObject(context, new IContentManagementInterface(context));

            return ResultCode.Success;
        }
        
        [CommandCmif(7999)]
        // GetDocumentInterface() -> object<nn::ns::detail::IDocumentInterface>
        public ResultCode GetDocumentInterface(ServiceCtx context)
        {
            MakeObject(context, new IDocumentInterface());
            
            return ResultCode.Success;
        }
    }
}

using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Ldn
{
    [Service("ldn:m")]
    class IMonitorServiceCreator : IpcService
    {
        public IMonitorServiceCreator(ServiceCtx context) { }
        
        [CommandCmif(0)]
        // CreateMonitorService() -> object<nn::ldn::detail::IMonitorService>
        public ResultCode CreateMonitorService(ServiceCtx context)
        {
            MakeObject(context, new IMonitorService());

            return ResultCode.Success;
        }
    }
}

namespace Ryujinx.HLE.HOS.Services.Ldn.Lp2p
{
    [Service("lp2p:m")]
    class ISfMonitorServiceCreator : IpcService
    {
        public ISfMonitorServiceCreator(ServiceCtx context) { }
        
        [CommandCmif(0)]
        // CreateMonitorService() -> object<nn::lp2p::sf::IMonitorService>
        public ResultCode CreateMonitorService(ServiceCtx context)
        {
            MakeObject(context, new IMonitorService());

            return ResultCode.Success;
        }
    }
}

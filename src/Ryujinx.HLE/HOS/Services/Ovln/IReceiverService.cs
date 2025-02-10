namespace Ryujinx.HLE.HOS.Services.Ovln
{
    [Service("ovln:rcv")]
    class IReceiverService : IpcService
    {
        public IReceiverService(ServiceCtx context) { }
        [CommandCmif(0)]
        // OpenReceiver() -> object<nn::ovln::sf::IReceiver>
        public ResultCode OpenReceiver(ServiceCtx context)
        {
            MakeObject(context, new IReceiver(context));
            return ResultCode.Success;
        }
    }
}

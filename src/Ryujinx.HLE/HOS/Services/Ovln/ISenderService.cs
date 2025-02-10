namespace Ryujinx.HLE.HOS.Services.Ovln
{
    [Service("ovln:snd")]
    class ISenderService : IpcService
    {
        public ISenderService(ServiceCtx context) { }
        [CommandCmif(0)]
        // OpenSender() -> object<nn::ovln::sf::ISender>
        public ResultCode OpenSender(ServiceCtx context)
        {
            MakeObject(context, new ISender());
            return ResultCode.Success;
        }
    }
}

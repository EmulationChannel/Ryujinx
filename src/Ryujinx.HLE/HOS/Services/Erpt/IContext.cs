using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Erpt
{
    [Service("erpt:c")]
    class IContext : IpcService
    {
        public IContext(ServiceCtx context) { }
        
        [CommandCmif(0)]
        // SubmitContext(buffer<unknown, 5>, buffer<unknown, 5>) -> u32
        public ResultCode SubmitContext(ServiceCtx context)
        {
            Logger.Info?.PrintStub(LogClass.Service, $"ContextEntry size: {context.Request.SendBuff[0].Size}");
            Logger.Info?.PrintStub(LogClass.Service, $"FieldList size: {context.Request.SendBuff[1].Size}");
            context.ResponseData.Write(0);
            return ResultCode.Success;
        }
    }
}

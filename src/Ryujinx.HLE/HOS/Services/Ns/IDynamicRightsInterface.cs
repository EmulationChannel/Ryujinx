using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    class IDynamicRightsInterface : IpcService
    {
        [CommandCmif(5)]
        // VerifyActivatedRightsOwners(u64)
        public ResultCode VerifyActivatedRightsOwners(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNs);
            return ResultCode.Success;
        }


        [CommandCmif(13)]
        // GetRunningApplicationStatus() -> nn::ns::RunningApplicationStatus
        public ResultCode GetRunningApplicationStatus(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNs);
            context.ResponseData.Write(0);
            return ResultCode.Success;
        }

        [CommandCmif(18)]
        // NotifyApplicationRightsCheckStart()
        public ResultCode NotifyApplicationRightsCheckStart(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNs);
            return ResultCode.Success;
        }
    }
}

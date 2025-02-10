using Ryujinx.Common.Logging;
using System;

namespace Ryujinx.HLE.HOS.Services.News
{
    class INewsDatabaseService : IpcService
    {
        public INewsDatabaseService(ServiceCtx context) { }

        [CommandCmif(0)]
        // GetListV1(unknown<4>, buffer<unknown, 9>, buffer<unknown, 9>) -> (unknown<4>, buffer<unknown, 6>)
        public ResultCode GetListV1(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.Service);
            return ResultCode.Success;
        }
        
        [CommandCmif(1)]
        // Count(buffer<nn::news::detail::Count>) -> u32
        public ResultCode Count(ServiceCtx context)
        {
            // TODO: Implement news database count
            context.ResponseData.Write(0);
            Logger.Stub?.PrintStub(LogClass.Service);
            return ResultCode.Success;
        }
        
        [CommandCmif(3)]
        // UpdateIntegerValue(unknown<4>, buffer<unknown, 9>, buffer<unknown, 9>)
        public ResultCode UpdateIntegerValue(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.Service);
            return ResultCode.Success;
        }
    }
}

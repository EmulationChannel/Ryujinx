using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.Applets;
using System.Linq;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IApplicationCreator : IpcService
    {
        private readonly ulong _pid;

        public IApplicationCreator(ulong pid)
        {
            _pid = pid;
        }
        
        [CommandCmif(0)]
        // CreateApplication(nn::ncm::ApplicationId) -> object<nn::am::service::IApplicationAccessor>
        public ResultCode CreateApplication(ServiceCtx context)
        {
            ulong applicationId = context.RequestData.ReadUInt64();
            Horizon system = context.Device.System;
            string contentPath = system.Device.Configuration.Titles.FirstOrDefault(t => t.AppId.Value == applicationId).Path;
            
            MakeObject(context, new IApplicationAccessor(_pid, applicationId, contentPath, context.Device.System));
            return ResultCode.Success;
        }
        
        [CommandCmif(10)]
        // CreateSystemApplication(nn::ncm::SystemApplicationId) -> object<nn::am::service::IApplicationAccessor>
        public ResultCode CreateSystemApplication(ServiceCtx context)
        {
            var applicationId = context.RequestData.ReadUInt64();
            var system = context.Device.System;
            string contentPath = system.ContentManager.GetInstalledContentPath(applicationId, StorageId.BuiltInSystem, NcaContentType.Program);

            if (contentPath.Length == 0)
            {
                return ResultCode.TitleIdNotFound;
            }

            if (contentPath.StartsWith("@SystemContent"))
            {
                contentPath = VirtualFileSystem.SwitchPathToSystemPath(contentPath);
            }

            MakeObject(context, new IApplicationAccessor(_pid, applicationId, contentPath, context.Device.System));

            return ResultCode.Success;
        }
    }
}

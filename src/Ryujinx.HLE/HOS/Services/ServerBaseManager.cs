using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Services.Sm;
using Ryujinx.Horizon;

namespace Ryujinx.HLE.HOS.Services
{
    public class ServerBaseManager
    {
        internal SmRegistry SmRegistry { get; set; }
        internal ServerBase SmServer { get; set; }
        internal ServerBase BsdServer { get; set; }
        internal ServerBase FsServer { get; set; }
        internal ServerBase HidServer { get; set; }
        internal ServerBase NvDrvServer { get; set; }
        internal ServerBase TimeServer { get; set; }
        internal ServerBase ViServer { get; set; }
        internal ServerBase ViServerM { get; set; }
        internal ServerBase ViServerS { get; set; }
        internal ServerBase LdnServer { get; set; }
        internal ServiceTable ServiceTable { get; set; }
        internal LibHacHorizonManager LibHacHorizonManager { get; set; }

        public ServerBaseManager Clone()
        {
            ServerBaseManager clone = new ServerBaseManager();
            clone.LibHacHorizonManager = this.LibHacHorizonManager.Clone();
            clone.ServiceTable = this.ServiceTable.Clone();
            clone.LdnServer = this.LdnServer;
            clone.ViServer = this.ViServer;
            clone.ViServerM = this.ViServerM;
            clone.ViServerS = this.ViServerS;
            clone.TimeServer = this.TimeServer;
            clone.NvDrvServer = this.NvDrvServer;
            clone.HidServer = this.HidServer;
            clone.FsServer = this.FsServer;
            clone.BsdServer = this.BsdServer;
            clone.SmServer = this.SmServer;
            clone.SmRegistry = this.SmRegistry;
            return clone;
        }
    }
}

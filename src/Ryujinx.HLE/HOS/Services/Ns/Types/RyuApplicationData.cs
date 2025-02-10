using LibHac;
using LibHac.Ns;

namespace Ryujinx.HLE.HOS.Services.Ns.Types
{
    // Used to store some internal data about applications.
    public struct RyuApplicationData
    {
        public ApplicationId AppId;
        public ApplicationControlProperty Nacp;
        public string Path;
        public byte[] Icon;

        public RyuApplicationData(ApplicationId appId, ApplicationControlProperty nacp, string path, byte[] icon)
        {
            AppId = appId;
            Nacp = nacp;
            Path = path;
            Icon = icon;
        }
    }
}

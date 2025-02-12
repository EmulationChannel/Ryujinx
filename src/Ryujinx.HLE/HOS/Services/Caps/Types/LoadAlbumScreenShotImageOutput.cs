using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Caps.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x500)]
    struct LoadAlbumScreenShotImageOutput
    {
        public long width;
        public long height;
        public ScreenShotAttribute attribute;
    }
}

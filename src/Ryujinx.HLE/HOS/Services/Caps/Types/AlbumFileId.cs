using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Caps.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct AlbumFileId
    {
        public ulong ApplicationId;
        public AlbumFileDateTime Time;
        public byte Storage;
        public byte Contents;
        public byte Field19_0;
        public byte Field19_1;
        public uint Reserved;
    }
}

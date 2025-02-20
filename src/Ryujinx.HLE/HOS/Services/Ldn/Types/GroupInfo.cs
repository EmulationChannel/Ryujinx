using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ldn.Types
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct GroupInfo
    {
        public fixed byte info[0x200]; // Fixed size buffer
    }
}

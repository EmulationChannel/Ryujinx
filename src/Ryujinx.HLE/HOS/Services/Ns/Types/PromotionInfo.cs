using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ns.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PromotionInfo
    {
        [MarshalAs(UnmanagedType.I8)]
        public long StartTimestamp;

        [MarshalAs(UnmanagedType.I8)]
        public long EndTimestamp;

        [MarshalAs(UnmanagedType.I8)]
        public long RemainingTime;

        [MarshalAs(UnmanagedType.U4)]
        public uint Reserved;

        [MarshalAs(UnmanagedType.U1)]
        public byte Flags;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] Padding;
    }
}

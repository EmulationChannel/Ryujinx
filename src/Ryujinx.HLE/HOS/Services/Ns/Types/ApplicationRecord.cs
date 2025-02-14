using LibHac;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ns.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ApplicationRecord
    {
        public ApplicationId AppId;
        public ApplicationRecordType Type;
        public byte Unknown;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] Padding1;

        public byte Unknown2;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public byte[] Padding2;
    }
}

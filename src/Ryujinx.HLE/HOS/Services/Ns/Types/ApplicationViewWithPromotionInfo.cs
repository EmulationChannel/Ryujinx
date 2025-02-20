using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ns.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x70)]
    struct ApplicationViewWithPromotionInfo
    {
        public ApplicationView AppView;
        public PromotionInfo Promotion;
    }
}

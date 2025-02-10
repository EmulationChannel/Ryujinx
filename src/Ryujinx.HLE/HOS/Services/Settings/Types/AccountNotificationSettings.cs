using Ryujinx.Common.Memory;

namespace Ryujinx.HLE.HOS.Services.Settings.Types
{
    struct AccountNotificationSettings
    {
        public Array16<byte> Uid;                             // 0x0 - 0x10: Unique ID (16 bytes)

        public uint Flags;                             // 0x10 - 0x4: Notification Flags

        public byte FriendPresenceOverlayPermission;   // 0x14 - 0x1: Friend presence overlay permission

        public byte FriendInvitationOverlayPermission; // 0x15 - 0x1: Friend invitation overlay permission
            
        public byte Reserved1;                        // 0x16 - 0x2: Reserved bytes
        public byte Reserved2;                        // 0x16 - 0x2: Reserved bytes
    }

}

namespace Ryujinx.HLE.HOS.Services.Caps.Types
{
    struct ApplicationAlbumFileEntry
    {
        public ApplicationAlbumEntry ApplicationAlbumEntry;
        public AlbumFileDateTime Date;
        public Common.Memory.Array8<byte> Unknown;
    }
}

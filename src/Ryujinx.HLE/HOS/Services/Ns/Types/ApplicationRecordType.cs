namespace Ryujinx.HLE.HOS.Services.Ns.Types
{
    enum ApplicationRecordType : byte
    {
        Installing = 2,
        Installed = 3,
        GameCardNotInserted = 5,
        Archived = 11,
        GameCard = 16,
    }
}

namespace Ryujinx.Horizon.Sdk.Sf.Hipc
{
    readonly struct ManagerOptions
    {
        public static ManagerOptions Default => new(0, 0, 0, false);

        public int PointerBufferSize { get; }
        public int MaxDomains { get; }
        public int MaxDomainObjects { get; }
        public bool CanDeferInvokeRequest { get; }

        public ManagerOptions(int pointerBufferSize, int maxDomains, int maxDomainObjects, bool canDeferInvokeRequest)
        {
            PointerBufferSize = pointerBufferSize;
            MaxDomains = maxDomains;
            // TODO: Implement domain objects removal before enabling this.
            // MaxDomainObjects = maxDomainObjects;
            MaxDomainObjects = 256;
            CanDeferInvokeRequest = canDeferInvokeRequest;
        }
    }
}

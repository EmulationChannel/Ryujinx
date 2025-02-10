using Ryujinx.HLE.HOS.Services.BluetoothManager.BtmSystem;

namespace Ryujinx.HLE.HOS.Services.BluetoothManager
{
    [Service("btm:sys")]
    class IBtmSystem : IpcService
    {
        public IBtmSystem(ServiceCtx context) { }
        
        [CommandCmif(0)]
        // GetCoreImpl() -> object<nn::btm::IBtmSystemCore>
        public ResultCode GetCoreImpl(ServiceCtx context)
        {
            MakeObject(context, new IBtmSystemCore());

            return ResultCode.Success;
        }
        
        [CommandCmif(6)]
        // IsRadioEnabled() -> b8
        public ResultCode IsRadioEnabled(ServiceCtx context)
        {
            context.ResponseData.Write(true);

            return ResultCode.Success;
        }
    }
}

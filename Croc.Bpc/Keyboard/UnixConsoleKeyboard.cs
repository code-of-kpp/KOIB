using System; 
using System.Runtime.InteropServices; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Keyboard.Config; 
using Croc.Core.Diagnostics; 
namespace Croc.Bpc.Keyboard 
{ 
    public class UnixConsoleKeyboard : BaseKeyboardDriver 
    { 
        private const string DRIVER_KEYBOARD_TYPE = "KeyboardType"; 
        private const int DATA_READY = 1; 
        public override void Init(KeyboardDriverConfig config, ILogger logger) 
        { 
            base.Init(config, logger); 
            KeyboardType type; 
            var typeStr = config.Settings[DRIVER_KEYBOARD_TYPE]; 
            if (typeStr != null) 
                type = (KeyboardType)Enum.Parse(typeof(KeyboardType), typeStr.Value, true); 
            else 
                type = KeyboardType.Default; 
            _logger.LogInfo(Message.KeyboardType, type, (int)type); 
            KeyboardProvider((int)type); 
        } 
        protected override void ReadKey() 
        { 
            byte scanCode = 0; 
            var timeStamp = 0; 
            if (getAll(ref scanCode, ref timeStamp) == DATA_READY && !_disposed) 
                RaiseKeyPressed(new DriverKeyPressedEventArgs(scanCode, timeStamp)); 
        } 
        protected override void FreeDevice() 
        { 
            closeAll(); 
        } 
        #region KeyboardProvider extern methods 
        [DllImport("KeyboardProvider.dll")] 
        private static extern void KeyboardProvider(int keyboardType); 
        [DllImport("KeyboardProvider.dll")] 
        private static extern void closeAll(); 
        [DllImport("KeyboardProvider.dll")] 
        private static extern int getAll(ref byte scanCode, ref int time); 
        #endregion 
    } 
}

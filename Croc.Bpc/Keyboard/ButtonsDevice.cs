using System; 
using System.Runtime.InteropServices; 
namespace Croc.Bpc.Keyboard 
{ 
    public class ButtonsDevice : BaseKeyboardDriver 
    { 
        [Flags] 
        private enum Buttons : byte 
        { 
            PowerOff = 0x1, 
            No = 0x2, 
            Yes = 0x4, 
            Help = 0x8, 
        } 
        private static readonly Buttons[] s_buttons = (Buttons[])Enum.GetValues(typeof(Buttons)); 
        private const int DATA_READY = 1; 
        protected override void ReadKey() 
        { 
            var timeStamp = 0; 
            byte mask = 0; 
            if (GetButtons(ref mask, ref timeStamp) == DATA_READY && !_disposed) 
            { 
                foreach (byte button in s_buttons) 
                { 
                    if ((mask & button) != 0) 
                    { 
                        RaiseKeyPressed(new DriverKeyPressedEventArgs(button, timeStamp)); 
                    } 
                } 
            } 
        } 
        #region ButtonsProvider extern methods 
        [DllImport("ButtonsProvider.dll")] 
        public static extern int GetButtons(ref byte mask, ref int time); 
        #endregion 
    } 
}

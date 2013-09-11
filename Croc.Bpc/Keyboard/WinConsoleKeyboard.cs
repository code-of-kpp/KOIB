using System; 
namespace Croc.Bpc.Keyboard 
{ 
    public class WinConsoleKeyboard : BaseKeyboardDriver 
    { 
        protected override void ReadKey() 
        { 
            var scanCode = (int)Console.ReadKey(true).Key; 
            var timeStamp = (int)(DateTime.Now.Ticks / 10000); 
            RaiseKeyPressed(new DriverKeyPressedEventArgs(scanCode, timeStamp)); 
        } 
    } 
}

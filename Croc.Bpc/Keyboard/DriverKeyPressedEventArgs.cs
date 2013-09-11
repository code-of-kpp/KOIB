using System; 
namespace Croc.Bpc.Keyboard 
{ 
    public class DriverKeyPressedEventArgs : EventArgs 
    { 
        public readonly int ScanCode; 
        public readonly int TimeStamp; 
        public DriverKeyPressedEventArgs(int scanCode, int timeStamp) 
        { 
            ScanCode = scanCode; 
            TimeStamp = timeStamp; 
        } 
    } 
}

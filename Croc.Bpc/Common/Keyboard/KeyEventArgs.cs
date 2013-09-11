using System; 
namespace Croc.Bpc.Keyboard 
{ 
    public class KeyEventArgs : EventArgs 
    { 
        public readonly int ScanCode; 
        public readonly KeyType Type; 
        public readonly int Value; 
        public readonly int TimeStamp; 
        public KeyEventArgs(int scanCode, KeyType type, int value, int timeStamp) 
        { 
            ScanCode = scanCode; 
            Type = type; 
            Value = value; 
            TimeStamp = timeStamp; 
        } 
        public new static KeyEventArgs Empty 
        { 
            get 
            { 
                return new KeyEventArgs(0, KeyType.Unknown, 0, 0); 
            } 
        } 
    } 
}

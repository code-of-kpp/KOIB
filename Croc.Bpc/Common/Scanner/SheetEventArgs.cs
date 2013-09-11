using System; 
namespace Croc.Bpc.Scanner 
{ 
    public class SheetEventArgs : EventArgs 
    { 
        public readonly SheetProcessingSession SheetProcessingSession; 
        public SheetEventArgs(SheetProcessingSession session) 
        { 
            SheetProcessingSession = session; 
        } 
    } 
}

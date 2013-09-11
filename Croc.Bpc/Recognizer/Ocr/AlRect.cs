using System; 
using System.Runtime.InteropServices; 
namespace Croc.Bpc.Recognizer.Ocr 
{ 
    [StructLayout(LayoutKind.Sequential)] 
    public struct AlRect     
    { 
        public int  x, y, w, h;            
    }; 
}

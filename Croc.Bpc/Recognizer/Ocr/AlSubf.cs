using System; 
using System.Runtime.InteropServices; 
namespace Croc.Bpc.Recognizer.Ocr 
{ 
    [StructLayout(LayoutKind.Sequential)] 
    public struct AlSubf    
    { 
        public IntPtr    @base;       
        public int       width;       
        public int       lbit; 
        public int       xs;   
        public int       ys;   
        public int       x;    
        public int       y;           
    }; 
}

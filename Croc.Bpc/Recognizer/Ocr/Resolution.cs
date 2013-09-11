using System.Runtime.InteropServices; 
namespace Croc.Bpc.Recognizer.Ocr 
{ 
    [StructLayout(LayoutKind.Sequential)] 
    struct Resolution 
    { 
        public int side; 
        public float x, y; 
    }; 
}

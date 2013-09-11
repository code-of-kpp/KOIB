using System; 
using System.Runtime.InteropServices; 
namespace Croc.Bpc.Recognizer.Ocr 
{ 
    [StructLayout(LayoutKind.Sequential)] 
    struct    OcrResult 
    { 
        public int        PollNum;     
        public int        IsValid;     
        public int        numChecked;     
        public IntPtr    sqData;         
        public IntPtr    piDigits;     
    }; 
}

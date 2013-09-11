using System; 
using System.Runtime.InteropServices; 
namespace Croc.Bpc.Recognizer.Ocr 
{ 
    [StructLayout(LayoutKind.Sequential)] 
    public struct BufferHeader 
    { 
        public uint     size;              
        public uint     bin_size;          
        public uint     tone_size;          
        public IntPtr   all_bin_buf;          
        public IntPtr      bin_buf;          
        public IntPtr      tone_buf;          
        public int         ImageWidth;          
        public int         ByteImageWidth;  
        public int         bin_width;     
        public GrayParam g_p0;      
        public GrayParam g_p1;      
        public GrayParam g_p2;          
        public int         flag;             
        public int         ActualScanNumber;    
        public int         ScanCounter;         
        public int         OutFlag;             
        public int         LineCount;           
        public int         xl,yl,xr,yr;         
    } 
    [StructLayout(LayoutKind.Sequential)] 
    public struct GrayParam 
    { 
        public int start; 
        public int len; 
    }; 
}

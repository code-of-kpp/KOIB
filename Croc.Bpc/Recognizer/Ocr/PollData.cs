using System; 
using System.Runtime.InteropServices; 
namespace Croc.Bpc.Recognizer.Ocr 
{ 
    [StructLayout(LayoutKind.Sequential)] 
    public struct PollData 
    { 
        public int polltype; 
        public int totalNum; 
        public int MinValid; 
        public int MaxValid; 
    }; 
}

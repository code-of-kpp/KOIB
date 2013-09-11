using System; 
using Croc.Bpc.Utils; 
namespace Croc.Bpc.Recognizer.Ocr 
{ 
    public interface IOcrEventHandler 
    { 
        int GetHalfToneBuffer(IOcr ocr, short side, int x, int y, int height, int width, MemoryBlock image); 
        int GetBinaryThreshold(IOcr ocr, short side); 
        void Error(IOcr ocr, int errorCode, string message); 
        void AppendToLog(IOcr ocr, string message); 
    } 
}

using System; 
namespace Croc.Bpc.Recognizer.Ocr 
{ 
    public enum OcrRecognitionResult 
    { 
        OK = 0, 
        NUF = -1, 
        BRK = -2, 
        MARK = -3, 
        SKEW = -4, 
        REFP = -5, 
        FSQR = -6, 
        CLRTOP = -7, 
        CLRBOT = -8, 
        CALL = -9, 
        SCTLINE = -10, 
        BULNUM = -11, 
        ERROR = -100 
    } 
}

using System; 
namespace Croc.Bpc.Recognizer.Ocr 
{ 
    public enum StampResult 
    { 
        YES = 1, 
        CALL_ERROR = -1,         
        EMPTY = -2,             
        FAINT    = -3,             
        BADLINES = -4,         
        BADPRINT = -5,         
    }; 
}

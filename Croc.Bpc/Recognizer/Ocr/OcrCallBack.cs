using System; 
namespace Croc.Bpc.Recognizer.Ocr 
{ 
    public delegate int OcrCallBack(OcrCallBackType type, IntPtr data, int size); 
    public enum OcrCallBackType 
    { 
        ModelSave = 1, 
        ModelRestore, 
        DataSave, 
        DataRestore, 
        GetStamp, 
        GetStampCount, 
        GetStampMinLineWidth, 
        GetStampMaxLineWidth, 
        GetStampTestLevel, 
        GetInlineRecognitionLevel, 
        GetSideResolution, 
        GetExcludedSquares, 
        PutBulletinNumber, 
        PutResults, 
        SaveImage, 
        ShowImage, 
        GetBinThreshold, 
        GetModelFileSize, 
        ReportProgress, 
        GetGrayRectBuffSize, 
        GetGrayRectImage, 
        GetPath2Data, 
        GetDigitSquares, 
        UnloadDigitOcrResult, 
    }; 
}

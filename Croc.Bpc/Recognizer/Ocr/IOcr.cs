using System; 
using System.Collections.Generic; 
using Croc.Bpc.Utils; 
namespace Croc.Bpc.Recognizer.Ocr 
{ 
    public interface IOcr : IDisposable 
    { 
        void Init(); 
        void SetEventsHandler(IOcrEventHandler eventHandler); 
        StampTestLevel StampTestLevel { get; set; } 
        OnlineLevel OnlineRecognitionLevel { get; set; } 
        int MaxOnlineSkew { get; set; } 
        float DpiX0 { get; set; } 
        float DpiX1 { get; set; } 
        float DpiY0 { get; set; } 
        float DpiY1 { get; set; } 
        void SetDpi(float x0, float y0, float x1, float y1); 
        int BulletinNumber { get; } 
        StampResult StampResult { get; } 
        List<PollResult> Results { get; } 
        void OCR_ExcludeSquare(int bulletin, int election, int square); 
        void OCR_RestoreSquare(int bulletin, int election, int square); 
        int OCR_IsSquareValid(int bulletin, int election, int square, out int retVal); 
        void RunRecognize(MemoryBlock pdImage0, MemoryBlock pdImage1, int nLineWidth0, int nLineWidth1, int nNumOfLines0, int nNumOfLines1); 
        int EndRecognize(MarkerType markerType); 
        int TestBallot(ref GeoData geoData); 
        void LinkModel(); 
        int NextBuffer(int count); 
        int GetOnlineMarker(MarkerType markerType); 
        void InitRecognize(); 
        void AddStamp(int nStamp); 
        void ClearStamps(); 
        string ModelFilePath { get; set; } 
        string Path2RecognitionData { get; set; } 
        int MinMarkerWid { get; set; } 
        int MaxMarkerWid { get; set; } 
        int MinMarkerHgh { get; set; } 
        int MaxMarkerHgh { get; set; } 
        double MinMarkerRio { get; set; } 
        double MaxMarkerRio { get; set; } 
        int BlankTestStart { get; set; } 
        int BlankTestStop { get; set; } 
        int MinCheckArea { get; set; } 
        int StampLowThr { get; set; } 
        int StampDigitXsize { get; set; } 
        int StampDigitYsize { get; set; } 
        int StampDigitMinLineWidth { get; set; } 
        int StampDigitMaxLineWidth { get; set; } 
        int StampDigitGap { get; set; } 
        int StampDigitDistBotLine { get; set; } 
        int StampDigitDistLftLine { get; set; } 
        int StampDigitDistRghLine { get; set; } 
        int StampFrameWidth { get; set; } 
        bool CutWeakCheck { get; set; } 
        bool SeekBottomRightLine { get; set; } 
        int StampVSize { get; set; } 
        bool LookForLostSquare { get; set; } 
        int RunRecCount { get; } 
        void EnableLogging(string sLogFileName); 
        int MinStandartMarkerWid { get; set; } 
        int MaxStandartMarkerWid { get; set; } 
        int MinStandartMarkerHgh { get; set; } 
        int MaxStandartMarkerHgh { get; set; } 
        int StandartMarkerZone { get; set; } 
        int OffsetFirstRule { get; set; } 
    } 
}

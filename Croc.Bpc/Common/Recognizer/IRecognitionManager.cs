using Croc.Bpc.Scanner; 
using Croc.Bpc.Voting; 
using Croc.Core; 
namespace Croc.Bpc.Recognizer 
{ 
    public interface IRecognitionManager : ISubsystem 
    { 
        void CheckCreateModel(SourceData sourceData); 
        void InitRecognition(); 
        void RunRecognition(int lineWidth); 
        bool ProcessNextBuffer(short linesCount, out int blankMarker); 
        void ResetRecognition(); 
        void EndRecognition(short linesCount, SheetType sheetType); 
        void SaveLastImageOnDriverError(int errorCode); 
        bool NeedSaveImageOnDriverReverse { get; } 
        bool StampControlEnabled{ get; set; } 
        BlankMarking GetBlankMarking(BlankType blankType); 
        void SetBlankMarking(BlankType blankType, BlankMarking marking); 
    } 
}

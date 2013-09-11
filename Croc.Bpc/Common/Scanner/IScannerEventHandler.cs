using System; 
using Croc.Bpc.Voting; 
namespace Croc.Bpc.Scanner 
{ 
    public interface IScannerEventHandler 
    { 
        void BufferIsReady(IScanner scanner, short bufferId); 
        void DebugMessage(IScanner scanner, string message, int messageLength); 
        void Error(IScanner scanner, ScannerError error); 
        void NewSheet(IScanner scanner); 
        void NextBuffer(IScanner scanner, short linesCount); 
        void SheetDroped(IScanner scanner, BlankMarking marking, DropResult result); 
        void SheetIsReady(IScanner scanner, short linesCount, SheetType sheetType); 
        void ReadyToScanning(IScanner scanner); 
        void PowerStatistics(IScanner scanner, bool powerFailure, uint min, uint max, uint avg); 
    } 
}

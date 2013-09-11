using System; 
using Croc.Bpc.Utils; 
using Croc.Bpc.Utils.Images; 
using Croc.Bpc.Voting; 
namespace Croc.Bpc.Scanner 
{ 
    public interface IScanner : IDisposable 
    { 
        string IPAddress 
        { 
            get; 
        } 
        string SerialNumber 
        { 
            get; 
        } 
        int DriverVersion 
        { 
            get; 
        } 
        ScannerVersion Version 
        { 
            get; 
        } 
        int Status 
        { 
            get; 
        } 
        DropResult Drop(BlankMarking marking); 
        void GetHalftoneBuffer(ScannedSide side, short x, short y, short w, short h, MemoryBlock iMemory, out short id); 
        int IndicatorLength { get; } 
        void SetIndicator(string str); 
        int MotorCount { get; } 
        void Motor(short number, bool enable, int dir, int step); 
        int PageOffset_AddItem(int width, int maxOffset); 
        void PageOffset_ClearAll(); 
        void PageOffset_ClearItem(int itemId); 
        ReverseCommandResult Reverse(); 
        void SetEventsHandler(IScannerEventHandler pEvent); 
        void SetWorkZone(ScannedSide side, short x, short y); 
        void TestMarker(short sheetIssue); 
        int ValidLength_AddItem(int width, int minLength, int maxLength); 
        void ValidLength_ClearAll(); 
        void ValidLength_ClearItem(int itemId); 
        short BinaryThresholdTop 
        { 
            get; 
            set; 
        } 
        short BinaryThresholdBottom 
        { 
            get; 
            set; 
        } 
        short CurrentBinaryThresholdTop 
        { 
            get; 
        } 
        short CurrentBinaryThresholdBottom 
        { 
            get; 
        } 
        short DpiXTop 
        { 
            get; 
            set; 
        } 
        short DpiXBottom 
        { 
            get; 
            set; 
        } 
        short DpiYTop 
        { 
            get; 
            set; 
        } 
        short DpiYBottom 
        { 
            get; 
            set; 
        } 
        void EnableLamps(bool enable); 
        bool Marker 
        { 
            get; 
            set; 
        } 
        bool MarkerWork 
        { 
            get; 
            set; 
        } 
        short MaxSheetLength 
        { 
            get; 
            set; 
        } 
        short MinSheetLength 
        { 
            get; 
            set; 
        } 
        bool SheetScanning 
        { 
            get; 
        } 
        bool TuningEnabled 
        { 
            get; 
            set; 
        } 
        bool LengthValidationEnabled 
        { 
            get; 
            set; 
        } 
        int WhiteCoeff 
        { 
            get; 
            set; 
        } 
        MemoryBlock WorkBufferTop 
        { 
            get; 
        } 
        MemoryBlock WorkBufferBottom 
        { 
            get; 
        } 
        bool ScanningEnabled 
        { 
            get; 
            set; 
        } 
        int WorkZoneH 
        { 
            get; 
        } 
        int WorkZoneW 
        { 
            get; 
        } 
        void ScanningIndicatorMessage(string str); 
        bool DirtDetectionEnabled 
        { 
            get; 
            set; 
        } 
        bool SaveBuffer(string fileName, ImageType imageType, ScannedSide side, BufferSize bufferSize); 
        long GetBufferSize(ImageType imageType, BufferSize bufferSize); 
        bool Green 
        { 
            get; 
            set; 
        } 
        bool Red 
        { 
            get; 
            set; 
        } 
        void RestoreNormalState(); 
        bool DoubleSheetSensorEnabled 
        { 
            get; 
            set; 
        } 
        short DoubleSheetSensorCurrentValue 
        { 
            get; 
        } 
        void GetDoubleSheetSensorCurrentValue(out short l, out short r); 
        void GetDoubleSheetSensorLevel(out short l, out short r); 
        void SetDoubleSheetSensorLevel(short l, short r); 
        void SetDoubleSheetSensorCurrentSheetLevel(short nLeftSensorLevel, short nRightSensorLevel); 
        void CheckDoubleSheetSensor(out bool leftWork, out bool rightWork); 
        bool ScannerBusy 
        { 
            get; 
        } 
        int ExpectedLength 
        { 
            set; 
        } 
        void GetMarkerParameters( 
            out short on, 
            out short off, 
            out short markingTime, 
            out short rollbackTime, 
            out short downTime); 
        void SetMarkerParameters( 
            short on, 
            short off, 
            short markingTime, 
            short rollbackTime, 
            short downTime); 
    } 
}

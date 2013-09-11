using System; 
using System.Collections.Generic; 
using Croc.Bpc.Utils; 
using Croc.Bpc.Utils.Images; 
using Croc.Bpc.Voting; 
using Croc.Core; 
namespace Croc.Bpc.Scanner 
{ 
    public interface IScannerManager : ISubsystem, IQuietMode 
    { 
        #region Подключение к сканеру и диагностика 
        bool ScannerConnected { get; } 
        bool EstablishConnectionToScanner(int maxTryCount, TimeSpan delay); 
        event EventHandler<ScannerEventArgs> RemoteScannerConnected; 
        void RestartBroadcasting(); 
        List<ScannerDiagnosticsError> PerformDiagnostics(); 
        bool IsDoubleSheetSensorWork { get; } 
        #endregion 
        #region Основные атрибуты 
        string SerialNumber { get; } 
        int IntSerialNumber { get; } 
        int DriverVersion { get; } 
        ScannerVersion Version { get; } 
        string IPAddress { get; } 
        void SetBlankPaperType(int blankMarker, PaperType type); 
        Dictionary<int, PaperType> BlanksPaperType { get; } 
        #endregion 
        #region Параметры сканера 
        short DpiXTop { get; } 
        short DpiYTop { get; } 
        short DpiXBottom { get; } 
        short DpiYBottom { get; } 
        short DssLeftLevel { get; } 
        short DssRightLevel { get; } 
        short BinarizationThresholdTop { get; } 
        short BinarizationThresholdBottom { get; } 
        bool DoubleSheetSensorEnabled {get; set;} 
        void GetDoubleSheetSensorLevel(out short left, out short right); 
        void SetDoubleSheetSensorLevel(short left, short right); 
        void GetRelativePaperDensity(out short thick, out short thin); 
        void SetRelativePaperDensity(short thick, short thin); 
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
        #endregion 
        #region Сессия обработки листа 
        event EventHandler<SheetEventArgs> NewSheetReceived; 
        event EventHandler<SheetEventArgs> SheetProcessed; 
        SheetProcessingSession SheetProcessingSession { get; } 
        #endregion 
        #region Управление процессом сканирования 
        bool StartScanning(ScannerLampsRegime regime); 
        bool StopScanning(); 
        bool RestoreScanningAfterError(); 
        DropResult DropSheet(BlankMarking marking); 
        int ExpectedLength 
        { 
            set; 
        } 
        #endregion 
        #region Реверс листа 
        void ReverseSheet(int reasonCode); 
        #endregion 
        #region Cканирование и его результаты 
        MemoryBlock WorkBufferTop { get; } 
        MemoryBlock WorkBufferBottom { get; } 
        bool GetHalftoneBuffer(ScannedSide side, short xCoord, short yCoord, short width, short height, MemoryBlock image); 
        void SaveBuffer(string filePath, ImageType imageType, ScannedSide side, BufferSize bufferSize); 
        long GetBufferSize(ImageType imageType, BufferSize bufferSize); 
        #endregion 
        #region Управление индикатором 
        int IndicatorLength { get; } 
        void SetIndicator(string text); 
        #endregion 
        #region Управление лампами 
        void SetLampsRegime(ScannerLampsRegime lampsRegime); 
        #endregion 
    } 
}

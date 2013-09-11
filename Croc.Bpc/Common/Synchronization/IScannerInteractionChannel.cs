using System; 
using System.Collections.Generic; 
using System.Collections.Specialized; 
using Croc.Bpc.Printing; 
using Croc.Bpc.Voting; 
namespace Croc.Bpc.Synchronization 
{ 
    public interface IScannerInteractionChannel 
    { 
        #region Система 
        Version ApplicationVersion { get; } 
        void Ping(); 
        void SetSystemTime(DateTime utcDateTime); 
        #endregion 
        #region Исходные данные 
        ElectionDayСomming IsElectionDay { get; } 
        string SourceDataHashCode { get; } 
        bool IsSourceDataCorrect { get; } 
        #endregion 
        #region Роль сканера 
        event EventHandler ScannerRoleChanged; 
        void RaiseRemoteScannerRoleChanged(); 
        ScannerRole ScannerRole { get; } 
        #endregion 
        #region Передача данных 
        void PutData(string name, object data); 
        void NoticeAboutWaitForInitialization(); 
        void NoticeAboutExitFromMenu(); 
        byte[] GetFileContent(string filePath); 
        #endregion 
        #region Состояние 
        bool IsStateInitial { get; } 
        void ResetState(string reason); 
        void NeedSynchronizeState(List<StateItem> newStateItems); 
        void StateSynchronizationFinished(SynchronizationResult syncResult); 
        #endregion 
        #region Печать 
        bool FindPrinter(); 
        bool PrintReport(PrinterJob printerJob); 
        PrinterJob CreateReport(ReportType reportType, ListDictionary reportParameters, int copies); 
        void PrintReportStarting(); 
        void PrintReportFinished(); 
        #endregion 
        #region Сброс ПО 
        void ResetSoft(ResetSoftReason reason, bool isRemoteScannerInitiator, bool needRestartApp); 


        #endregion 
    } 
}

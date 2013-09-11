using System; 
using System.Collections.Generic; 
using System.Collections.Specialized; 
using System.Diagnostics; 
using System.Text; 
using Croc.Bpc.Printing; 
using Croc.Bpc.Voting; 
using Croc.Core; 
using Croc.Core.Diagnostics; 
namespace Croc.Bpc.Synchronization 
{ 
    public class RemoteScannerConnector : MarshalByRefObject, IScannerInteractionChannel 
    { 
        private readonly IScannerInteractionChannel _localScannerChannel; 
        private readonly ILogger _logger; 
        public delegate IScannerInteractionChannel GetChannelToLocalScannerDelegate(); 
        public static event GetChannelToLocalScannerDelegate GetChannelToLocalScannerEvent; 
        public delegate bool IsRemoteConnectionAllowDelegate(string serialNumber, string ipAddress); 
        public static event IsRemoteConnectionAllowDelegate IsRemoteConnectionAllowEvent; 
        public RemoteScannerConnector() 
        { 
            _localScannerChannel = GetChannelToLocalScanner(); 
            if (_localScannerChannel == null) 
                throw new Exception("Ошибка получения канала доступа к локальному объекту"); 
            _logger = CoreApplication.Instance.GetSubsystemOrThrow<ISynchronizationManager>().Logger; 
        } 
        private static IScannerInteractionChannel GetChannelToLocalScanner() 
        { 
            var handler = GetChannelToLocalScannerEvent; 
            return handler != null ? handler() : null; 
        } 
        private static object[] GetParamsForLogRemoteScannerCall(params object[] parameters) 
        { 
            var methodName = (new StackTrace()).GetFrame(3).GetMethod().Name; 
            var paramsString = new StringBuilder(); 
            foreach (var param in parameters) 
            { 
                paramsString.Append(param); 
                paramsString.Append(';'); 
            } 
            if (paramsString.Length > 0) 
                paramsString.Length -= 1; 
            else 
                paramsString.Append('-'); 
            paramsString.Replace(Environment.NewLine, string.Empty); 
            return new object[] {methodName, paramsString.ToString()}; 
        } 
        public bool IsRemoteConnectionAllow(string serialNumber, string ipAddress) 
        { 
            var handler = IsRemoteConnectionAllowEvent; 
            if (handler != null) 
                return handler(serialNumber, ipAddress); 
            return false; 
        } 
        #region IScannerInteractionChannel Members 
        #region Система 
        public Version ApplicationVersion 
        { 
            get 
            { 
                return _localScannerChannel.ApplicationVersion; 
            } 
        } 
        public void Ping() 
        { 
            _localScannerChannel.Ping(); 
        } 
        public void SetSystemTime(DateTime utcDateTime) 
        { 
            _localScannerChannel.SetSystemTime(utcDateTime); 
        } 
        #endregion 
        #region Исходные данные 
        public ElectionDayСomming IsElectionDay 
        { 
            get 
            { 
                return _localScannerChannel.IsElectionDay; 
            } 
        } 
        public string SourceDataHashCode 
        { 
            get 
            { 
                return _localScannerChannel.SourceDataHashCode; 
            } 
        } 
        public bool IsSourceDataCorrect 
        { 
            get 
            { 
                return _localScannerChannel.IsSourceDataCorrect; 
            } 
        } 
        #endregion 
        #region Роль сканера 
        public event EventHandler ScannerRoleChanged; 
        public void RaiseRemoteScannerRoleChanged() 
        { 
            _localScannerChannel.RaiseRemoteScannerRoleChanged(); 
        } 
        public ScannerRole ScannerRole 
        { 
            get 
            { 
                return _localScannerChannel.ScannerRole; 
            } 
        } 
        #endregion 
        #region Передача данных 
        public void PutData(string name, object data) 
        { 
            _localScannerChannel.PutData(name, data); 
        } 
        public void NoticeAboutWaitForInitialization() 
        { 
            _localScannerChannel.NoticeAboutWaitForInitialization(); 
        } 
        public void NoticeAboutExitFromMenu() 
        { 
            _localScannerChannel.NoticeAboutExitFromMenu(); 
        } 
        public byte[] GetFileContent(string filePath) 
        { 
            return _localScannerChannel.GetFileContent(filePath); 
        } 
        #endregion 
        #region Состояние 
        public bool IsStateInitial 
        { 
            get 
            { 
                return _localScannerChannel.IsStateInitial; 
            } 
        } 
        public void ResetState(string reason) 
        { 
            _localScannerChannel.ResetState(reason); 
        } 
        public void NeedSynchronizeState(List<StateItem> newStateItems) 
        { 
            _localScannerChannel.NeedSynchronizeState(newStateItems); 
        } 
        public void StateSynchronizationFinished(SynchronizationResult syncResult) 
        { 
            _localScannerChannel.StateSynchronizationFinished(syncResult); 
        } 
        #endregion 
        #region Печать 
        public bool FindPrinter() 
        { 
            return _localScannerChannel.FindPrinter(); 
        } 
        public bool PrintReport(PrinterJob printerJob) 
        { 
            return _localScannerChannel.PrintReport(printerJob); 
        } 
        public PrinterJob CreateReport(ReportType reportType, ListDictionary reportParameters, int copies) 
        { 
            return _localScannerChannel.CreateReport(reportType, reportParameters, copies); 
        } 
        public void PrintReportStarting() 
        { 
            _localScannerChannel.PrintReportStarting(); 
        } 
        public void PrintReportFinished() 
        { 
            _localScannerChannel.PrintReportFinished(); 
        } 
        #endregion 
        #region Сброс ПО 
        public void ResetSoft(ResetSoftReason reason, bool isRemoteScannerInitiator, bool needRestartApp) 
        { 
            _localScannerChannel.ResetSoft(reason, isRemoteScannerInitiator, needRestartApp); 
        } 
        #endregion 
        #endregion 
    } 
}

using System; 
using System.Collections.Generic; 
using System.Collections.Specialized; 
using System.Diagnostics; 
using System.Net.Sockets; 
using System.Runtime.Remoting; 
using System.Threading; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Printing; 
using Croc.Bpc.Scanner; 
using Croc.Bpc.Synchronization.Config; 
using Croc.Bpc.Utils; 
using Croc.Bpc.Voting; 
using Croc.Core; 
using Croc.Core.Diagnostics; 
using Croc.Core.Extensions; 
using Croc.Core.Utils; 
using Croc.Core.Utils.Threading; 
namespace Croc.Bpc.Synchronization 
{ 
    internal class RemoteScannerInterface : IScannerInteractionChannel, IDisposable 
    { 
        private readonly ILogger _logger; 
        private volatile IScannerInteractionChannel _interactionChannel; 
        private ScannerInfo _remoteScannerInfo; 
        private readonly CallPropertiesConfig _commonCallProperties; 
        private readonly CallPropertiesConfig _synchronizationCallProperties; 
        private readonly CallPropertiesConfig _printingCallProperties; 
        public RemoteScannerInterface( 
            ILogger logger, 
            CallPropertiesConfig commonCallProperties, 
            CallPropertiesConfig synchronizationCallProperties, 
            CallPropertiesConfig printingCallProperties) 
        { 
            CodeContract.Requires(logger != null); 
            CodeContract.Requires(commonCallProperties != null); 
            CodeContract.Requires(synchronizationCallProperties != null); 
            CodeContract.Requires(printingCallProperties != null); 
            _logger = logger; 
            _commonCallProperties = commonCallProperties; 
            _synchronizationCallProperties = synchronizationCallProperties; 
            _printingCallProperties = printingCallProperties; 
        } 
        public void SetInteractionChannel( 
            IScannerInteractionChannel interactionChannel, ScannerInfo remoteScannerInfo) 
        { 
            CodeContract.Requires(interactionChannel != null); 
            CodeContract.Requires(remoteScannerInfo != null); 
            lock (s_aliveSync) 
            { 
                _interactionChannel = interactionChannel; 
                _remoteScannerInfo = remoteScannerInfo; 
                _alive = true; 
            } 
        } 
        #region IScannerInteractionChannel Members 
        #region Система 
        private static readonly Version s_undefinedApplicationVersion = new Version(0, 0, 0, 0); 
        public Version ApplicationVersion 
        { 
            get 
            { 
                return SafeCall( 
                    () => _interactionChannel.ApplicationVersion, 
                    _commonCallProperties, 
                    s_undefinedApplicationVersion); 
            } 
        } 
        public void Ping() 
        { 
            SafeCall<object>( 
                () => { _interactionChannel.Ping(); return null; }, 
                _commonCallProperties, 
                null); 
        } 
        public void SetSystemTime(DateTime utcDateTime) 
        { 
            SafeCall<object>( 
                () => { _interactionChannel.SetSystemTime(utcDateTime); return null; }, 
                _commonCallProperties, 
                null); 
        } 
        #endregion 
        #region Исходные данные 
        public ElectionDayСomming IsElectionDay 
        { 
            get 
            { 
                return SafeCall( 
                    () => _interactionChannel.IsElectionDay, 
                    _commonCallProperties, 
                    ElectionDayСomming.ItsElectionDay); 
            } 
        } 
        public string SourceDataHashCode 
        { 
            get 
            { 
                return SafeCall( 
                    () => _interactionChannel.SourceDataHashCode, 
                    _commonCallProperties, 
                    string.Empty); 
            } 
        } 
        public bool IsSourceDataCorrect 
        { 
            get 
            { 
                return SafeCall( 
                    () => _interactionChannel.IsSourceDataCorrect, 
                    _commonCallProperties, 
                    false); 
            } 
        } 
        #endregion 
        #region Роль сканера 
        public event EventHandler ScannerRoleChanged; 
        internal void RaiseScannerRoleChanged() 
        { 
            ScannerRoleChanged.RaiseEvent(this); 
        } 
        public void RaiseRemoteScannerRoleChanged() 
        { 
            SafeCall<object>( 
                () => { _interactionChannel.RaiseRemoteScannerRoleChanged(); return null; }, 
                _commonCallProperties, 
                null); 
        } 
        public ScannerRole ScannerRole 
        { 
            get 
            { 
                return SafeCall( 
                    () => _interactionChannel.ScannerRole, 
                    _commonCallProperties, 
                    ScannerRole.Undefined); 
            } 
        } 
        #endregion 
        #region Передача данных 
        public void PutData(string name, object data) 
        { 
            SafeCall<object>( 
                () => { _interactionChannel.PutData(name, data); return null; }, 
                _commonCallProperties, 
                null); 
        } 
        public void NoticeAboutWaitForInitialization() 
        { 
            SafeCall<object>( 
                () => { _interactionChannel.NoticeAboutWaitForInitialization(); return null; }, 
                _commonCallProperties, 
                null); 
        } 
        public void NoticeAboutExitFromMenu() 
        { 
            SafeCall<object>( 
                () => { _interactionChannel.NoticeAboutExitFromMenu(); return null; }, 
                _commonCallProperties, 
                null); 
        } 
        public byte[] GetFileContent(string filePath) 
        { 
            return SafeCall( 
                () => _interactionChannel.GetFileContent(filePath), 
                _commonCallProperties, 
                null); 
        } 
        #endregion 
        #region Состояние 
        public bool IsStateInitial 
        { 
            get 
            { 
                return SafeCall( 
                    () => _interactionChannel.IsStateInitial, 
                    _commonCallProperties, 
                    true); 
            } 
        } 
        public void ResetState(string reason) 
        { 
            SafeCall<object>( 
                () => { _interactionChannel.ResetState(reason); return null; }, 
                _commonCallProperties, 
                null); 
        } 
        #endregion 
        #region Синхронизация состояния 
        public void NeedSynchronizeState(List<StateItem> newStateItems) 
        { 
            SafeCall<object>( 
                () => { _interactionChannel.NeedSynchronizeState(newStateItems); return null; }, 
                _synchronizationCallProperties, 
                null); 
        } 
        public void StateSynchronizationFinished(SynchronizationResult syncResult) 
        { 
            SafeCall<object>( 
                () => { _interactionChannel.StateSynchronizationFinished(syncResult); return null; }, 
                _synchronizationCallProperties, 
                null); 
        } 
        #endregion 
        #region Печать 
        public bool FindPrinter() 
        { 
            return SafeCall( 
                () => _interactionChannel.FindPrinter(), 
                _commonCallProperties, 
                false); 
        } 
        public bool PrintReport(PrinterJob printerJob) 
        { 
            return SafeCall( 
                () => _interactionChannel.PrintReport(printerJob), 
                _printingCallProperties, 
                false); 
        } 
        public PrinterJob CreateReport(ReportType reportType, ListDictionary reportParameters, int copies) 
        { 
            return SafeCall( 
                () => _interactionChannel.CreateReport(reportType, reportParameters, copies), 
                _printingCallProperties, 
                null); 
        } 
        public void PrintReportStarting() 
        { 
            SafeCall<object>( 
                () => { _interactionChannel.PrintReportStarting(); return null; }, 
                _commonCallProperties, 
                null); 
        } 
        public void PrintReportFinished() 
        { 
            SafeCall<object>( 
                () => { _interactionChannel.PrintReportFinished(); return null; }, 
                _commonCallProperties, 
                null); 
        } 
        #endregion 
        #region Сброс ПО 
        public void ResetSoft(ResetSoftReason reason, bool isRemoteScannerInitiator, bool needRestartApp) 
        { 
            SafeCall<object>( 
                () => 
                    { 
                        _interactionChannel.ResetSoft(reason, isRemoteScannerInitiator, needRestartApp); 
                        return null; 
                    }, 
                _commonCallProperties, 
                null); 
        } 
        #endregion 
        #endregion 
        #region Безопасный вызов методов канала 
        private static readonly object s_aliveSync = new object(); 
        private volatile bool _alive; 
        public bool Alive 
        { 
            get 
            { 
                return _alive; 
            } 
        } 
        public event EventHandler Disconnected; 
        private void RaiseDisconnected() 
        { 
            lock (s_aliveSync) 
            { 
                if (!_alive) 
                { 
                    _logger.LogInfo(Message.SyncDisconnectedAlreadyKnown); 
                    return; 
                } 
                _logger.LogInfo(Message.SyncDisconnected); 
                _alive = false; 
                ScannerRoleChanged = null; 
            } 
            try 
            { 
                var handler = Disconnected; 
                if (handler != null) 
                    handler(this, EventArgs.Empty); 
            } 
            catch (Exception ex) 
            { 
                _logger.LogError(Message.SyncRemoteScannerDisconnectedError, ex); 
            } 
        } 
        private static readonly object s_ifrestartSync = new object(); 
        private readonly ManualResetEvent _ifrestartDoneEvent = new ManualResetEvent(true); 
        private enum IfrestartResult 
        { 
            Ok = 0, 
            OkAfterRestart = 1, 
            Failed = 2 
        } 
        private T SafeCall<T>(Func<T> method, CallPropertiesConfig callProps, T returnOnError) 
        { 
            if (!_alive) 
                return returnOnError; 
            var tryCount = 0; 
            var methodComplete = new AutoResetEvent(false); 
            while (!_disposed && _alive) 
            { 
                try 
                { 
                    var result = default(T); 
                    Exception methodEx = null; 
                    methodComplete.Reset(); 
                    var thread = ThreadUtils.StartBackgroundThread( 
                        () => 
                            { 
                                try 
                                { 
                                    result = method(); 
                                } 
                                catch (Exception ex) 
                                { 
                                    methodEx = ex; 
                                } 
                                finally 
                                { 
                                    methodComplete.Set(); 
                                } 
                            }); 
                    if (!methodComplete.WaitOne(callProps.Timeout)) 
                    { 
                        thread.SafeAbort(); 
                        throw new TimeoutException("Не дождались завершение выполнения метода"); 
                    } 
                    if (methodEx != null) 
                        throw methodEx; 
                    return result; 
                } 
                catch (Exception ex) 
                { 
                    _logger.LogVerbose( 
                        Message.SyncCallRemoteMethodFailed, 
                        () => 
                            { 
                                var methodInfo = (new StackTrace()).GetFrame(3).GetMethod(); 
                                return new object[] {methodInfo.Name, ex.ToString()}; 
                            }); 
                    if ( // ошибка не из-за сети 
                        ((!(ex is SocketException) && !(ex is RemotingException)) && !(ex is TimeoutException)) || 
                        ++tryCount >= callProps.MaxTryCount || 
                        !_alive || 
                        _disposed) 
                    { 
                        RaiseDisconnected(); 
                        return returnOnError; 
                    } 
                    if (!Monitor.TryEnter(s_ifrestartSync)) 
                    { 
                        Thread.Sleep(300); 
                        _ifrestartDoneEvent.WaitOne(TimeSpan.FromMinutes(1)); 
                        continue; 
                    } 
                    try 
                    { 
                        _ifrestartDoneEvent.Reset(); 
                        IfrestartResult res; 
                        if (PlatformDetector.IsUnix) 
                        { 
                            _logger.LogVerbose(Message.SyncIfrestartStarting); 
                            string lastLine = null; 
                            ProcessHelper.StartProcessAndWaitForFinished( 
                                "./ifrestart.sh", 
                                string.Format("{0} {1}", callProps.RetryDelay, _remoteScannerInfo.IpAddress), 
                                state => 
                                    { 
                                        lastLine = state.Line; 
                                        return false; 
                                    }, 
                                null); 
                            _logger.LogVerbose(Message.SyncIfrestartDone, lastLine); 
                            int i; 
                            res = int.TryParse(lastLine, out i) 
                                      ? (IfrestartResult) i 
                                      : IfrestartResult.Failed; 
                        } 
                        else 
                        { 
                            _logger.LogVerbose(Message.Common_Debug, 
                                               "Имитируем выполнение команды переподнятия сети"); 
                            Thread.Sleep(4000 + callProps.RetryDelay * 2 * 1000); 
                            res = IfrestartResult.OkAfterRestart; 
                        } 
                        switch (res) 
                        { 
                            case IfrestartResult.OkAfterRestart: 
                                _logger.LogVerbose(Message.SyncTryCallRemoteMethodAgain, 
                                    () => 
                                        { 
                                            var methodInfo = (new StackTrace()).GetFrame(3).GetMethod(); 
                                            return new object[] {methodInfo.Name, tryCount + 1}; 
                                        }); 
                                continue; 
                            default: 
                                RaiseDisconnected(); 
                                return returnOnError; 
                        } 
                    } 
                    finally 
                    { 
                        Monitor.Exit(s_ifrestartSync); 
                        _ifrestartDoneEvent.Set(); 
                    } 
                } 
            } 
            return returnOnError; 
        } 
        #endregion 
        #region IDisposable Members 
        private bool _disposed; 
        public void Dispose() 
        { 
            _disposed = true; 
            _alive = false; 
        } 
        #endregion 
    } 
}

using System; 
using System.Collections; 
using System.Collections.Generic; 
using System.Collections.Specialized; 
using System.IO; 
using System.Net.Sockets; 
using System.Runtime.Remoting; 
using System.Runtime.Remoting.Channels; 
using System.Runtime.Remoting.Channels.Tcp; 
using System.Runtime.Serialization; 
using System.Runtime.Serialization.Formatters.Binary; 
using System.Security.Permissions; 
using System.Text; 
using System.Threading; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.FileSystem; 
using Croc.Bpc.Printing; 
using Croc.Bpc.Scanner; 
using Croc.Bpc.Synchronization.Config; 
using Croc.Bpc.Utils; 
using Croc.Bpc.Voting; 
using Croc.Core; 
using Croc.Core.Configuration; 
using Croc.Core.Extensions; 
using Croc.Core.Utils.IO; 
using Croc.Core.Utils.Threading; 
using Message = Croc.Bpc.Diagnostics.Message; 
namespace Croc.Bpc.Synchronization 
{ 
    [SubsystemConfigurationElementTypeAttribute(typeof(SynchronizationManagerConfig))] 
    public class SynchronizationManager : 
        Subsystem, 
        ISynchronizationManager 
    { 
        private SynchronizationManagerConfig _config; 
        private IScannerManager _scannerManager; 
        private IElectionManager _electionManager; 
        private IVotingResultManager _votingResultManager; 
        private IFileSystemManager _fileSystemManager; 
        private IPrintingManager _printingManager; 
        public override void Init(SubsystemConfig config) 
        { 
            _config = (SynchronizationManagerConfig)config; 
            _scannerManager = Application.GetSubsystemOrThrow<IScannerManager>(); 
            _fileSystemManager = Application.GetSubsystemOrThrow<IFileSystemManager>(); 
            _electionManager = Application.GetSubsystemOrThrow<IElectionManager>(); 
            _votingResultManager = Application.GetSubsystemOrThrow<IVotingResultManager>(); 
            _printingManager = Application.GetSubsystemOrThrow<IPrintingManager>(); 
            _printingManager.PrintReportStarting += (s, e) => SetPrintReportExecutingNow(true); 
            _printingManager.PrintReportFinished += (s, e) => ResetPrintReportExecutingNow(true); 
            InitRemoteScannerCommunication(); 
            InitState(); 
        } 
        #region Печать 
        private void SetPrintReportExecutingNow(bool printStartingOnLocal) 
        { 
            Logger.LogVerbose(printStartingOnLocal 
                                  ? Message.SyncPrintReportStartingOnLocal 
                                  : Message.SyncPrintReportStartingOnRemote); 
            if (printStartingOnLocal && IsRemoteScannerConnected) 
                RemoteScanner.PrintReportStarting(); 
            EnablePing(false); 
        } 
        private void ResetPrintReportExecutingNow(bool printFinishedOnLocal) 
        { 
            Logger.LogVerbose(printFinishedOnLocal 
                                  ? Message.SyncPrintReportFinishedOnLocal 
                                  : Message.SyncPrintReportFinishedOnRemote); 
            if (printFinishedOnLocal && IsRemoteScannerConnected) 
                RemoteScanner.PrintReportFinished(); 
            EnablePing(true); 
        } 
        #endregion 
        #region Установка соединения с удаленным сканером 
        private const int REMOTE_INTERACTION_PORT = 9090; 
        private const string REMOTE_INTERACTION_OBJECT = "RemoteInteraction.rem"; 
        private const string REMOTE_INTERACTION_URI_FORMAT = "tcp://{0}:{1}/" + REMOTE_INTERACTION_OBJECT; 
        private const int REMOTE_CONNECTION_TIMEOUT = 5000; 
        private TcpChannel _hostChannel; 
        private volatile ScannerInfo _localScannerInfo; 
        private volatile ScannerInfo _remoteScannerInfo; 
        private RemoteScannerInterface _remoteScannerInterface; 
        private static readonly object s_remoteScannerSync = new object(); 
        public event EventHandler RemoteScannerConnected; 
        private readonly ManualResetEvent _remoteScannerConnected = new ManualResetEvent(false); 
        public event EventHandler RemoteScannerDisconnected; 
        private readonly ManualResetEvent _remoteScannerDisconnected = new ManualResetEvent(true); 
        public void OpenIncomingInteractionChannel(string localSerialNumber, string localIpAddress) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(localSerialNumber)); 
            CodeContract.Requires(!string.IsNullOrEmpty(localIpAddress)); 
            if (_hostChannel != null) 
                return; 
            _localScannerInfo = new ScannerInfo(localSerialNumber, localIpAddress); 
            try 
            { 
                IDictionary props = new Hashtable(); 
                props["name"] = "BpcTcpChannel"; 
                props["port"] = REMOTE_INTERACTION_PORT; 
                _hostChannel = new TcpChannel( 
                    props, 
                    new BinaryClientFormatterSinkProvider(), 
                    new BinaryServerFormatterSinkProvider()); 
                ChannelServices.RegisterChannel(_hostChannel, false); 
                Logger.LogInfo(Message.SyncChannelOpened, localIpAddress); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.SyncOpenInteractionChannelError, ex); 
            } 
        } 
        private void CloseIncomingInteractionChannel() 
        { 
            if (_hostChannel == null) 
                return; 
            try 
            { 
                _hostChannel.StopListening(null); 
            } 
            catch 
            { 
            } 
            try 
            { 
                ChannelServices.UnregisterChannel(_hostChannel); 
            } 
            catch 
            { 
            } 
            _hostChannel = null; 
            Logger.LogInfo(Message.SyncChannelClosed); 
        } 
        private static string GetUriForRemoteInteraction(string ipAddress) 
        { 
            return string.Format(REMOTE_INTERACTION_URI_FORMAT, ipAddress, REMOTE_INTERACTION_PORT); 
        } 
        private void InitRemoteScannerCommunication() 
        { 
            _scannerManager.RemoteScannerConnected += ScannerManager_RemoteScannerConnected; 
            _remoteScannerInterface = new RemoteScannerInterface( 
                Logger, 
                _config.RemoteScannerCallProperties.Common, 
                _config.RemoteScannerCallProperties.Synchronization, 
                _config.RemoteScannerCallProperties.Printing); 
            _remoteScannerInterface.Disconnected += RemoteScannerInterface_Disconnected; 
            RemoteScannerConnector.GetChannelToLocalScannerEvent += () => this; 
            RemoteScannerConnector.IsRemoteConnectionAllowEvent += OnIsRemoteConnectionAllow; 
            RemotingConfiguration.RegisterWellKnownServiceType( 
                typeof(RemoteScannerConnector), 
                REMOTE_INTERACTION_OBJECT, 
                WellKnownObjectMode.Singleton); 
            StartPing(); 
        } 
        private void DisposeRemoteScannerCommunication() 
        { 
            if (_scannerManager != null) 
                _scannerManager.RemoteScannerConnected -= ScannerManager_RemoteScannerConnected; 
            if (_remoteScannerInterface != null) 
            { 
                _remoteScannerInterface.Disconnected -= RemoteScannerInterface_Disconnected; 
                _remoteScannerInterface.Dispose(); 
            } 
        } 
        #region Проверка связи 
        private Thread _pingThread; 
        private volatile bool _pingEnabled = true; 
        private void StartPing() 
        { 
            if (_config.PingPeriod.Value > 0) 
                _pingThread = ThreadUtils.StartBackgroundThread(PingThread); 
        } 
        private void StopPing() 
        { 
            if (_pingThread != null) 
                _pingThread.SafeAbort(1000); 
        } 
        private void EnablePing(bool enabled) 
        { 
            _pingEnabled = enabled; 
            Logger.LogVerbose(enabled ? Message.SyncMonitorConnectionEnabled : Message.SyncMonitorConnectionDisabled); 
            if (!enabled) 
                Thread.Sleep(100); 
        } 
        private void PingThread() 
        { 
            Logger.LogVerbose(Message.SyncMonitorConnectionStarted); 
            while (!_disposed) 
            { 
                if (!WaitOne(_remoteScannerConnected, null)) 
                    return; 
                var waitSeconds = _config.PingPeriod.Value; 
#if DEBUG 
                waitSeconds *= 10; 
#endif 
                Thread.Sleep(TimeSpan.FromSeconds(waitSeconds)); 
                if (!_pingEnabled) 
                    continue; 
                Logger.LogVerbose(Message.SyncExecMonitorConnection); 
                _remoteScannerInterface.Ping(); 
            } 
        } 
        void IScannerInteractionChannel.Ping() 
        { 
            if (!IsRemoteScannerConnected) 
                throw new Exception("Соединение с удаленным сканером не установлено"); 
        } 
        #endregion 
        private bool OnIsRemoteConnectionAllow(string remoteSerialNumber, string remoteIpAddress) 
        { 
            lock (s_remoteScannerSync) 
            { 
                if (// нам еще не известен (т.е. мы еще не получали от него широковещательных сообщений) 
                    _remoteScannerInfo == null || 
                    !_remoteScannerInfo.Equals(new ScannerInfo(remoteSerialNumber, remoteIpAddress))) 
                { 
                    Logger.LogVerbose(Message.SyncConnectRejected, remoteSerialNumber, remoteIpAddress); 
                    return false; 
                } 
                Logger.LogVerbose(Message.SyncConnectAccepted, remoteSerialNumber, remoteIpAddress); 
                return true; 
            } 
        } 
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)] 
        private void ScannerManager_RemoteScannerConnected(object sender, ScannerEventArgs e) 
        { 
            lock (s_remoteScannerSync) 
            { 
                if (IsRemoteScannerConnected) 
                { 
                    Logger.LogVerbose(Message.SyncIgnoreRemoteScannerConnectedEvent); 
                    return; 
                } 
                _remoteScannerInfo = new ScannerInfo(e.SerialNumber, e.IpAddress); 
            } 
            if (_localScannerInfo == null) 
                return; 
            try 
            { 
                Logger.LogVerbose(Message.SyncTryOpenConnection); 
                var uri = GetUriForRemoteInteraction(e.IpAddress); 
                var connector = GetRemoteScannerConnector(uri); 
                var allowed = connector.IsRemoteConnectionAllow( 
                    _localScannerInfo.SerialNumber, _localScannerInfo.IpAddress); 
                if (!allowed) 
                { 
                    Logger.LogVerbose(Message.SyncRemoteConnectionNotAllow); 
                    return; 
                } 
                _remoteScannerInterface.SetInteractionChannel(connector, _remoteScannerInfo); 
                Logger.LogInfo(Message.SyncRemoteConnectionSuccess, e.SerialNumber, e.IpAddress); 
                _remoteScannerDisconnected.Reset(); 
                _remoteScannerConnected.Set(); 
                RemoteScannerConnected.RaiseEvent(this); 
            } 
            catch (ThreadAbortException) 
            { 
                throw; 
            } 
            catch (Exception ex) 
            { 
                if (ex is RemotingException || ex is SocketException) 
                    Logger.LogInfo(Message.SyncCannotConnectToRemoteScanner, ex.Message); 
                else 
                    Logger.LogError(Message.SyncConnectRemoteScannerError, ex); 
            } 
        } 
        private RemoteScannerConnector GetRemoteScannerConnector(string uri) 
        { 
            Logger.LogVerbose(Message.SyncGetRemoteConnector, uri); 
            RemoteScannerConnector connector = null; 
            var done = new AutoResetEvent(false); 
            Exception getConnectorException = null; 
            var getConnectorThread = ThreadUtils.StartBackgroundThread( 
                () => 
                    { 
                        try 
                        { 
                            connector = (RemoteScannerConnector)Activator.GetObject( 
                                typeof(RemoteScannerConnector), uri); 
                        } 
                        catch (Exception ex) 
                        { 
                            getConnectorException = ex; 
                        } 


                        done.Set(); 
                    }); 
            if (!done.WaitOne(REMOTE_CONNECTION_TIMEOUT)) 
            { 
                getConnectorThread.SafeAbort(); 
                throw new RemotingException("Таймаут при попытке получить коннектор"); 
            } 
            if (connector == null) 
                throw new Exception( 
                    "Не удалось получить коннектор" +  
                    getConnectorException == null ? null : ": " + getConnectorException.Message); 
            return connector; 
        } 
        private void RemoteScannerInterface_Disconnected(object sender, EventArgs e) 
        { 
            StateSynchronizationFinished(SynchronizationResult.RemoteScannerDisconnected); 
            Logger.LogInfo( 
                Message.SyncRemoteConnectionLoss, 
                _remoteScannerInfo.SerialNumber, _remoteScannerInfo.IpAddress); 
            _remoteScannerConnected.Reset(); 
            _remoteScannerDisconnected.Set(); 
            _scannerManager.RestartBroadcasting(); 
            RemoteScannerDisconnected.RaiseEvent(this, e); 
        } 
        #endregion 
        #region Работа с удаленным сканером 
        Version IScannerInteractionChannel.ApplicationVersion 
        { 
            get 
            { 
                return CoreApplication.Instance.ApplicationVersion; 
            } 
        } 
        void IScannerInteractionChannel.SetSystemTime(DateTime utcDateTime) 
        { 
            SystemHelper.SetSystemTime(utcDateTime); 
            Logger.LogInfo(Message.SyncSetSystemTime, utcDateTime); 
        } 
        ElectionDayСomming IScannerInteractionChannel.IsElectionDay 
        { 
            get 
            { 
                return _electionManager.IsElectionDay(); 
            } 
        } 
        string IScannerInteractionChannel.SourceDataHashCode 
        { 
            get 
            { 
                return _electionManager.SourceData == null 
                           ? string.Empty 
                           : _electionManager.SourceData.HashCode; 
            } 
        } 
        bool IScannerInteractionChannel.IsSourceDataCorrect 
        { 
            get 
            { 
                return _electionManager.IsSourceDataCorrect; 
            } 
        } 
        public IScannerInteractionChannel RemoteScanner 
        { 
            get 
            { 
                return _remoteScannerInterface; 
            } 
        } 
        public bool IsRemoteScannerConnected 
        { 
            get 
            { 
                return _remoteScannerInterface.Alive; 
            } 
        } 
        #region Печать 
        bool IScannerInteractionChannel.FindPrinter() 
        { 
            return _printingManager.FindPrinter(); 
        } 
        bool IScannerInteractionChannel.PrintReport(PrinterJob printerJob) 
        { 
            return _printingManager.PrintReport(printerJob); 
        } 
        PrinterJob IScannerInteractionChannel.CreateReport(ReportType reportType, ListDictionary reportParameters, int copies) 
        { 
            return _printingManager.CreateReport(reportType, reportParameters, copies); 
        } 
        void IScannerInteractionChannel.PrintReportStarting() 
        { 
            SetPrintReportExecutingNow(false); 
        } 
        void IScannerInteractionChannel.PrintReportFinished() 
        { 
            ResetPrintReportExecutingNow(false); 
        } 
        #endregion 
        #region Сброс ПО 
        void IScannerInteractionChannel.ResetSoft( 
            ResetSoftReason reason, bool isRemoteScannerInitiator, bool needRestartApp) 
        { 
            var resetSoftParams = new ResetSoftParams(reason, isRemoteScannerInitiator, needRestartApp); 
            if (isRemoteScannerInitiator) 
                ThreadUtils.StartBackgroundThread(ResetSoftInternal, resetSoftParams); 
            else 
                ResetSoftInternal(resetSoftParams); 
        } 
        private class ResetSoftParams 
        { 
            public ResetSoftReason Reason { get; private set; } 
            public bool IsRemoteScannerInitiator { get; private set; } 
            public bool NeedRestartApp { get; private set; } 
            public ResetSoftParams(ResetSoftReason reason, bool isRemoteScannerInitiator, bool needRestartApp) 
            { 
                Reason = reason; 
                IsRemoteScannerInitiator = isRemoteScannerInitiator; 
                NeedRestartApp = needRestartApp; 
            } 
        } 
        private void ResetSoftInternal(object state) 
        { 
            var resetSoftParams = (ResetSoftParams)state; 
            Logger.LogInfo(Message.SyncResetSoft,  
                resetSoftParams.Reason, resetSoftParams.IsRemoteScannerInitiator, resetSoftParams.NeedRestartApp); 
            var archivePrefix = GetArchivePrefix(resetSoftParams); 
            _fileSystemManager.ArchiveFiles(archivePrefix); 
            if (resetSoftParams.NeedRestartApp) 
                Application.Exit(ApplicationExitType.RestartApplication); 
        } 
        private string GetArchivePrefix(ResetSoftParams resetSoftParams) 
        { 
            var electionMode = ElectionMode.Training; 
            var electionDate = "xxxxxxxx"; 
            if (_electionManager.HasSourceData()) 
            { 
                electionMode = _electionManager.SourceData.ElectionMode; 
                electionDate = _electionManager.SourceData.ElectionDate.ToString("yyyyMMdd"); 
            } 
            var electionModeStr = (electionMode == ElectionMode.Real ? "real" : "train"); 
            string reasonStr; 
            switch (resetSoftParams.Reason) 
            { 
                case ResetSoftReason.ElectionFinished: 
                    reasonStr = "el"; 
                    break; 
                case ResetSoftReason.ResetSoftFromSystemMenu: 
                    reasonStr = "rsm"; 
                    break; 
                case ResetSoftReason.ResetUikFromSystemMenu: 
                    reasonStr = "rum"; 
                    break; 
                case ResetSoftReason.ControlVotingStartTriggered: 
                    reasonStr = "cvs"; 
                    break; 
                case ResetSoftReason.ConflictDetected: 
                    reasonStr = "cnf"; 
                    break; 
                default: 
                    throw new ArgumentOutOfRangeException("resetSoftParams", "Неожиданная причина сброса ПО"); 
            } 
            if (resetSoftParams.IsRemoteScannerInitiator) 
                reasonStr += "_rs"; 
            return string.Format("{{0:000000}}_{0}_{1}_{2}", electionModeStr, electionDate, reasonStr); 
        } 
        public void ResetUik(ResetSoftReason reason) 
        { 
            SynchronizationEnabled = false; 
            if (IsRemoteScannerConnected) 
            { 
                EnablePing(false); 
                RemoteScanner.ResetSoft(reason, true, true); 
            } 
            ((IScannerInteractionChannel)this).ResetSoft(reason, false, true); 
        } 
        #endregion 
        #endregion 
        #region Обмен данными с удаленным сканером 
        private readonly Dictionary<string, object> _transmittedData = new Dictionary<string, object>(); 
        private static readonly object s_transmittedDataSync = new object(); 
        private readonly ManualResetEvent _newDataTransmitted = new ManualResetEvent(false); 
        public object GetDataTransmittedFromRemoteScanner(string name, IWaitController waitCtrl) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(name)); 
            while (true) 
            { 
                var data = FindTransmittedData(name); 
                if (data != null) 
                    return data; 
                int index; 
                var res = WaitAny( 
                    new[] { _remoteScannerDisconnected, _newDataTransmitted }, 
                    out index, 
                    1000,   // 1 сек 
                    waitCtrl); 
                if (!res || index == 0) 
                    return null; 
            } 
        } 
        private object FindTransmittedData(string name) 
        { 
            lock (s_transmittedDataSync) 
            { 
                object res = null; 
                if (_transmittedData.ContainsKey(name)) 
                { 
                    res = _transmittedData[name]; 
                    _transmittedData.Remove(name); 
                } 
                _newDataTransmitted.Reset(); 
                return res; 
            } 
        } 
        void IScannerInteractionChannel.PutData(string name, object data) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(name)); 
            CodeContract.Requires(data != null); 
            lock (s_transmittedDataSync) 
            { 
                _transmittedData[name] = data; 
                _newDataTransmitted.Set(); 
            } 
        } 
        public event EventHandler RemoteScannerWaitForInitialization; 
        public event EventHandler RemoteScannerExitFromMenu; 
        public void NoticeAboutWaitForInitialization() 
        { 
            Logger.LogVerbose(Message.Common_DebugCall); 
            RemoteScannerWaitForInitialization.RaiseEvent(this); 
        } 
        public void NoticeAboutExitFromMenu() 
        { 
            Logger.LogVerbose(Message.Common_DebugCall); 
            RemoteScannerExitFromMenu.RaiseEvent(this); 
        } 
        public byte[] GetFileContent(string filePath) 
        { 
            return File.ReadAllBytes(filePath); 
        } 
        #endregion 
        #region Роль сканера 
        public event EventHandler ScannerRoleChanged; 
        void IScannerInteractionChannel.RaiseRemoteScannerRoleChanged() 
        { 
            _remoteScannerInterface.RaiseScannerRoleChanged(); 
        } 
        private volatile ScannerRole _scannerRole = ScannerRole.Undefined; 
        public ScannerRole ScannerRole 
        { 
            get 
            { 
                return _scannerRole; 
            } 
            set 
            { 
                if (_scannerRole == value) 
                    return; 
                _scannerRole = value; 
                Logger.LogInfo(Message.SyncScannerRoleSet, _scannerRole); 
                ScannerRoleChanged.RaiseEvent(this); 
                _remoteScannerInterface.RaiseRemoteScannerRoleChanged(); 
            } 
        } 
        #endregion 
        #region Работа с состоянием 
        private const string STATE_FILE_NAME = "state"; 
        private const string STATE_FILE_EXT = "bin"; 
        private string _stateFilePath; 
        private readonly IFormatter _stateSerializationFormatter = new BinaryFormatter(); 
        private Dictionary<string, StateItem> _stateDict; 
        private static readonly object s_stateSync = new object(); 
        private const string STATE_SYNC = "StateSync"; 
        private bool _isStateInitial = true; 
        bool IScannerInteractionChannel.IsStateInitial 
        { 
            get 
            { 
                return _isStateInitial; 
            } 
        } 
        private readonly ManualResetEvent _synchronizationFinished = new ManualResetEvent(false); 
        private volatile SynchronizationResult _lastSyncResult = SynchronizationResult.Succeeded; 
        private static readonly object s_syncResultSync = new object(); 
        private volatile bool _stateSynchronizationEnabled; 
        public bool SynchronizationEnabled 
        { 
            get 
            { 
                return _stateSynchronizationEnabled; 
            } 
            set 
            { 
                _stateSynchronizationEnabled = value; 
                if (!_stateSynchronizationEnabled) 
                { 
                    Logger.LogVerbose(Message.SyncDisabled); 
                    StateSynchronizationFinished(SynchronizationResult.SynchronizationDisabled); 
                } 
                else 
                { 
                    Logger.LogVerbose(Message.SyncEnabled); 
                } 
            } 
        } 
        private void InitState() 
        { 
            var stateDirectory = _fileSystemManager.GetDataDirectoryPath(FileType.State); 
            _stateFilePath = Path.Combine( 
                stateDirectory, 
                string.Format("{0}.{1}", STATE_FILE_NAME, STATE_FILE_EXT)); 
            var stateSubsystems = Application.GetSubsystems<IStateSubsystem>(); 
            _stateDict = new Dictionary<string, StateItem>(stateSubsystems.Count); 
            foreach (var entry in stateSubsystems) 
            { 
                var subsystemName = entry.Key; 
                var subsystem = entry.Value; 
                subsystem.ResetState(false); 
                var subsystemState = subsystem.GetState(); 
                _stateDict[subsystemName] = new StateItem(subsystemName) { Value = subsystemState }; 
                subsystem.StateChanged += StateSubsystem_StateChanged; 
            } 
        } 
        private void StateSubsystem_StateChanged(object sender, SubsystemStateEventArgs e) 
        { 
            WaitForCurrentSynchronizationFinished(); 
            SaveSubsystemState(e.Subsystem.Name, e.State); 
            StartStateSynchronization(false); 
        } 
        #region Синхронизация состояния 
        public void StartStateSynchronization(bool enableSync) 
        { 
            if (!WaitForCurrentSynchronizationFinished()) 
                return; 
            lock (s_syncResultSync) 
            { 
                _synchronizationFinished.Reset(); 
            } 
            ThreadUtils.StartBackgroundThread(StartStateSynchronizationMethod, enableSync); 
        } 
        private void StartStateSynchronizationMethod(object state) 
        { 
            Logger.LogVerbose(Message.SyncStartSynchronization); 
            try 
            { 
                if ((bool)state) 
                { 
                    SynchronizationEnabled = true; 
                    lock (s_stateSync) 
                    { 
                        SaveState(); 
                    } 
                } 
                if (!_stateSynchronizationEnabled || !IsRemoteScannerConnected) 
                { 
                    Logger.LogVerbose(Message.SyncStopedOrNoSecondScanner); 
                    StateSynchronizationFinished(SynchronizationResult.SynchronizationNotEnabled); 
                    return; 
                } 
                lock (s_stateSync) 
                { 
                    TransferStateToRemoteScanner(false); 
                } 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.SyncSynchronizationError, ex); 
                StateSynchronizationFinished(SynchronizationResult.Failed); 
            } 
        } 
        private void TransferStateToRemoteScanner(bool signalIfSynchronized) 
        { 
            try 
            { 
                var stateItemsForSync = new List<StateItem>(); 
                foreach (var item in _stateDict) 
                    if (!item.Value.Synchronized) 
                    { 
                        signalIfSynchronized = false; 
                        stateItemsForSync.Add(item.Value); 
                    } 
                    else 
                        stateItemsForSync.Add(item.Value.GetSynchronizedEmptyClone()); 
                if (signalIfSynchronized) 
                { 
                    StateSynchronizationFinished(SynchronizationResult.Succeeded); 
                    ThreadUtils.StartBackgroundThread( 
                        () => _remoteScannerInterface.StateSynchronizationFinished(SynchronizationResult.Succeeded)); 
                } 
                else 
                { 
                    Logger.LogVerbose( 
                        Message.SyncStateItemsForSync, 
                        () => new object[] { GetStateItemsForSyncDescription(stateItemsForSync) }); 
                    ThreadUtils.StartBackgroundThread( 
                        () => _remoteScannerInterface.NeedSynchronizeState(stateItemsForSync)); 
                } 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.SyncSynchronizationError, ex); 
                StateSynchronizationFinished(SynchronizationResult.Failed); 
            } 
        } 
        private static string GetStateItemsForSyncDescription(IEnumerable<StateItem> items) 
        { 
            var sb = new StringBuilder(256); 
            foreach (var item in items) 
            { 
                sb.Append(item.Name); 
                if (item.Synchronized) 
                    sb.Append('+'); 
                sb.Append(';'); 
            } 
            if (sb.Length > 0) 
                sb.Length -= 1; 
            return sb.ToString(); 
        } 
        void IScannerInteractionChannel.NeedSynchronizeState(List<StateItem> newStateItems) 
        { 
            if (!_stateSynchronizationEnabled) 
            { 
                Logger.LogVerbose(Message.SyncIgnoreSyncRequestByDisabled); 
                ThreadUtils.StartBackgroundThread( 
                    () => _remoteScannerInterface.StateSynchronizationFinished( 
                        SynchronizationResult.SyncRequestIgnored)); 
                return; 
            } 
            ThreadUtils.StartBackgroundThread(DoSynchronizeState, newStateItems); 
        } 
        private void DoSynchronizeState(object state) 
        { 
            Logger.LogVerbose(Message.SyncStartRequestExecuting); 
            try 
            { 
                lock (s_syncResultSync) 
                { 
                    _synchronizationFinished.Reset(); 
                } 
                var newStateItems = (List<StateItem>)state; 
                lock (s_stateSync) 
                { 
                    var needSave = false; // признак, что нужно будет выполнить сохранение состояния 
                    foreach (var newStateItem in newStateItems) 
                    { 
                        var subsystemName = newStateItem.Name; 
                        Logger.LogVerbose(Message.SyncSubsystemState, subsystemName); 
                        if (!_stateDict.ContainsKey(subsystemName)) 
                            continue; 
                        var curStateItem = _stateDict[subsystemName]; 
                        if (newStateItem.Synchronized) 
                        { 
                            Logger.LogVerbose(Message.SyncNotNeeded); 
                            curStateItem.Synchronized = true; 
                        } 
                        else 
                        { 
                            var subsystem = (IStateSubsystem)Application.GetSubsystem(subsystemName); 
                            var res = subsystem.AcceptNewState(newStateItem.Value); 
                            Logger.LogVerbose(Message.SyncSubsystemStateResult, subsystemName, res); 
                            switch (res) 
                            { 
                                case SubsystemStateAcceptanceResult.Accepted: 
                                    curStateItem.Value = newStateItem.Value; 
                                    curStateItem.Synchronized = true; 
                                    needSave = true; 
                                    break; 
                                case SubsystemStateAcceptanceResult.AcceptedByMerge: 
                                    curStateItem.Value = subsystem.GetState(); 
                                    needSave = true; 
                                    break; 
                                case SubsystemStateAcceptanceResult.Rejected: 
                                    curStateItem.Synchronized = false; 
                                    break; 
                                default: 
                                    throw new Exception("Неизвестный результат"); 
                            } 
                        } 
                    } 
                    if (needSave) 
                        SaveState(); 
                    TransferStateToRemoteScanner(true); 
                } 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.SyncSynchronizationError, ex); 
                StateSynchronizationFinished(SynchronizationResult.Failed); 
            } 
            finally 
            { 
                Logger.LogVerbose(Message.SyncRequestProcessed); 
            } 
        } 
        public void StateSynchronizationFinished(SynchronizationResult syncResult) 
        { 
            Logger.LogVerbose(Message.SyncComplete, syncResult); 
            lock (s_stateSync) 
            { 
                var synchronized = (syncResult == SynchronizationResult.Succeeded); 
                foreach (var item in _stateDict) 
                    item.Value.Synchronized = synchronized; 
            } 
            lock (s_syncResultSync) 
            { 
                _lastSyncResult = syncResult; 
                _synchronizationFinished.Set(); 
            } 
        } 
        private bool WaitForCurrentSynchronizationFinished() 
        { 
            if (!_stateSynchronizationEnabled) 
                return true; 
            Logger.LogVerbose(Message.SyncWaitForCurrentSynchronizationFinished); 
            return WaitOne(_synchronizationFinished, 3000, null); 
        } 
        public bool WaitForSynchronizationFinished(IWaitController waitCtrl) 
        { 
            while (true) 
            { 
                if (!WaitOne(_synchronizationFinished, waitCtrl)) 
                    return false; 
                lock (s_syncResultSync) 
                { 
                    if (_lastSyncResult == SynchronizationResult.SyncRequestIgnored) 
                    { 
                        _synchronizationFinished.Reset(); 
                        continue; 
                    } 
                    return (_lastSyncResult == SynchronizationResult.SynchronizationNotEnabled || 
                            _lastSyncResult == SynchronizationResult.Succeeded); 
                } 
            } 
        } 
        #endregion 
        #region Загрузка/Сохранение/Сброс состояния 
        private bool _stateLoaded; 
        public bool LoadState() 
        { 
            try 
            { 
                if (LoadStateInternal()) 
                { 
                    Logger.LogInfo(Message.SyncStateLoaded); 
                    return true; 
                } 
                Logger.LogInfo(Message.SyncStateLoadFailed); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.SyncStateLoadError, ex); 
            } 
            return false; 
        } 
        private bool LoadStateInternal() 
        { 
            lock (s_stateSync) 
            { 
                _stateLoaded = true; 
                Dictionary<string, StateItem> loadedStateDict; 
                if (!_fileSystemManager.SafeDeserialization( 
                    out loadedStateDict, _stateSerializationFormatter, _stateFilePath)) 
                { 
                    return false; 
                } 
                if (loadedStateDict == null) 
                    return true; 
                foreach (var subsystemName in loadedStateDict.Keys) 
                { 
                    if (!_stateDict.ContainsKey(subsystemName)) 
                        continue; 
                    var stateItem = loadedStateDict[subsystemName]; 
                    var subsystem = (IStateSubsystem)Application.GetSubsystem(subsystemName); 
                    if (stateItem.Value != null) 
                    { 
                        try 
                        { 
                            subsystem.RestoreState(stateItem.Value); 
                        } 
                        catch (Exception ex) 
                        { 
                            Logger.LogError(Message.SyncSubsystemRestoreStateFailed, ex); 
                            return false; 
                        } 
                    } 
                    else 
                    { 
                        subsystem.ResetState(false); 
                        stateItem.Value = subsystem.GetState(); 
                    } 
                    stateItem.Synchronized = false; 
                    _stateDict[subsystemName] = stateItem; 
                } 
                _isStateInitial = false; 
                return true; 
            } 
        } 
        private void SaveSubsystemState(string subsystemName, object subsystemState) 
        { 
            lock (s_stateSync) 
            { 
                var oldState = _stateDict[subsystemName].Value; 
                _stateDict[subsystemName].Value = subsystemState; 
                if ((oldState == null && subsystemState == null) || 
                    (subsystemState != null && subsystemState.Equals(oldState))) 
                    return; 
                SaveState(); 
            } 
        } 
        void IScannerInteractionChannel.ResetState(string reason) 
        { 
            Logger.LogInfo(Message.SyncResetState, reason); 
            lock (s_stateSync) 
            { 
                try 
                { 
                    BackupCurrentState(); 
                    var subsystemNames = new string[_stateDict.Keys.Count]; 
                    _stateDict.Keys.CopyTo(subsystemNames, 0); 
                    foreach (var subsystemName in subsystemNames) 
                    { 
                        var subsystem = (IStateSubsystem)Application.GetSubsystem(subsystemName); 
                        subsystem.ResetState(false); 
                        var state = subsystem.GetState(); 
                        _stateDict[subsystemName] = new StateItem(subsystemName) { Value = state }; 
                    } 
                    _isStateInitial = true; 
                    SaveState(); 
                    Logger.LogInfo(Message.SyncResetStateSucceeded); 
                } 
                catch (Exception ex) 
                { 
                    Logger.LogError(Message.SyncStateResetFailed, ex); 
                } 
            } 
        } 
        private void BackupCurrentState() 
        { 
            try 
            { 
                var stateDirectory = _fileSystemManager.GetDataDirectoryPath(FileType.State); 
                var stateBackupFileName = FileUtils.GetUniqueName(stateDirectory, STATE_FILE_NAME, STATE_FILE_EXT, 3); 
                stateBackupFileName = Path.Combine(stateDirectory, stateBackupFileName); 
                _fileSystemManager.SafeSerialization( 
                    _stateDict, _stateSerializationFormatter, stateBackupFileName, false, true); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.SyncBackupCurrentStateFailed, ex); 
            } 
        } 
        private void SaveState() 
        { 
            try 
            { 
                if (!_stateLoaded) 
                    return; 
                if (_fileSystemManager.SafeSerialization( 
                    _stateDict, _stateSerializationFormatter, _stateFilePath, true, true)) 
                { 
                    Logger.LogVerbose(Message.SyncStateSaved); 
                } 
                else 
                { 
                    Logger.LogError(Message.SyncStateSaveFailed); 
                } 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.SyncStateSaveError, ex); 
            } 
        } 
        #endregion 
        #endregion 
        #region IScannersInfo Members 
        public string LocalScannerSerialNumber 
        { 
            get 
            { 
                return _scannerManager.SerialNumber; 
            } 
        } 
        public string RemoteScannerSerialNumber 
        { 
            get 
            { 
                return IsRemoteScannerConnected ? _remoteScannerInfo.SerialNumber : null; 
            } 
        } 
        public List<ScannerInfo> GetScannerInfos() 
        { 
            var infos = new List<ScannerInfo> { _localScannerInfo }; 
            if (IsRemoteScannerConnected) 
                infos.Add(_remoteScannerInfo); 
            foreach (var scannerInfo in _votingResultManager.VotingResults.GetScannerInfos()) 
            { 
                if (!infos.Contains(scannerInfo)) 
                    infos.Add(scannerInfo); 
            } 
            return infos; 
        } 
        #endregion 
        #region IDisposable Members 
        public override void Dispose() 
        { 
            base.Dispose(); 
            StopPing(); 
            CloseIncomingInteractionChannel(); 
            DisposeRemoteScannerCommunication(); 
        } 
        #endregion 
    } 
}

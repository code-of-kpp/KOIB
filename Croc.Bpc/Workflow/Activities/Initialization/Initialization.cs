using System; 
using System.Threading; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Keyboard; 
using Croc.Bpc.Synchronization; 
using Croc.Bpc.Voting; 
using Croc.Core.Utils.Threading; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.Initialization 
{ 
    [Serializable] 
    public class InitializationActivity : BpcCompositeActivity 
    { 
        [NonSerialized] 
        private SourceDataFileDescriptor _sourceDataFileDescriptor; 
        public SourceDataFileDescriptor SourceDataFileDescriptor 
        { 
            get 
            { 
                return _sourceDataFileDescriptor; 
            } 
        } 
        [NonSerialized] 
        private SourceData _sourceDataFromFile; 
        public SourceData SourceDataFromFile 
        { 
            get 
            { 
                return _sourceDataFromFile; 
            } 
        } 
        public DateTime ElectionDate 
        { 
            get 
            { 
                return _electionManager.SourceData.ElectionDate; 
            } 
        } 
        #region Определение роли сканера 
        [NonSerialized] 
        private EventWaitHandleEx _remoteScannerRoleDefined; 
        [NonSerialized] 
        private EventWaitHandleEx _remoteScannerBecameMaster; 
        public WaitHandle RemoteScannerBecameMaster 
        { 
            get 
            { 
                return _remoteScannerBecameMaster; 
            } 
        } 
        [NonSerialized] 
        private EventWaitHandleEx _remoteScannerBecameMasterOrSlave; 
        public WaitHandle RemoteScannerBecameMasterOrSlave 
        { 
            get 
            { 
                return _remoteScannerBecameMasterOrSlave; 
            } 
        } 
        private volatile ScannerRole _remoteScannerRole; 
        protected override void Initialize(WorkflowExecutionContext context) 
        { 
            base.Initialize(context); 
            _remoteScannerRoleDefined = new EventWaitHandleEx(false, true, this); 
            _remoteScannerBecameMaster = new EventWaitHandleEx(false, true, this); 
            _remoteScannerBecameMasterOrSlave = new EventWaitHandleEx(false, true, this); 
            _remoteScannerRole = ScannerRole.Undefined; 
            _syncManager.RemoteScanner.ScannerRoleChanged += RemoteScanner_ScannerRoleChanged; 
            RemoteScanner_ScannerRoleChanged(this, EventArgs.Empty); 
        } 
        protected override void Uninitialize(WorkflowExecutionContext context) 
        { 
            _syncManager.RemoteScanner.ScannerRoleChanged -= RemoteScanner_ScannerRoleChanged; 
            _remoteScannerRoleDefined.Dispose(); 
            _remoteScannerBecameMaster.Dispose(); 
            _remoteScannerBecameMasterOrSlave.Dispose(); 
            base.Uninitialize(context); 
        } 
        private void RemoteScanner_ScannerRoleChanged(object sender, EventArgs e) 
        { 
            _remoteScannerRole = _syncManager.RemoteScanner.ScannerRole; 
            _logger.LogVerbose(Message.WorkflowRemoteScannerTakeRole, _remoteScannerRole); 
            _remoteScannerRoleDefined.GetAccess(this); 
            _remoteScannerBecameMaster.GetAccess(this); 
            _remoteScannerBecameMasterOrSlave.GetAccess(this); 
            switch (_remoteScannerRole) 
            { 
                case ScannerRole.Undefined: 
                    _remoteScannerRoleDefined.Reset(); 
                    _remoteScannerBecameMaster.Reset(); 
                    _remoteScannerBecameMasterOrSlave.Reset(); 
                    break; 
                case ScannerRole.Slave: 
                    _remoteScannerRoleDefined.Set(); 
                    _remoteScannerBecameMaster.Reset(); 
                    _remoteScannerBecameMasterOrSlave.Set(); 
                    break; 
                case ScannerRole.Master: 
                    _remoteScannerRoleDefined.Set(); 
                    _remoteScannerBecameMaster.Set(); 
                    _remoteScannerBecameMasterOrSlave.Set(); 
                    break; 
                case ScannerRole.RemoteScannerMasterToo: 
                    _remoteScannerRoleDefined.Set(); 
                    _remoteScannerBecameMaster.Reset(); 
                    _remoteScannerBecameMasterOrSlave.Reset(); 
                    break; 
                case ScannerRole.Failed: 
                    _remoteScannerRoleDefined.Set(); 
                    _remoteScannerBecameMaster.Reset(); 
                    _remoteScannerBecameMasterOrSlave.Reset(); 
                    break; 
            } 
        } 
        private void ResetRemoteScannerRoleInfo() 
        { 
            _remoteScannerRole = ScannerRole.Undefined; 
            _remoteScannerBecameMaster.GetAccess(this); 
            _remoteScannerBecameMasterOrSlave.GetAccess(this); 
            _remoteScannerBecameMaster.Reset(); 
            _remoteScannerBecameMasterOrSlave.Reset(); 
        } 
        public void SetScannerRoleToRemoteScannerMasterToo(WorkflowExecutionContext context) 
        { 
            _syncManager.ScannerRole = ScannerRole.RemoteScannerMasterToo; 
        } 
        public void SetScannerRoleToFailed(WorkflowExecutionContext context) 
        { 
            _syncManager.ScannerRole = ScannerRole.Failed; 
        } 
        public NextActivityKey SetRoleToMaster( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _syncManager.ScannerRole = ScannerRole.Master; 
            if (!_syncManager.IsRemoteScannerConnected) 
                return BpcNextActivityKeys.Yes; 
            _scannerManager.SetIndicator(CommonActivity.SYNCHRONIZATION_INDICATOR_TEXT); 
            _logger.LogVerbose(Message.WorkflowMasterWaitForRemoteScannerRoleDefined); 
            context.Sleep(300); 
            while (true) 
            { 
                context.WaitOne(_remoteScannerRoleDefined); 
                switch (_remoteScannerRole) 
                { 
                    case ScannerRole.Slave: 
                        return BpcNextActivityKeys.Yes; 
                    case ScannerRole.Master: 
                    case ScannerRole.RemoteScannerMasterToo: 
                        ResetRemoteScannerRoleInfo(); 
                        return BpcNextActivityKeys.No; 
                    case ScannerRole.Failed: 
                        break; 
                } 
                _logger.LogVerbose(Message.WorkflowMasterWaitForRemoteScannerRoleDefined); 
                context.Sleep(1000); 
            } 
        } 
        public NextActivityKey SetRoleToSlave( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _syncManager.ScannerRole = ScannerRole.Slave; 
            if (!_syncManager.IsRemoteScannerConnected) 
                return BpcNextActivityKeys.No; 
            _scannerManager.SetIndicator(CommonActivity.SYNCHRONIZATION_INDICATOR_TEXT); 
            _logger.LogVerbose(Message.WorkflowSlaveWaitForRemoteScannerRoleDefined); 
            context.Sleep(300); 
            while (true) 
            { 
                context.WaitOne(_remoteScannerRoleDefined); 
                switch (_remoteScannerRole) 
                { 
                    case ScannerRole.Slave: 
                        return BpcNextActivityKeys.No; 
                    case ScannerRole.Master: 
                        return BpcNextActivityKeys.Yes; 
                    case ScannerRole.RemoteScannerMasterToo: 
                        break; 
                    case ScannerRole.Failed: 
                        return BpcNextActivityKeys.No; 
                } 
                _logger.LogVerbose(Message.WorkflowSlaveWaitForRemoteScannerRoleDefined); 
                context.Sleep(1000); 
            } 
        } 
        #endregion 
        #region Реализация действий 
        public NextActivityKey LoadState( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _scannerManager.SetIndicator("Загрузка состояния..."); 
            return _syncManager.LoadState() 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey ResetState( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var reason = parameters.GetParamValue<string>("Reason"); 
            _syncManager.ResetState(string.IsNullOrEmpty(reason) ? "не определено" : reason); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey WaitForRemoteScanner( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            const string WAIT_DATANAME = "Wait"; 
            const string WAIT_DATA = "IAmHere"; 
            _syncManager.ScannerRole = ScannerRole.Undefined; 
            if (!_syncManager.IsRemoteScannerConnected) 
                return context.DefaultNextActivityKey; 
            _syncManager.SynchronizationEnabled = false; 
            _scannerManager.SetIndicator(CommonActivity.SYNCHRONIZATION_INDICATOR_TEXT); 
            _syncManager.RemoteScanner.PutData(WAIT_DATANAME, WAIT_DATA); 
            var waitDone = new ManualResetEvent(false); 
            ThreadUtils.StartBackgroundThread( 
                () => 
                { 
                    try 
                    { 
                        _logger.LogVerbose(Message.WorkflowInitStartWaitForRemoteScanner); 
                        _syncManager.GetDataTransmittedFromRemoteScanner(WAIT_DATANAME, context); 
                        waitDone.Set(); 
                    } 
                    catch (ActivityExecutionInterruptException) 
                    { 
                        return; 
                    } 
                }); 
            while (true) 
            { 
                if (context.WaitOne(waitDone, TimeSpan.FromSeconds(10))) 
                { 
                    return context.DefaultNextActivityKey; 
                } 
                _logger.LogVerbose(Message.WorkflowInitStartNoticeRemoteScannerAboutWait); 
                _syncManager.RemoteScanner.NoticeAboutWaitForInitialization(); 
            } 
        } 
        public NextActivityKey SearchSourceDataFile( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _syncManager.SynchronizationEnabled = false; 
            _scannerManager.SetIndicator("Поиск Flash..."); 
            if (!_electionManager.FindSourceDataFile(_remoteScannerBecameMaster, out _sourceDataFileDescriptor)) 
            { 
                return BpcNextActivityKeys.No; 
            } 
            _logger.LogInfo(Message.Election_SourceDataFileFound, _sourceDataFileDescriptor.FilePath); 
            return BpcNextActivityKeys.Yes; 
        } 
        public NextActivityKey SetDateTimeOnSlave( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (_syncManager.IsRemoteScannerConnected) 
                _syncManager.RemoteScanner.SetSystemTime(DateTime.UtcNow); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey LoadSourceData( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (_syncManager.ScannerRole == ScannerRole.Master) 
            { 
                _scannerManager.SetIndicator("Загрузка исходных данных..."); 
                var result = _electionManager.LoadSourceDataFromFile( 
                    _sourceDataFileDescriptor, true, out _sourceDataFromFile); 
                if (!result) 
                    return BpcNextActivityKeys.No; 
                _logger.LogInfo(Message.Election_SourceDataLoaded); 
            } 
            return BpcNextActivityKeys.Yes; 
        } 
        public NextActivityKey IsSourceDataCorrect( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _syncManager.IsSourceDataCorrect || _syncManager.RemoteScanner.IsSourceDataCorrect 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey IsSdElectionModeTraining( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _electionManager.SourceData.ElectionMode == ElectionMode.Training 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey CheckElectionDate( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var edc = _electionManager.IsElectionDay(); 
            switch (edc) 
            { 
                case ElectionDayСomming.NotComeYet: 
                    return BpcNextActivityKeys_VotingTime.ElectionDayHasNotCome; 
                case ElectionDayСomming.ItsElectionDay: 
                case ElectionDayСomming.ItsExtraElectionDay: 
                    return BpcNextActivityKeys_VotingTime.ElectionDayNow; 
                case ElectionDayСomming.AlreadyPassed: 
                    return BpcNextActivityKeys_VotingTime.ElectionDayPassed; 
                default: 
                    throw new ArgumentOutOfRangeException(); 
            } 
        } 
        public NextActivityKey SetElectionModeToTraining( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _electionManager.SourceData.ElectionMode = ElectionMode.Training; 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey SetElectionModeToReal( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _electionManager.SourceData.ElectionMode = ElectionMode.Real; 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey SetSourceDataIsCorrect( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _electionManager.IsSourceDataCorrect = true; 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey Synchronize( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _logger.LogInfo(Message.WorkflowStartSynchronization); 
            _scannerManager.SetIndicator(CommonActivity.SYNCHRONIZATION_INDICATOR_TEXT); 
            _syncManager.StartStateSynchronization(true); 
            var synchronizationSucceeded = _syncManager.WaitForSynchronizationFinished(context); 
            if (!synchronizationSucceeded) 
                return BpcNextActivityKeys.No; 
            var res = _workflowManager.GoToStateActivity(); 
            if (res) 
                context.Sleep(Timeout.Infinite); 


            return BpcNextActivityKeys.Yes; 
        } 
        #endregion 
    } 
}

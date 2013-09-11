using System; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Synchronization; 
using Croc.Bpc.Voting; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.Initialization 
{ 
    [Serializable] 
    public class CheckConflictActivity : BpcCompositeActivity 
    { 
        [NonSerialized] 
        private SourceDataFileDescriptor _sourceDataFileDescriptor; 
        public SourceDataFileDescriptor SourceDataFileDescriptor 
        { 
            get 
            { 
                return _sourceDataFileDescriptor; 
            } 
            set 
            { 
                _sourceDataFileDescriptor = value; 
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
            set 
            { 
                _sourceDataFromFile = value; 
            } 
        } 
        #region Ожидание и принятие решения о наличии конфликта 
        private const string HASCONFLICT_DATANAME = "HasConflict"; 
        public NextActivityKey WaitForMasterDecision( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            NextActivityKey result; 
            _scannerManager.SetIndicator(CommonActivity.SYNCHRONIZATION_INDICATOR_TEXT); 
            _logger.LogInfo(Message.WorkflowWaitForMasterDecision); 
            var hasConflict = _syncManager.GetDataTransmittedFromRemoteScanner(HASCONFLICT_DATANAME, context); 
            if (hasConflict == null) 
            { 
                _logger.LogInfo(Message.WorkflowCannotDetectConflict); 
                result = BpcNextActivityKeys.No; 
            } 
            else if ((bool) hasConflict) 
            { 
                _logger.LogInfo(Message.WorkflowMasterDetectConflict); 
                result = BpcNextActivityKeys.No; 
            } 
            else 
            { 
                _logger.LogInfo(Message.WorkflowMasterDetectNoConflict); 
                result = BpcNextActivityKeys.Yes; 
            } 
            return result; 
        } 
        public NextActivityKey TransmitDecisionToSlaveScanner( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var hasConflict = parameters.GetParamValue<bool>("HasConflict"); 
            if (hasConflict) 
                _logger.LogInfo(Message.WorkflowHasConflict); 
            if (_syncManager.ScannerRole == ScannerRole.Master && _syncManager.IsRemoteScannerConnected) 
            { 
                _syncManager.RemoteScanner.PutData(HASCONFLICT_DATANAME, hasConflict); 
            } 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey ReplaseStateSdToFileSd( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (IsStateSdEqualsFileSd()) 
                return BpcNextActivityKeys.Yes; 
            return _electionManager.SetSourceData(_sourceDataFromFile, _sourceDataFileDescriptor) 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        #endregion 
        #region Различные проверки 
        public NextActivityKey IsStateRestored( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return 
                !_syncManager.IsStateInitial && 
                _electionManager.SourceData != null 
                    ? BpcNextActivityKeys.Yes 
                    : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey IsStateSdEqualsFileSd( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return IsStateSdEqualsFileSd() 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        private bool IsStateSdEqualsFileSd() 
        { 
            return 
                _sourceDataFromFile != null && 
                _electionManager.SourceData != null && 
                _sourceDataFromFile.Equals(_electionManager.SourceData); 
        } 
        public NextActivityKey IsSlaveStateSdEqualsFileSd( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return 
                _sourceDataFromFile != null && 
                string.CompareOrdinal( 
                    _sourceDataFromFile.HashCode, 
                    _syncManager.RemoteScanner.SourceDataHashCode) == 0 
                    ? BpcNextActivityKeys.Yes 
                    : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey IsElectionDayNowInStateSd( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return IsIsElectionDayOrExtra(_electionManager.IsElectionDay()) 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey IsElectionDayNowInFileSd( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return IsIsElectionDayOrExtra(_electionManager.IsElectionDay(_sourceDataFromFile)) 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey IsElectionDayNowInStateSdOnSlaveScanner( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return IsIsElectionDayOrExtra(_syncManager.RemoteScanner.IsElectionDay) 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey SlaveScannerExistsAndHasRestoredState( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return 
                _syncManager.IsRemoteScannerConnected && 
                !_syncManager.RemoteScanner.IsStateInitial && 
                !string.IsNullOrEmpty(_syncManager.RemoteScanner.SourceDataHashCode) 
                    ? BpcNextActivityKeys.Yes 
                    : BpcNextActivityKeys.No; 
        } 
        #endregion 
        #region Сброс ПО 
        [NonSerialized] 
        private bool _needResetSoftOnMaster; 
        [NonSerialized] 
        private bool _needResetSoftOnSlave; 
        public NextActivityKey RememberToResetSoftOnMaster( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _needResetSoftOnMaster = true; 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey RememberToResetSoftOnSlave( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _needResetSoftOnSlave = true; 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey NeedToResetSoft( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _needResetSoftOnMaster || _needResetSoftOnSlave 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey ResetSoft( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (_needResetSoftOnSlave) 
            { 
                _logger.LogInfo(Message.WorkflowResetSoftOnSlaveBecauseConflictDetected); 
                _syncManager.RemoteScanner.ResetSoft(ResetSoftReason.ConflictDetected, true, false); 
            } 
            if (_needResetSoftOnMaster) 
            { 
                _logger.LogInfo(Message.WorkflowResetSoftOnMasterBecauseConflictDetected); 
                _syncManager.ResetSoft(ResetSoftReason.ConflictDetected, false, false); 
            } 
            return context.DefaultNextActivityKey; 
        } 
        #endregion 
    } 
}

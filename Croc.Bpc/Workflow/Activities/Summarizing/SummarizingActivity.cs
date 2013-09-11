using System; 
using System.Collections.Generic; 
using System.Collections.Specialized; 
using Croc.Bpc.Diagnostics; 
using Croc.Core.Diagnostics; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.Summarizing 
{ 
    [Serializable] 
    public class SummarizingActivity : ElectionEnumeratorActivity 
    { 
        [NonSerialized] 
        private readonly List<string> _generatedProtocolElectionIds = new List<string>(); 
        [NonSerialized] 
        private string _addInfoEnteredOnMasterElectionId; 
        public object[] ElectionNumsWithoutProtocolsPhraseParameters 
        { 
            get 
            { 
                var paramList = new List<object>(); 
                var elections = _electionManager.SourceData.Elections; 
                for (var i = 0; i < elections.Length; i++) 
                { 
                    if (!_generatedProtocolElectionIds.Contains(elections[i].ElectionId)) 
                        paramList.Add(i + 1); 
                } 
                return paramList.ToArray(); 
            } 
        } 
        public ListDictionary ElectionProtocolParameters 
        { 
            get 
            { 
                var protocolParams = new ListDictionary 
                                         { 
                                             {"Election", _currentElection}, 
                                             {"withResults", true} 
                                         }; 
                return protocolParams; 
            } 
        } 
        public NextActivityKey ResetGeneratedProtocolElectionIds( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _generatedProtocolElectionIds.Clear(); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey IsProtocolsGeneratedForAllElections( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _electionManager.SourceData.Elections.Length == _generatedProtocolElectionIds.Count 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey IsProtocolGeneratedForCurrentElection( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _generatedProtocolElectionIds.Contains(_currentElection.ElectionId) 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey GenerateVotingResultProtocolOnSlave( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var election = _electionManager.SourceData.GetElectionByNum(_addInfoEnteredOnMasterElectionId); 
            if (election == null) 
                _logger.LogWarning(Message.WorkflowElectionWithIdFromMasterNotFound, _addInfoEnteredOnMasterElectionId); 
            _votingResultManager.GenerateVotingResultProtocol(election); 
            return context.DefaultNextActivityKey; 
        } 
        #region Ожидание и извещение о завершении ввода доп. сведений 
        private const string ADDINFOENTERED_DATANAME = "AddInfoEntered"; 
        private const string ADDINFOENTERINGFINISHED_DATA = "AddInfoEnteringFinished"; 
        public NextActivityKey NoticeSlaveAboutAddInfoEnteringFinished( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _syncManager.RemoteScanner.PutData(ADDINFOENTERED_DATANAME, ADDINFOENTERINGFINISHED_DATA); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey NoticeSlaveAboutAddInfoEnteredAndGenerateVotingResultProtocol( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _syncManager.RemoteScanner.PutData(ADDINFOENTERED_DATANAME, _currentElection.ElectionId); 
            _votingResultManager.GenerateVotingResultProtocol(_currentElection); 
            _generatedProtocolElectionIds.Add(_currentElection.ElectionId); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey WaitForAddInfoEnteredOnMaster( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _scannerManager.SetIndicator(CommonActivity.SYNCHRONIZATION_INDICATOR_TEXT); 


            _logger.LogVerbose(Message.WorkflowWaitForAddInfoEnteredOnMaster); 
            var addInfoEnteredRes = _syncManager.GetDataTransmittedFromRemoteScanner(ADDINFOENTERED_DATANAME, context); 
            if (addInfoEnteredRes == null) 
            { 
                _logger.LogVerbose(Message.WorkflowCannotDetectAddInfoEnteredOnMaster); 
                return BpcNextActivityKeys.No; 
            } 
            _addInfoEnteredOnMasterElectionId = (string)addInfoEnteredRes; 
            if (string.CompareOrdinal(ADDINFOENTERINGFINISHED_DATA, _addInfoEnteredOnMasterElectionId) == 0) 
            { 
                _logger.LogVerbose(Message.WorkflowAddInfoEnteringFinished); 
                return BpcNextActivityKeys.No; 
            } 
            _logger.LogVerbose(Message.WorkflowAddInfoEnteredForElection, _addInfoEnteredOnMasterElectionId); 
            return BpcNextActivityKeys.Yes; 
        } 
        #endregion 
    } 
}

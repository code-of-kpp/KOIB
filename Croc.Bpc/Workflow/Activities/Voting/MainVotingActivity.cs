using System; 
using System.Threading; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Scanner; 
using Croc.Bpc.Voting; 
using Croc.Core.Extensions; 
using Croc.Core.Utils.Threading; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.Voting 
{ 
    [Serializable] 
    public class MainVotingActivity : ScanningActivity 
    { 
        protected override void Initialize(WorkflowExecutionContext context) 
        { 
            base.Initialize(context); 
            _notMoreThen10MinToVotingEndEvent = new EventWaitHandleEx(false, true, this); 
            if (_electionManager.SourceData.ElectionMode == ElectionMode.Real) 
            { 
                _stopMonitorVotingEndEvent = new AutoResetEvent(false); 
                _monitorVotingEndThread = ThreadUtils.StartBackgroundThread(MonitorVotingEndThreadMethod); 
            } 
        } 
        protected override void Uninitialize(WorkflowExecutionContext context) 
        { 
            _notMoreThen10MinToVotingEndEvent.Dispose(); 
            if (_stopMonitorVotingEndEvent != null && _monitorVotingEndThread != null) 
            { 
                _stopMonitorVotingEndEvent.Set(); 
                _monitorVotingEndThread.SafeAbort(100); 
                _stopMonitorVotingEndEvent = null; 
                _monitorVotingEndThread = null; 
            } 
            base.Uninitialize(context); 
        } 
        public string HelpSoundForEndMainVoiting 
        { 
            get 
            { 
                if (_electionManager.SourceData.VotingModeExists(VotingMode.Portable)) 
                    return "go_to_portable_mode_help_message"; 
                return "empty"; 
            } 
        } 
        public NextActivityKey SetVotingModeToMain( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _electionManager.CurrentVotingMode = VotingMode.Main; 
            _votingResultManager.VotingResults.SetCounterValueKeys( 
                new[] 
                    { 
                        new VoteKey 
                            { 
                                VotingMode = VotingMode.Main, 
                                BlankType = BlankType.All 
                            } 
                    }); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey CanGoToPortableVotingMode( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return 
                _electionManager.IsElectionDay() == ElectionDayСomming.ItsElectionDay && 
                _electionManager.SourceData.ElectionMode == ElectionMode.Real && 
                !_electionManager.SourceData.IsVotingModeExpired(VotingMode.Main) 
                ? BpcNextActivityKeys.No : BpcNextActivityKeys.Yes; 
        } 
        public TimeSpan PortableVotingModeStartTime 
        { 
            get 
            { 
                return _electionManager.SourceData.GetVotingModeStartTime(VotingMode.Portable); 
            } 
        } 
        #region Включение/выключение сканирования 
        protected override ScannerLampsRegime LampsRegime 
        { 
            get 
            { 
                return ScannerLampsRegime.GreenOn; 
            } 
        } 
        #endregion 
        #region Ожидание листа 
        public TimeSpan TimeToVotingEnd 
        { 
            get 
            { 
                return _electionManager.SourceData.GetVotingEndRemainingTime().RoundMinutes(); 
            } 
        } 
        [NonSerialized] 
        private EventWaitHandleEx _notMoreThen10MinToVotingEndEvent; 
        public WaitHandle NotMoreThen10MinToVotingEndEvent 
        { 
            get 
            { 
                return _notMoreThen10MinToVotingEndEvent; 
            } 
        } 
        private Thread _monitorVotingEndThread; 
        private AutoResetEvent _stopMonitorVotingEndEvent; 
        private void MonitorVotingEndThreadMethod() 
        { 
            while (true) 
            { 
                var votingEndRemainingTime = _electionManager.SourceData.GetVotingEndRemainingTime(); 
                if (votingEndRemainingTime - TimeSpan.FromMinutes(10) <= TimeSpan.Zero) 
                { 
                    _logger.LogVerbose(Message.WorkflowNotMoreThen10MinToVotingEnd); 
                    _notMoreThen10MinToVotingEndEvent.GetAccess(this); 
                    _notMoreThen10MinToVotingEndEvent.Set(); 
                    return; 
                } 
                if (_stopMonitorVotingEndEvent.WaitOne(TimeSpan.FromSeconds(10))) 
                    return;                 
            } 
        } 
        public NextActivityKey ResetNotMoreThen10MinToVotingEndEvent( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _notMoreThen10MinToVotingEndEvent.GetAccess(this); 
            _notMoreThen10MinToVotingEndEvent.Reset(); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey IsVotingTimeOver( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _electionManager.SourceData.GetVotingEndRemainingTime() == TimeSpan.Zero 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        #endregion 
        #region Обработка листа 
        protected override string NewSheetReceivedHandlerActivityName 
        { 
            get 
            { 
                return "MainVotingActivity.WaitSheetProcessed"; 
            } 
        } 
        protected override bool CanReceiveBulletin() 
        { 
            return !(// нельзя, если 
                _electionManager.IsElectionDay() == ElectionDayСomming.ItsElectionDay && 
                _electionManager.SourceData.ElectionMode == ElectionMode.Real && 
                _electionManager.SourceData.IsVotingModeExpired(VotingMode.Main)); 
        } 
        #endregion 
    } 
}

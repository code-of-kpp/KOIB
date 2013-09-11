using System; 
using System.Threading; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Synchronization; 
using Croc.Bpc.Voting; 
using Croc.Core; 
using Croc.Core.Extensions; 
using Croc.Core.Utils.Threading; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.Initialization 
{ 
    [Serializable] 
    public class ControlVotingStartActivity : BpcCompositeActivity 
    { 
        public TimeSpan ControlPeriod { get; set; } 
        public TimeSpan TrainingMinTime { get; set; } 
        public TimeSpan RealMinTime { get; set; } 
        public event EventHandler VotingWillSoonStart; 
        private volatile bool _votingWillSoonStartEventIsProcessed; 
        protected override void Initialize(WorkflowExecutionContext context) 
        { 
            base.Initialize(context); 
            ControlPeriod = TimeSpan.FromSeconds(20); 
            TrainingMinTime = TimeSpan.FromHours(2); 
            RealMinTime = TimeSpan.FromMinutes(10); 
        } 
        #region Запуск контроля начала голосования 
        public NextActivityKey CheckVotingDate( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var edc = _electionManager.IsElectionDay(); 
            switch (edc) 
            { 
                case ElectionDayСomming.NotComeYet: 
                    return BpcNextActivityKeys_VotingTime.ElectionDayHasNotCome; 
                case ElectionDayСomming.ItsElectionDay: 
                    return BpcNextActivityKeys_VotingTime.ElectionDayNow; 
                case ElectionDayСomming.ItsExtraElectionDay: 
                case ElectionDayСomming.AlreadyPassed: 
                    return BpcNextActivityKeys_VotingTime.ElectionDayPassed; 
                default: 
                    throw new ArgumentOutOfRangeException(); 
            } 
        } 
        public int DaysToVotingStart 
        { 
            get 
            { 
                var date = _electionManager.SourceData.ElectionDate.Date - 
                    _electionManager.SourceData.LocalTimeNow.Date; 
                return date.Days; 
            } 
        } 
        public NextActivityKey IsNoMoreThanMinTimeToVotingStart( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return IsNoMoreThanMinTimeToVotingStart() 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public TimeSpan VotingStartRemainingTime 
        { 
            get 
            { 
                return _electionManager.SourceData.GetVotingStartRemainingTime().RoundMinutes(); 
            } 
        } 
        public NextActivityKey StartControlThread( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (_controlThread == null) 
            { 
                _controlThread = ThreadUtils.StartBackgroundThread(ControlThread); 
            } 
            return BpcNextActivityKeys.Yes; 
        } 
        #endregion 
        #region Поток контроля начала голосования 
        private Thread _controlThread; 
        private void ControlThread() 
        { 
            try 
            { 
                _logger.LogInfo(Message.WorkflowControlThreadStarted, 
                                ControlPeriod.ToString(), TrainingMinTime.ToString(), RealMinTime.ToString()); 
                while (true) 
                { 
                    if (CoreApplication.Instance.ExitEvent.WaitOne(ControlPeriod)) 
                        return; 
                    if (_electionManager.SourceData == null) 
                        continue; 
                    if (!IsNoMoreThanMinTimeToVotingStart()) 
                        continue; 
                    _votingWillSoonStartEventIsProcessed = false; 
                    while (true) 
                    { 
                        if (// Режим выборов - "Боевые выборы" 
                            _electionManager.SourceData.ElectionMode == ElectionMode.Real && 
                            _electionManager.CurrentVotingMode >= VotingMode.Main) 
                        { 
                            _logger.LogInfo(Message.WorkflowControlThreadVotingAlreadyGoes); 
                            return; 
                        } 
                        _logger.LogInfo(Message.WorkflowControlThreadVotingWillSoonStart); 
                        VotingWillSoonStart.RaiseEvent(this); 
                        if (CoreApplication.Instance.ExitEvent.WaitOne(TimeSpan.FromMinutes(1))) 
                            return; 
                        if (_votingWillSoonStartEventIsProcessed) 
                            return; 
                    } 
                } 
            } 
            finally 
            { 
                _controlThread = null; 
            } 
        } 
        private bool IsNoMoreThanMinTimeToVotingStart() 
        { 
            var votingStartRemainingExactTime = _electionManager.SourceData.GetVotingStartRemainingTime(); 
            var minTime = _electionManager.SourceData.ElectionMode == ElectionMode.Training 
                              ? TrainingMinTime 
                              : RealMinTime; 
            return votingStartRemainingExactTime <= minTime; 
        } 
        #endregion 
        #region Обработка события "Скоро начало голосования" 
        public NextActivityKey HasVotingStarted( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _electionManager.SourceData.GetVotingStartRemainingTime() == TimeSpan.Zero 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey SetVotingWillSoonStartEventIsProcessed( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _votingWillSoonStartEventIsProcessed = true; 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey IsElectionModeTraining( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _electionManager.SourceData.ElectionMode == ElectionMode.Training 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey ResetUik 
            (WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _syncManager.ResetUik(ResetSoftReason.ControlVotingStartTriggered); 
            return context.DefaultNextActivityKey; 
        } 
        #endregion 
    } 
}

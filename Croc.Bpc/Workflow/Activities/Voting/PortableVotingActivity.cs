using System; 
using Croc.Bpc.Scanner; 
using Croc.Workflow.ComponentModel; 
using Croc.Bpc.Voting; 


namespace Croc.Bpc.Workflow.Activities.Voting 
{ 
    [Serializable] 
    public class PortableVotingActivity : ScanningActivity 
    { 
        public NextActivityKey SetVotingModeToPortable( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _electionManager.CurrentVotingMode = VotingMode.Portable; 
            _votingResultManager.VotingResults.SetCounterValueKeys( 
                new[] 
                    { 
                        new VoteKey 
                            { 
                                VotingMode = VotingMode.Main, 
                                BlankType = BlankType.All 
                            }, 
                        new VoteKey 
                            { 
                                VotingMode = VotingMode.Portable, 
                                BlankType = BlankType.All 
                            } 
                    } 
                ); 
            return context.DefaultNextActivityKey; 
        } 
        #region Включение/выключение сканирования 
        protected override ScannerLampsRegime LampsRegime 
        { 
            get 
            { 
                return ScannerLampsRegime.GreenBlinking; 
            } 
        } 
        #endregion 
        #region Обработка листа 
        protected override string NewSheetReceivedHandlerActivityName 
        { 
            get 
            { 
                return "PortableVotingActivity.WaitSheetProcessed"; 
            } 
        } 
        protected override bool CanReceiveBulletin() 
        { 
            return true; 
        } 
        #endregion 
    } 
}

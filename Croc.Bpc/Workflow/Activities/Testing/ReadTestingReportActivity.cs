using System; 
using System.Collections.Specialized; 
using Croc.Bpc.Voting; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.Testing 
{ 
    [Serializable] 
    public class ReadTestingReportActivity : ElectionEnumeratorActivity 
    { 
        public int TotalBulletinCount 
        { 
            get 
            { 
                var key = new VoteKey 
                { 
                    VotingMode = VotingMode.Test, 
                    BlankType = BlankType.AllButBad, 
                    BlankId = _currentBlankId 
                }; 
                return _votingResultManager.VotingResults.VotesCount(key); 
            } 
        } 
        public int ValidBulletinCount 
        { 
            get 
            { 
                var key = new VoteKey 
                { 
                    VotingMode = VotingMode.Test, 
                    BlankType = BlankType.Valid, 
                    BlankId = _currentBlankId 
                }; 
                return _votingResultManager.VotingResults.VotesCount(key); 
            } 
        } 
        public int NotValidBulletinCount 
        { 
            get 
            { 
                var key = new VoteKey 
                { 
                    VotingMode = VotingMode.Test, 
                    BlankType = BlankType.NotValid, 
                    BlankId = _currentBlankId 
                }; 
                return _votingResultManager.VotingResults.VotesCount(key); 
            } 
        } 
        public ListDictionary TestResultsPrintParameters 
        { 
            get  
            { 
                var parameters = new ListDictionary(); 
                parameters.Add("test", true); 
                return parameters; 
            } 
        } 
        public int BadBulletinCount 
        { 
            get 
            { 
                var key = new VoteKey 
                { 
                    VotingMode = VotingMode.Test, 
                    BlankType = BlankType.Bad, 
                    BlankId = _currentBlankId 
                }; 
                return _votingResultManager.VotingResults.VotesCount(key); 
            } 
        } 
        public NextActivityKey NeedSayBadBulletinsCount( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (_votingResultManager.AddBadBlankToCounterValue && BadBulletinCount > 0) 
                return BpcNextActivityKeys.Yes; 


            return BpcNextActivityKeys.No; 
        } 
    } 
}

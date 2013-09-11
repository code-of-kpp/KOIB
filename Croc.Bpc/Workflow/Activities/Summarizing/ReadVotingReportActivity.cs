using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using Croc.Bpc.Voting; 
using Croc.Workflow.ComponentModel; 
using Croc.Core.Diagnostics; 
namespace Croc.Bpc.Workflow.Activities.Summarizing 
{ 
    [Serializable] 
    public class ReadVotingReportActivity : ElectionParametrizedActivity 
    { 
        public int TotalBulletinCount 
        { 
            get 
            { 
                var key = new VoteKey() 
                { 
                    BlankType = BlankType.AllButBad, 
                    BlankId = BlankId 
                }; 
                return _votingResultManager.VotingResults.VotesCount(key); 
            } 
        } 
        public int ValidBulletinCount 
        { 
            get 
            { 
                var key = new VoteKey() 
                { 
                    BlankType = BlankType.Valid, 
                    BlankId = BlankId 
                }; 
                return _votingResultManager.VotingResults.VotesCount(key); 
            } 
        } 
        public int NotValidBulletinCount 
        { 
            get 
            { 
                var key = new VoteKey() 
                { 
                    BlankType = BlankType.NotValid, 
                    BlankId = BlankId 
                }; 
                return _votingResultManager.VotingResults.VotesCount(key); 
            } 
        } 
        public int ProcessedInMainBulletinCount 
        { 
            get 
            { 
                var key = new VoteKey() 
                { 
                    VotingMode = VotingMode.Main, 
                    BlankType = BlankType.AllButBad, 
                    BlankId = BlankId 
                }; 
                return _votingResultManager.VotingResults.VotesCount(key); 
            } 
        } 
        public int ProcessedInPortableBulletinCount 
        { 
            get 
            { 
                var key = new VoteKey() 
                { 
                    VotingMode = VotingMode.Portable, 
                    BlankType = BlankType.AllButBad, 
                    BlankId = BlankId 
                }; 
                return _votingResultManager.VotingResults.VotesCount(key); 
            } 
        } 
    } 
}

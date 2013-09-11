using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using Croc.Workflow.ComponentModel; 
using Croc.Core.Diagnostics; 
using Croc.Bpc.Voting; 
namespace Croc.Bpc.Workflow.Activities.Testing 
{ 
    [Serializable] 
    public class ReadCandidatesTestVoteCountActivity : CandidateEnumeratorActivity 
    { 
        public int CandidateVoteCount 
        { 
            get 
            { 
                var key = new VoteKey() 
                { 
                    VotingMode = VotingMode.Test, 
                    CandidateId = _currentCandidate.Id, 
                    ElectionNum = Election.ElectionId 
                }; 
                return _votingResultManager.VotingResults.VotesCount(key); 
            } 
        } 
        public NextActivityKey IsCandidateCanceled( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _currentCandidate.Disabled ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 
        } 
    } 
}

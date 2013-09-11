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
    public class ReadCandidatesVoteCountActivity : CandidateEnumeratorActivity 
    { 
        public int CandidateVoteCount 
        { 
            get 
            { 
                var key = new VoteKey() 
                { 
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

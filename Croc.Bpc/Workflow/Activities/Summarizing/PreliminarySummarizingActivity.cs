using System; 
using System.Collections.Specialized; 
using Croc.Bpc.Voting; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.Summarizing 
{ 
    [Serializable] 
    public class PreliminarySummarizingActivity : ElectionEnumeratorActivity 
    { 
        public NextActivityKey SetVotingModeToResults( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _electionManager.CurrentVotingMode = VotingMode.Results; 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey GeneratePreliminaryVotingResultProtocol( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _votingResultManager.GeneratePreliminaryVotingResultProtocol(); 
            return context.DefaultNextActivityKey; 
        } 
        public ListDictionary PreliminaryElectionProtocolParameters 
        { 
            get 
            { 
                return new ListDictionary {{"withResults", true}}; 
            } 
        } 
    } 
}

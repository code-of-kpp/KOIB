using System.Collections.Specialized; 
using Croc.Bpc.Voting; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.SystemMenu 
{ 
    public class PrintPreliminaryProtocolActivity : BpcCompositeActivity 
    { 
        public ListDictionary PreliminaryElectionProtocolParameters 
        { 
            get 
            { 
                return new ListDictionary { { "withResults", true } }; 
            } 
        } 
        public NextActivityKey IsVotingModeResults( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _electionManager.CurrentVotingMode == VotingMode.Results 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
    } 
}

using System; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.Summarizing 
{ 
    [Serializable] 
    public class SaveVotingResultsActivity : BpcCompositeActivity 
    { 
        public NextActivityKey FindFilePathToSaveVotingResultProtocol( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _votingResultManager.FindFilePathToSaveVotingResultProtocol() 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey SaveVotingResultProtocol( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _votingResultManager.SaveVotingResultProtocol() 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
    } 
}

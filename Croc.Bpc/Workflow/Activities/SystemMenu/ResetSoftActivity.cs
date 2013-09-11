using Croc.Bpc.Synchronization; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.SystemMenu 
{ 
    public class ResetSoftActivity : BpcCompositeActivity 
    { 
        public NextActivityKey ResetSoft( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _syncManager.ResetSoft(ResetSoftReason.ResetSoftFromSystemMenu, false, true); 
            return context.DefaultNextActivityKey; 
        } 
    } 
}

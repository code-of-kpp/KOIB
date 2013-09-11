using Croc.Bpc.Synchronization; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.SystemMenu 
{ 
    public class ResetUikActivity : BpcCompositeActivity 
    { 
        public NextActivityKey ResetUik 
            (WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _syncManager.ResetUik(ResetSoftReason.ResetUikFromSystemMenu); 
            return context.DefaultNextActivityKey; 
        } 
    } 
}

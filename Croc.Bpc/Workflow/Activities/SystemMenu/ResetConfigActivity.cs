using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.SystemMenu 
{ 
    public class ResetConfigActivity : BpcCompositeActivity 
    { 
        public NextActivityKey ResetConfig( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _configManager.ResetWorkingConfig(); 
            return context.DefaultNextActivityKey; 
        } 
    } 
}

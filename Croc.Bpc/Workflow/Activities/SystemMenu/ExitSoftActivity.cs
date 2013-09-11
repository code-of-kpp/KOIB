using Croc.Core; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.SystemMenu 
{ 
    public class ExitSoftActivity : BpcCompositeActivity 
    { 
        public NextActivityKey ExitSoft( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            CoreApplication.Instance.Exit(ApplicationExitType.Exit); 
            return context.DefaultNextActivityKey; 
        } 
    } 
}

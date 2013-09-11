using System; 
using Croc.Core; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Workflow.Runtime 
{ 
    public class WorkflowExecutionContextEventArgs : EventArgs 
    { 
        public readonly WorkflowExecutionContext Context; 
        public readonly Activity Activity; 
        public WorkflowExecutionContextEventArgs(WorkflowExecutionContext context, Activity activity) 
        { 
            CodeContract.Requires(context != null); 
            CodeContract.Requires(activity != null); 


            Context = context; 
            Activity = activity; 
        } 
    } 
}

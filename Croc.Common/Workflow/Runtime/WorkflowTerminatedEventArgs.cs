using Croc.Core; 
namespace Croc.Workflow.Runtime 
{ 
    public class WorkflowTerminatedEventArgs : WorkflowEventArgs 
    { 
        public readonly string Reason; 
        public WorkflowTerminatedEventArgs(WorkflowInstance wi, string reason) 
            : base(wi) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(reason)); 
            Reason = reason; 
        } 
    } 
}

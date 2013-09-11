using System; 
namespace Croc.Workflow.Runtime 
{ 
    public class WorkflowEventArgs : EventArgs 
    { 
        public WorkflowInstance WorkflowInstance 
        { 
            get; 
            private set; 
        } 
        public WorkflowEventArgs(WorkflowInstance wi) 
        { 
            WorkflowInstance = wi; 
        } 
    } 
}

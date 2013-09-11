using System; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Workflow.Runtime.Hosting 
{ 
    public abstract class WorkflowPersistenceService : WorkflowRuntimeService 
    { 
        public abstract WorkflowExecutionContext LoadWorkflowInstanceState(Guid instanceId); 
        public abstract void SaveWorkflowInstanceState(WorkflowExecutionContext context); 
    } 
}

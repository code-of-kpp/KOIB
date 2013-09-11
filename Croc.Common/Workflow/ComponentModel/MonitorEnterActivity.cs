using System; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    public class MonitorEnterActivity : MonitorActivity 
    { 
        public MonitorEnterActivity() 
        { 
            ExecutionMethodCaller = new ActivityExecutionMethodCaller("MonitorEnter", this); 
        } 
        internal NextActivityKey MonitorEnter( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            context.MonitorEnter(LockName); 
            return context.DefaultNextActivityKey; 
        } 
    } 
}

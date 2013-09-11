using System; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    public class MonitorExitActivity : MonitorActivity 
    { 
        public MonitorExitActivity() 
        { 
            ExecutionMethodCaller = new ActivityExecutionMethodCaller("MonitorExit", this); 
        } 
        internal NextActivityKey MonitorExit( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            context.MonitorExit(LockName); 
            return context.DefaultNextActivityKey; 
        } 
    } 
}

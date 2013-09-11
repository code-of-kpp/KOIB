using System; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    public class UnsubscribeFromEventActivity : EventHandlerActivity 
    { 
        public UnsubscribeFromEventActivity() 
        { 
            ExecutionMethodCaller = new ActivityExecutionMethodCaller("UnsubscribeFromEvent", this); 
        } 
        internal NextActivityKey UnsubscribeFromEvent( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            context.UnsubscribeFromEvent(Event, Handler); 
            return context.DefaultNextActivityKey; 
        } 
    } 
}

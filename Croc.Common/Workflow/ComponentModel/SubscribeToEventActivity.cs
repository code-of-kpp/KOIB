using System; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    public class SubscribeToEventActivity : EventHandlerActivity 
    { 
        public EventHandlingType HandlingType 
        { 
            get; 
            set; 
        } 
        public SubscribeToEventActivity() 
        {             
            ExecutionMethodCaller = new ActivityExecutionMethodCaller("SubscribeToEvent", this); 
        } 
        internal NextActivityKey SubscribeToEvent( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            context.SubscribeToEvent(Event, Handler, HandlingType); 
            return context.DefaultNextActivityKey; 
        } 
    } 
}

using System; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Действие, которое выполняет отписку от события 

    /// </summary> 

    [Serializable] 

    public class UnsubscribeFromEventActivity : EventHandlerActivity 

    { 

        public UnsubscribeFromEventActivity() 

        { 

            base.ExecutionMethodCaller = new ActivityExecutionMethodCaller("UnsubscribeFromEvent", this); 

        } 

 

 

        internal NextActivityKey UnsubscribeFromEvent( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // отписываемся от события 

            context.UnsubscribeFromEvent(Event, Handler); 

            return context.DefaultNextActivityKey; 

        } 

    } 

}



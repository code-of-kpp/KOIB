using System; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Действие, которое выполняет подписку на событие 

    /// </summary> 

    [Serializable] 

    public class SubscribeToEventActivity : EventHandlerActivity 

    { 

        /// <summary> 

        /// Тип обработки события 

        /// </summary> 

        public EventHandlingType HandlingType 

        { 

            get; 

            set; 

        } 

 

 

        public SubscribeToEventActivity() 

        {             

            base.ExecutionMethodCaller = new ActivityExecutionMethodCaller("SubscribeToEvent", this); 

        } 

 

 

        internal NextActivityKey SubscribeToEvent( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // подписываемся на событие 

            context.SubscribeToEvent(Event, Handler, HandlingType); 

            return context.DefaultNextActivityKey; 

        } 

    } 

}



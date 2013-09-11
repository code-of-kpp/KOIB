using System; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Абстрактное действие, которое определяет событие и его действие-обработчик 

    /// </summary> 

    [Serializable] 

    public abstract class EventHandlerActivity : Activity 

    { 

        /// <summary> 

        /// Событие 

        /// </summary> 

        public EventHolder Event 

        { 

            get; 

            set; 

        } 

 

 

        /// <summary> 

        /// Действие-обработчик события 

        /// </summary> 

        public Activity Handler 

        { 

            get; 

            set; 

        } 

 

 

        public EventHandlerActivity() 

        { 

            this.Tracking = false; 

        } 

    } 

}



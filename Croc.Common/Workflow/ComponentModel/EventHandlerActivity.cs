using System; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    public abstract class EventHandlerActivity : Activity 
    { 
        public EventHolder Event 
        { 
            get; 
            set; 
        } 
        public Activity Handler 
        { 
            get; 
            set; 
        } 
        protected EventHandlerActivity() 
        { 
            Tracking = false; 
        } 
    } 
}

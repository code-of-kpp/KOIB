using System; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    public abstract class MonitorActivity : Activity 
    { 
        public string LockName 
        { 
            get; 
            set; 
        } 
        protected MonitorActivity() 
        { 
            Tracking = false; 
        } 
    } 
}

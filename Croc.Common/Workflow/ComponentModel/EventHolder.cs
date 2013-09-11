using System; 
using System.Reflection; 
using Croc.Core; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    public class EventHolder 
    { 
        public EventInfo Event 
        { 
            get; 
            private set; 
        } 
        public object EventOwner 
        { 
            get; 
            private set; 
        } 
        public string EventName 
        { 
            get; 
            private set; 
        } 
        internal EventHolder() 
        { 
        } 
        public EventHolder(EventInfo ev, object eventOwner) 
        { 
            CodeContract.Requires(ev != null); 
            CodeContract.Requires(eventOwner != null); 
            Event = ev; 
            EventOwner = eventOwner; 
            EventName = string.Format("{0}#{1}", eventOwner.GetType().FullName, ev.Name); 
        } 
        public void AddEventHandler(Delegate handler) 
        { 
            Event.AddEventHandler(EventOwner, handler); 
        } 
        public void RemoveEventHandler(Delegate handler) 
        { 
            Event.RemoveEventHandler(EventOwner, handler); 
        } 
    } 
}

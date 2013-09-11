using System.Collections.Generic; 
namespace Croc.Core.Diagnostics 
{ 
    public class EventWriterTriplet 
    { 
        public IEventWriter Writer; 
        public IEventFormatter Formatter = new Default.EventFormatter(); 
        public bool Raw; 
        public List<IEventWriterFilter> Filters = new List<IEventWriterFilter>(); 
    } 
}

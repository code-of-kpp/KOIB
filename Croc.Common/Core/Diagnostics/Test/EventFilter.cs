using System; 
using System.Configuration; 
namespace Croc.Core.Diagnostics.Test 
{ 
    class EventFilter : IEventFilter 
    { 
        public void Init(NameValueConfigurationCollection props) 
        { 
            Console.WriteLine("EventFilter.Init"); 
            foreach (NameValueConfigurationElement prop in props) 
            { 
                Console.WriteLine("{0}={1}", prop.Name, prop.Value); 
            } 
        } 
        public bool Accepted(LoggerEvent logEvent) 
        { 
            Console.WriteLine("EventFilter.Accepted({0})", logEvent.Id); 
            return true; 
        } 
    } 
}

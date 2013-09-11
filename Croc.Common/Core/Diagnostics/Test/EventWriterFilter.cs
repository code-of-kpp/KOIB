using System; 

using System.Configuration; 

 

 

namespace Croc.Core.Diagnostics.Test 

{ 

    class EventWriterFilter : IEventWriterFilter 

    { 

        public void Init(NameValueConfigurationCollection props) 

        { 

            Console.WriteLine("EventWriterFilter.Init"); 

            foreach (NameValueConfigurationElement prop in props) 

            { 

                Console.WriteLine("{0}={1}", prop.Name, prop.Value); 

            } 

        } 

 

 

        public bool Accepted(EventWriterTriplet writerTriplet, LoggerEvent loggerEvent, string message) 

        { 

            Console.WriteLine("EventWriterFilter.Accepted({0}, {1})", loggerEvent.Id, message); 

            return true; 

        } 

    } 

}



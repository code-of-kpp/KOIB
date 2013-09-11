using System; 

using System.Configuration; 

 

 

namespace Croc.Core.Diagnostics.Test 

{ 

    class EventWriter : IEventWriter 

    { 

        public void Init(NameValueConfigurationCollection props) 

        { 

            Console.WriteLine("EventWriter.Init"); 

            foreach (NameValueConfigurationElement prop in props) 

            { 

                Console.WriteLine("{0}={1}", prop.Name, prop.Value); 

            } 

        } 

 

 

        public void Write(string uniqueLogId, string message) 

        { 

            Console.WriteLine("EventWriter.Write({0}, {1})", uniqueLogId, message); 

        } 

    } 

}



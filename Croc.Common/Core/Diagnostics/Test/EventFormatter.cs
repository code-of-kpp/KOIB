using System; 
using System.Configuration; 
namespace Croc.Core.Diagnostics.Test 
{ 
    class EventFormatter : IEventFormatter 
    { 
        private string _fieldId = LoggerEvent.MESSAGE_PROPERTY; 
        public void Init(NameValueConfigurationCollection props) 
        { 
            Console.WriteLine("EventFormatter.Init"); 
            foreach (NameValueConfigurationElement prop in props) 
            { 
                Console.WriteLine("{0}={1}", prop.Name, prop.Value); 
            } 
            if(props["format"] != null && !String.IsNullOrEmpty(props["format"].Value)) 
            { 
                _fieldId = props["format"].Value; 
            } 
        } 
        public string Format(LoggerEvent loggerEvent) 
        { 
            Console.WriteLine("EventFormatter.Format({0}, {1})", _fieldId, loggerEvent.Id); 
            return loggerEvent[_fieldId].ToString(); 
        } 
    } 
}

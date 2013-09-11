using System; 
using System.Collections.Generic; 
using System.Configuration; 
using Croc.Core.Diagnostics; 
namespace Croc.Bpc.Diagnostics 
{ 
    public class MessageDublicatesFilter : IEventWriterFilter 
    { 
        private const string MESSAGE_TYPES_PARAMETER_NAME = "MessageTypes"; 
        private string _lastMessageText; 
        private List<Message> _filteredTypes; 
        public void Init(NameValueConfigurationCollection props) 
        { 
            _filteredTypes = new List<Message>(); 
            var types = props[MESSAGE_TYPES_PARAMETER_NAME].Value.Split(';'); 
            foreach (var type in types) 
            { 
                if (Enum.IsDefined(typeof(Message), type)) 
                    _filteredTypes.Add((Message) Enum.Parse(typeof (Message), type)); 
            } 
        } 
        public bool Accepted(EventWriterTriplet writerTriplet, LoggerEvent loggerEvent, string message) 
        { 
            var newMessage = loggerEvent.Properties[LoggerEvent.MESSAGE_PROPERTY].ToString(); 
            bool asseptResult = false; 
            if (!loggerEvent.Properties.ContainsKey(BpcLoggerExtensions.MESSAGEID_PROPERTY)) 
                asseptResult = true; 
            else if (!_filteredTypes.Contains((Message)loggerEvent.Properties[BpcLoggerExtensions.MESSAGEID_PROPERTY])) 
                asseptResult = true; 
            else if (writerTriplet.Raw) 
            { 
                newMessage = string.Format(newMessage, (object[])loggerEvent[LoggerEvent.PARAMETERS_PROPERTY]); 
                asseptResult = _lastMessageText != newMessage; 
            } 
            else if (!writerTriplet.Raw && _lastMessageText != newMessage) 
                asseptResult = true; 
            _lastMessageText = newMessage; 
            return asseptResult; 
        } 
    } 
}

using System.Configuration; 
using Croc.Core.Utils.Text; 
namespace Croc.Core.Diagnostics.Default 
{ 
    public class EventFormatter : IEventFormatter 
    { 
        public string Format(LoggerEvent loggerEvent) 
        { 
            var textBuilder = new TextBuilder(); 
            textBuilder 
                .Line("EventType: " + loggerEvent.EventType); 
            LoggingUtils.Format(textBuilder, loggerEvent.Properties); 
            LoggingUtils.AddSeparator(textBuilder); 
            return textBuilder.ToString(); 
        } 
        public void Init(NameValueConfigurationCollection props) 
        { 
        } 
    } 
}

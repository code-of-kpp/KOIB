using System; 
using System.Linq; 
using System.Threading; 
using Croc.Core.Utils; 
using Croc.Core.Utils.Text; 
namespace Croc.Core.Diagnostics 
{ 
    public static class LoggingUtils 
    { 
        public static void LogToConsole(string message, params object[] args) 
        { 
            if (args != null && args.Length > 0) 
                message = string.Format(message, args); 
            Console.WriteLine(string.Format("[{0:yyyy.MM.dd HH:mm:ss.fff}][{1}] {2}", 
                DateTime.Now, Thread.CurrentThread.ManagedThreadId, message)); 
        } 
        public static void FillCommonContextProperies(EventProperties properties) 
        { 
            properties[LoggerEvent.TIMESTAMP_PROPERTY] = DateTime.Now; 
            properties[LoggerEvent.THREAD_ID] = Thread.CurrentThread.ManagedThreadId; 
        } 
        public static TextBuilder Format(EventProperties properties) 
        { 
            var textBuilder = new TextBuilder(); 
            Format(textBuilder, properties); 
            return textBuilder; 
        } 
        public static void Format(TextBuilder textBuilder, EventProperties properties) 
        { 
            if (properties.ContainsKey(String.Empty)) 
                textBuilder.Append("EventData"); 
            ObjectDumper.DumpObject( 
                properties.OrderBy(pair => pair.Key), 
                textBuilder); 
        } 
        public static void AddSeparator(TextBuilder textBuilder) 
        { 
            textBuilder 
                .EmptyLine() 
                .Line("-------------------------------------------------------------------------------") 
                .EmptyLine(); 
        } 
    } 
}

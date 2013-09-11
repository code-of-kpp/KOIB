using System.Collections.Generic; 
using System.Diagnostics; 
using Croc.Core.Utils.Text; 
namespace Croc.Core.Diagnostics.Default 
{ 
    public class FormattedTraceListener : ConsoleTraceListener  
    { 
        private readonly string _format = ""; 
        private readonly List<string> _messageNames; 
        public override bool IsThreadSafe 
        { 
            get { return true; } 
        } 
        public FormattedTraceListener(string format) 
        { 
            _format = StringFormatParser.Parse(format, out _messageNames); 
            _format = _format.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t"); 
        } 
        public override void WriteLine(string message) 
        { 
            var resStr = FormatLine(message); 
            base.WriteLine(resStr); 
        } 
        private string FormatLine(string message) 
        { 
            var args = new string[_messageNames.Count]; 
            for (int i = 0; i < _messageNames.Count; ++i) 
            { 
                var messageName = _messageNames[i]; 
                var startIndex = message.IndexOf(messageName); 
                if (startIndex < 0) 
                    continue; 
                startIndex += messageName.Length + 2; 
                var endIndex = message.IndexOf('\r', startIndex); 
                if (endIndex < 0) 
                    endIndex = message.IndexOf('\n', startIndex); 
                if (endIndex < 0) 
                { 
                    args[i] = message.Substring(startIndex); 
                    break; 
                } 
                args[i] = message.Substring(startIndex, endIndex - startIndex); 
            } 
            return string.Format(_format, args); 
        } 
    } 
}

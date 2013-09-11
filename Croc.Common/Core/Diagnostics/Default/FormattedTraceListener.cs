using System; 

using System.Collections.Generic; 

using System.Diagnostics; 

using Croc.Core.Utils.Text; 

 

 

namespace Croc.Core.Diagnostics.Default 

{ 

    public class FormattedTraceListener : ConsoleTraceListener  

    { 

        private string _format = ""; 

        private List<string> _messageNames; 

 

 

        public override bool IsThreadSafe 

        { 

            get { return true; } 

        } 

 

 

        public FormattedTraceListener(string format)  

            : base() 

        { 

            _format = StringFormatParser.Parse(format, out _messageNames); 

            _format = _format.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t"); 

        } 

 

 

        public override void WriteLine(string message) 

        { 

            var args = new string[_messageNames.Count]; 

 

 

            for (int i = 0; i < _messageNames.Count; ++i) 

            { 

                var messageName = _messageNames[i]; 

 

 

                var startIndex = message.IndexOf(messageName); 

                if (startIndex < 0) 

                    continue; 

 

 

                // увеличиваем индек на длину строки "messageName: " 

                startIndex += messageName.Length + 2; 

                var endIndex = message.IndexOf('\r', startIndex); 

                if (endIndex < 0) 

                    endIndex = message.IndexOf('\n', startIndex); 

 

 


                if (endIndex < 0) 

                { 

                    // нашли нужный кусок сообщения, который последний в сообщении 

                    args[i] = message.Substring(startIndex); 

                    break; 

                } 

 

 

                // нашли нужный кусок сообщения 

                args[i] = message.Substring(startIndex, endIndex - startIndex); 

            } 

 

 

            var resStr = string.Format(_format, args); 

            base.WriteLine(resStr); 

        } 

    } 

}



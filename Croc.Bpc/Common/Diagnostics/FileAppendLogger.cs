using System; 
using System.IO; 
using System.Linq; 
using System.Text; 
using Croc.Core; 
using Croc.Core.Diagnostics; 
using Croc.Core.Utils.IO; 
namespace Croc.Bpc.Diagnostics 
{ 
    public class FileAppendLogger : ILogger, IDisposable 
    { 
        private readonly string _logFileNamePrefix; 
        protected readonly string _logDirectory; 
        protected StreamWriter _writer; 
        private void CreateWriter() 
        { 
            var fileName = Directory.GetFiles(_logDirectory, _logFileNamePrefix + "*.log") 
                .OrderBy(f => f).LastOrDefault(); 
            if (string.IsNullOrEmpty(fileName)) 
                fileName = CreateNewLogFileName(_logFileNamePrefix); 
            try 
            { 
                _writer = new StreamWriter(fileName, true, Encoding.GetEncoding(1251)); 
            } 
            catch 
            { 
                fileName = CreateNewLogFileName(_logFileNamePrefix); 
                _writer = new StreamWriter(fileName, true); 
            } 
        } 
        protected virtual string CreateNewLogFileName(string filePrefix) 
        { 
            return Path.Combine(_logDirectory, filePrefix + ".log"); 
        } 
        protected virtual string ConstructMessage(LoggerEvent logEvent) 
        { 
            var resultMsg = logEvent.Properties[BpcLoggerExtensions.MESSAGEID_PROPERTY].ToString(); 
            if (logEvent.Properties.ContainsKey(LoggerEvent.PARAMETERS_PROPERTY)) 
                resultMsg = ((object[]) logEvent.Properties[LoggerEvent.PARAMETERS_PROPERTY]). 
                    Aggregate(resultMsg + '\t', (current, param) => current + param.ToString() + '|'); 


            resultMsg = resultMsg.Trim('|'); 
            resultMsg = String.Format("{0:yyMMddHHmmss} {1}", DateTime.Now, resultMsg); 
            return resultMsg; 
        } 
        protected virtual bool CanAppendLine(LoggerEvent logEvent, string message) 
        { 
            return true; 
        } 
        public FileAppendLogger(string logDirectory, string logFileName) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(logDirectory)); 
            CodeContract.Requires(!string.IsNullOrEmpty(logFileName)); 
            _logDirectory = logDirectory; 
            _logFileNamePrefix = logFileName; 
            FileUtils.EnsureDirExists(_logDirectory); 
            CreateWriter(); 
        } 
        public void Log(LoggerEvent logEvent) 
        { 
            var resultMsg = ConstructMessage(logEvent); 
            if (CanAppendLine(logEvent, resultMsg)) 
            { 
                _writer.WriteLine(resultMsg); 
                _writer.Flush(); 
            } 
        } 
        public bool IsAcceptedByEventType(LoggerEvent logEvent) 
        { 
            return true; 
        } 
        public void Dispose() 
        { 
            if (_writer != null) 
                _writer.Close(); 
        } 
    } 
}

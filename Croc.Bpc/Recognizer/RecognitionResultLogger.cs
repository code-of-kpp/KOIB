using System; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.FileSystem; 
using Croc.Core; 
using Croc.Core.Diagnostics; 
using System.IO; 
using Croc.Core.Utils.IO; 
namespace Croc.Bpc.Recognizer 
{ 
    public class RecognitionResultLogger : FileAppendLogger 
    { 
        private const string LOG_FILE_NAME_PREFIX_TEMPLATE = "RecognizerLog.{0:yyyyMMdd}."; 
        private const string LOG_MESSAGE_FORMAT = "{0},{1}"; 
        private IFileSystemManager _fileSystemManager; 
        private static readonly object s_fileSystemManagerSync = new object(); 
        private IFileSystemManager GetFileSystemManager() 
        { 
            if (_fileSystemManager == null) 
                lock (s_fileSystemManagerSync) 
                    if (_fileSystemManager == null) 
                        _fileSystemManager = CoreApplication.Instance.GetSubsystem<IFileSystemManager>(); 
            return _fileSystemManager; 
        } 
        private static string ApplyLogEventParams(LoggerEvent logEvent) 
        { 
            var resultMsg = logEvent.Properties[LoggerEvent.MESSAGE_PROPERTY].ToString(); 
            if (logEvent.Properties.ContainsKey(LoggerEvent.PARAMETERS_PROPERTY)) 
            { 
                resultMsg = BpcLoggerExtensions. 
                    GetMessageBody((Message)logEvent.Properties[BpcLoggerExtensions.MESSAGEID_PROPERTY]); 
                resultMsg = 
                    String.Format(resultMsg, (object[])logEvent.Properties[LoggerEvent.PARAMETERS_PROPERTY]); 
            } 
            return resultMsg; 
        } 
        protected override string CreateNewLogFileName(string filePrefix) 
        { 
            var fileName = FileUtils.GetUniqueName(_logDirectory, filePrefix, "log", 6); 
            fileName = Path.Combine(_logDirectory, fileName); 
            return fileName; 
        } 
        protected override bool CanAppendLine(LoggerEvent logEvent, string message) 
        { 
            var fileSystemManager = GetFileSystemManager(); 
            if (fileSystemManager == null) 
                return true; 
            var requiredSize = message.Length / FileUtils.BYTES_IN_KB + 1; 
            return fileSystemManager.ReserveDiskSpace(_logDirectory, requiredSize); 
        } 
        protected override string ConstructMessage(LoggerEvent logEvent) 
        { 
            var recResultMsg = ApplyLogEventParams(logEvent); 
            recResultMsg = String.Format(LOG_MESSAGE_FORMAT, GetTimeLabel(), recResultMsg); 
            return recResultMsg; 
        } 
        private static string GetTimeLabel() 
        { 
            var now = DateTime.Now; 
            var fifteenMinutesCount = now.Hour * 4 + now.Minute / 15; // кол-во 15-минутных интервалов 
            return string.Format("{0:yyyyMMdd}{1:00}", now, fifteenMinutesCount); 
        } 
        public RecognitionResultLogger(string logDirectory) 
            : base(logDirectory, string.Format(LOG_FILE_NAME_PREFIX_TEMPLATE, DateTime.Now)) 
        { 
        } 
    } 
}

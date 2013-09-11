using System; 
using System.Collections.Generic; 
using System.Configuration; 
using System.Diagnostics; 
using Croc.Bpc.Diagnostics; 
using Croc.Core; 
using Croc.Core.Diagnostics; 
using Croc.Core.Utils.IO; 
namespace Croc.Bpc.FileSystem 
{ 
    public class AvailableSpaceLogFilter : IEventWriterFilter 
    { 
        private const int DEFAULT_MIN_FREE_SPACE = 1024; 
        private const string DEFAULT_LOG_NAME = "*"; 
        private readonly Dictionary<string, int> _minFreeSpaces = new Dictionary<string, int>(); 
        private readonly List<string> _blockedLogs = new List<string>(); 
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
        public bool Accepted(EventWriterTriplet writerTriplet, LoggerEvent logEvent, string message) 
        { 
            if (!(writerTriplet.Writer is IEventFileSystemWriter)) 
            { 
                return true; 
            } 
            string uniqueId = EventDispatcher.GetUniqueId(logEvent); 
            if (_blockedLogs.Contains(uniqueId)) 
            { 
                return false; 
            } 
            var pointName = DEFAULT_LOG_NAME; 
            if (EventDispatcher.GroupByFieldDefined && logEvent.Properties.ContainsKey(EventDispatcher.GroupByField)) 
            { 
                var groupByFieldValue = (string) logEvent[EventDispatcher.GroupByField]; 
                if (_minFreeSpaces.ContainsKey(groupByFieldValue)) 
                    pointName = groupByFieldValue; 
            } 
            var minFreeSpace = _minFreeSpaces[pointName]; 
            var requiredSize = message.Length / FileUtils.BYTES_IN_KB + 1; 
            long availableSize; 
            var fileSystemManager = GetFileSystemManager(); 
            if (fileSystemManager != null && 
                !fileSystemManager.ReserveDiskSpace( 
                    (writerTriplet.Writer as IEventFileSystemWriter).GetPoint(uniqueId), 
                    requiredSize, 
                    minFreeSpace, 
                    out availableSize)) 
            { 
                var errorEvent = new LoggerEvent { EventType = TraceEventType.Error }; 
                errorEvent[LoggerEvent.LOGGERNAME_PROPERTY] = logEvent[LoggerEvent.LOGGERNAME_PROPERTY]; 
                errorEvent[BpcLoggerExtensions.MESSAGEID_PROPERTY] = 
                    Message.Common_AvailableSpaceFilterNotEnoughSpaceError; 
                var minSizeMb = (double)minFreeSpace / FileUtils.BYTES_IN_KB; 
                var availableSizeMb = (double)availableSize / FileUtils.BYTES_IN_MB; 
                if (writerTriplet.Raw) 
                { 
                    errorEvent[LoggerEvent.MESSAGE_PROPERTY] = 
                        Message.Common_AvailableSpaceFilterNotEnoughSpaceError; 
                    errorEvent[LoggerEvent.PARAMETERS_PROPERTY] = 
                        new object[] { uniqueId, availableSizeMb, minSizeMb }; 
                } 
                else 
                { 
                    errorEvent[LoggerEvent.MESSAGE_PROPERTY] = 
                        string.Format( 
                            BpcLoggerExtensions.GetMessageBody(Message.Common_AvailableSpaceFilterNotEnoughSpaceError), 
                            uniqueId, availableSizeMb, minSizeMb); 
                } 
                writerTriplet.Writer.Write(uniqueId, writerTriplet.Formatter.Format(errorEvent)); 
                if (!_blockedLogs.Contains(uniqueId)) 
                { 
                    _blockedLogs.Add(uniqueId); 
                } 
                return false; 
            } 
            return true; 
        } 
        public void Init(NameValueConfigurationCollection props) 
        { 
            _minFreeSpaces.Add(DEFAULT_LOG_NAME, DEFAULT_MIN_FREE_SPACE); 
            if (!EventDispatcher.GroupByFieldDefined) 
            { 
                foreach (TraceEventType traceEventType in Enum.GetValues(typeof(TraceEventType))) 
                { 
                    _minFreeSpaces.Add(traceEventType.ToString(), DEFAULT_MIN_FREE_SPACE); 
                } 
            } 
            foreach (NameValueConfigurationElement prop in props) 
            { 
                _minFreeSpaces[prop.Name] = Convert.ToInt32(prop.Value); 
            } 
        } 
    } 
}

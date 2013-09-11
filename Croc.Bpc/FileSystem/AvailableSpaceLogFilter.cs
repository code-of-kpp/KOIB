using System; 

using System.Collections.Generic; 

using System.Configuration; 

using System.Diagnostics; 

using Croc.Core; 

using Croc.Core.Diagnostics; 

using Croc.Bpc.Common.Diagnostics; 

 

 

namespace Croc.Bpc.FileSystem 

{ 

    /// <summary> 

    /// Фильтр проверяет наличие доступного места при записи в журнал 

    /// </summary> 

    public class AvailableSpaceLogFilter : IEventWriterFilter 

    { 

        /// <summary> 

        /// Размер нижнего предела свободного пространства в Кб 

        /// </summary> 

        private const long DEFAULT_MIN_FREE_SPACE = 1024; 

 

 

        /// <summary> 

        /// Ограничение для тех событий, которым нет частного ограничения 

        /// </summary> 

        private const string DEFAULT_LOG_NAME = "*"; 

        /// <summary> 

        /// Число байт в килобайте 

        /// </summary> 

        private const int BYTES_IN_KB = 1024; 

        /// <summary> 

        /// Число байт в мегабайте 

        /// </summary> 

        private const double BYTES_IN_MB = BYTES_IN_KB * BYTES_IN_KB; 

        /// <summary> 

        /// Размер кластера 

        /// </summary> 

        private const int CLASTER_SIZE = 1024; 

 

 

        /// <summary> 

        /// Ограничения на доступное пространство для журналов 

        /// </summary> 

        private Dictionary<string, long> _minFreeSpaces = new Dictionary<string, long>(); 

        /// <summary> 

        /// Список "заблокированных" журналов, при записи в которые место закончилось 

        /// </summary> 

        private List<string> _blockedLogs = new List<string>(); 

 

 


 
 

        private IFileSystemManager _fileSystemManager; 

        private static object s_fileSystemManagerSync = new object(); 

        /// <summary> 

        /// Возвращает ссылку на Менеджера файловой системы 

        /// </summary> 

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

                // Работаем только с записью в файлы 

                return true; 

            } 

 

 

            string uniqueId = EventDispatcher.GetUniqueId(logEvent); 

 

 

            if(_blockedLogs.Contains(uniqueId)) 

            { 

                // уже был факт нехватки места 

                return false; 

            } 

 

 

            string pointName = DEFAULT_LOG_NAME; 

 

 

            if (logEvent.Properties.ContainsKey(EventDispatcher.GroupByField)) 

            { 

                if (_minFreeSpaces.ContainsKey(logEvent[EventDispatcher.GroupByField].ToString())) 

                { 

                    pointName = logEvent[EventDispatcher.GroupByField].ToString(); 

                } 

            } 

 

 


            long minFreeSpace = _minFreeSpaces[pointName]; 

 

 

            // округлим сверху до размера кластера 

            long requiredSize = message.Length > CLASTER_SIZE ? 

                CLASTER_SIZE * ((message.Length + CLASTER_SIZE - 1) / CLASTER_SIZE) : 

                CLASTER_SIZE; 

 

 

            // минимально необходимый размер в байтах 

            long minSize = minFreeSpace * BYTES_IN_KB; 

            long availableSize; 

 

 

            // если не удалось зарезервировать место на диске 

            var fileSystemManager = GetFileSystemManager(); 

            if (fileSystemManager != null && 

                !fileSystemManager.ReserveDiskSpace( 

                    FileType.Log, 

                    (writerTriplet.Writer as IEventFileSystemWriter).GetPoint(uniqueId),  

                    requiredSize,  

                    minSize, 

                    out availableSize)) 

            { 

                // пишем в лог об окончании места на диске 

                LoggerEvent errorEvent = new LoggerEvent(); 

                errorEvent.EventType = TraceEventType.Error; 

                errorEvent[LoggerEvent.LOGGERNAME_PROPERTY] = logEvent[LoggerEvent.LOGGERNAME_PROPERTY]; 

                errorEvent[BpcLoggerExtensions.MESSAGEID_PROPERTY] = Message.AvailableSpaceFilterNotEnoughSpaceError; 

 

 

                // если форматировать сообщение не нужно 

                if (writerTriplet.Raw) 

                { 

                    errorEvent[LoggerEvent.MESSAGE_PROPERTY] = 

                        Message.AvailableSpaceFilterNotEnoughSpaceError; 

                    errorEvent[LoggerEvent.PARAMETERS_PROPERTY] =  

                        new object[] { 

                            uniqueId, availableSize/BYTES_IN_MB, minSize/BYTES_IN_MB 

                        }; 

                } 

                else 

                { 

                    errorEvent[LoggerEvent.MESSAGE_PROPERTY] = 

                        string.Format( 

                            BpcLoggerExtensions.GetMessageBody(Message.AvailableSpaceFilterNotEnoughSpaceError), 

                            uniqueId, availableSize/BYTES_IN_MB, minSize/BYTES_IN_MB); 

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

            // устанавливаем ограничения по умолчанию 

            _minFreeSpaces.Add(DEFAULT_LOG_NAME, DEFAULT_MIN_FREE_SPACE); 

 

 

            if (String.IsNullOrEmpty(EventDispatcher.GroupByField)) 

            { 

                foreach (TraceEventType traceEventType in Enum.GetValues(typeof(TraceEventType))) 

                { 

                    _minFreeSpaces.Add(traceEventType.ToString(), DEFAULT_MIN_FREE_SPACE); 

                } 

            } 

 

 

            foreach (NameValueConfigurationElement prop in props) 

            { 

                _minFreeSpaces[prop.Name] = Convert.ToInt64(prop.Value); 

            } 

        } 

    } 

}



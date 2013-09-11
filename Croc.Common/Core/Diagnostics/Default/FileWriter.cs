using System; 

using System.Collections; 

using System.Collections.Generic; 

using System.Configuration; 

using System.IO; 

using System.Text; 

using Croc.Core.Utils.IO; 

 

 

namespace Croc.Core.Diagnostics.Default 

{ 

    /// <summary> 

    /// Протоколирование в файл 

    /// </summary> 

    public class FileWriter : IEventFileSystemWriter 

    { 

        /// <summary> 

        /// Корневой каталог протоколов 

        /// </summary> 

        private static string s_rootFolder; 

        /// <summary> 

        /// Объект синхронизации 

        /// </summary> 

        private static object s_LogWritersSync = new object(); 

        /// <summary> 

        /// Текущая дата 

        /// </summary> 

        private static DateTime s_currentDate; 

 

 

        /// <summary> 

        /// Инициализация писателей 

        /// </summary> 

        /// <param name="rootFolder"></param> 

        public static void Init(string rootFolder) 

        { 

            s_rootFolder = rootFolder; 

            s_currentDate = DateTime.Today; 

        } 

 

 

		/// <summary> 

		/// Закрытие всех писателей в файлы 

		/// </summary> 

		public static void Close() 

		{ 

            lock (s_LogWritersSync) 

            { 

                foreach (StreamWriter writer in s_LogWriters.Values) 

                    writer.Close(); 


 
 

                s_LogWriters.Clear(); 

            } 

		} 

 

 

        /// <summary> 

        /// Коллекция потоков записи в конфигурационные файлы 

        /// </summary> 

        private static Hashtable s_LogWriters = new Hashtable(); 

 

 

        /// <summary> 

        /// Функция создания журнала события и источника события 

        /// </summary> 

        /// <param name="uniqueId">Идентификатор</param> 

        /// <returns>Ссылка на созданный журнал</returns> 

        private static StreamWriter GetLogWriter(string uniqueId) 

        { 

            string writerId = GetWriterPoint(uniqueId); 

 

 

            StreamWriter oWriter; 

            lock (s_LogWritersSync) 

            { 

                // Попробуем получить поток записи в лог из хештаблицы 

                oWriter = (StreamWriter)s_LogWriters[writerId]; 

 

 

                // если изменилась текущая дата, то создаем новый файл, соответствующий этой дате 

                // (также это происходит при первой записи в файл) 

                DateTime today = DateTime.Today; 

 

 

                if (s_currentDate != today && oWriter != null) 

                { 

                    s_currentDate = today; 

                    oWriter.Flush(); 

                    oWriter.Close(); 

                    oWriter = null; 

                } 

 

 

                if (oWriter == null) 

                { 

                    if (!Directory.Exists(Path.GetDirectoryName(writerId))) 

                    { 

                        Directory.CreateDirectory(Path.GetDirectoryName(writerId)); 

                    } 


 
 

                    oWriter = new StreamWriter( 

                        FileUtils.CreateUniqueFileWithDateMark( 

                            Path.GetDirectoryName(writerId), 

                            Path.GetFileName(writerId), 

                            "log", // fileExtension 

                            6 // fileNumberLength 

                        ),  

                        Encoding.GetEncoding(1251) 

                    ); 

 

 

                    // устанавливаем флаг записи в файл 

                    oWriter.AutoFlush = true; 

                    // и добавим в хештаблицу, чтобы больше не создавать 

                    s_LogWriters[writerId] = oWriter; 

                } 

            } 

 

 

            // Вернем полученный поток записи 

            return oWriter; 

        } 

 

 

        /// <summary> 

        /// Получает точку файловой системы, в которую пишется протокол 

        /// </summary> 

        /// <param name="uniqueId">Уникальный идентификатор</param> 

        /// <returns>Полный путь</returns> 

        public static string GetWriterPoint(string uniqueId) 

        { 

            ISubsystem subsystem = CoreApplication.Instance.GetSubsystem(uniqueId); 

 

 

            string writerId = s_rootFolder; 

 

 

            if(subsystem != null) 

            { 

                if (!string.IsNullOrEmpty(subsystem.LogFileFolder)) 

                { 

                    writerId = subsystem.LogFileFolder; 

                } 

 

 

                writerId += "/" + (subsystem.SeparateLog ? subsystem.Name : CoreApplication.Instance.Name); 

            } 

            else 


            { 

                // если такой подсистемы найти не удалось, то протоколируем в журнал с переданным именем 

                writerId += "/" + uniqueId; 

            } 

            return writerId; 

        } 

 

 

        /// <summary> 

        /// Получает точку файловой системы, в которую пишется протокол 

        /// </summary> 

        /// <param name="uniqueId">Уникальный идентификатор</param> 

        /// <returns>Полный путь</returns> 

        public string GetPoint(string uniqueId) 

        { 

            return GetWriterPoint(uniqueId); 

        } 

 

 

        private readonly Object _locker = new Object(); 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        public FileWriter() 

        { 

        } 

 

 

        public void Write(string uniqueLogId, String msg) 

        { 

            lock (_locker) 

            { 

                try 

                { 

                    StreamWriter _writer = GetLogWriter(uniqueLogId); 

                    if (_writer != null) 

                    { 

                        _writer.WriteLine(msg); 

                    } 

                } 

                catch (Exception) 

                { 

                    // TODO: а что делать в этом случае? 

                    throw; 

                } 

            } 

        } 

 


 
        public void Init(NameValueConfigurationCollection props) 

        { 

            // ничего не делаем 

        } 

    } 

}



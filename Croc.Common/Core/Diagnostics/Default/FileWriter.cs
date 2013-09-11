using System; 
using System.Collections.Generic; 
using System.Configuration; 
using System.IO; 
using System.Text; 
using Croc.Core.Utils.IO; 
namespace Croc.Core.Diagnostics.Default 
{ 
    public class FileWriter : IEventFileSystemWriter 
    { 
        private static string s_rootFolder; 
        private static readonly Dictionary<string, string> s_writerPoints = new Dictionary<string, string>(); 
        private static readonly Dictionary<string, StreamWriter> s_logWriters = new Dictionary<string, StreamWriter>(); 
        private static readonly object s_logWritersSync = new object(); 
        private static DateTime s_currentDate; 
        public static void Init(string rootFolder) 
        { 
            s_rootFolder = rootFolder; 
            s_currentDate = DateTime.Today; 
        } 
        public void Init(NameValueConfigurationCollection props) 
        { 
        } 
        public static void Close() 
        { 
            lock (s_logWritersSync) 
            { 
                foreach (var writer in s_logWriters.Values) 
                { 
                    writer.Flush(); 
                    writer.Close(); 
                } 
                s_logWriters.Clear(); 
            } 
        } 
        public static string GetWriterPoint(string uniqueId) 
        { 
            if (!s_writerPoints.ContainsKey(uniqueId)) 
            { 
                var sb = new StringBuilder(64); 
                var subsystem = CoreApplication.Instance.GetSubsystem(uniqueId); 
                if (subsystem != null) 
                { 
                    sb.Append(!string.IsNullOrEmpty(subsystem.LogFileFolder) 
                                  ? subsystem.LogFileFolder 
                                  : s_rootFolder); 
                    sb.Append('/'); 
                    sb.Append(subsystem.SeparateLog 
                                  ? subsystem.Name 
                                  : CoreApplication.Instance.Name); 
                } 
                else 
                { 
                    sb.Append(s_rootFolder); 
                    sb.Append('/'); 
                    sb.Append(uniqueId); 
                } 
                s_writerPoints[uniqueId] = sb.ToString(); 
            } 
            return s_writerPoints[uniqueId]; 
        } 
        public string GetPoint(string uniqueId) 
        { 
            return GetWriterPoint(uniqueId); 
        } 
        private readonly object _writeSync = new object(); 
        public void Write(string uniqueLogId, string msg) 
        { 
            lock (_writeSync) 
            { 
                var writer = GetLogWriter(uniqueLogId); 
                if (writer != null) 
                    writer.WriteLine(msg); 
            } 
        } 
        private static StreamWriter GetLogWriter(string uniqueId) 
        { 
            var writerPoint = GetWriterPoint(uniqueId); 
            lock (s_logWritersSync) 
            { 
                var writer = s_logWriters.ContainsKey(writerPoint) 
                                 ? s_logWriters[writerPoint] 
                                 : null; 
                var today = DateTime.Today; 
                if (s_currentDate != today && writer != null) 
                { 
                    s_currentDate = today; 
                    writer.Flush(); 
                    writer.Close(); 
                    writer = null; 
                } 
                if (writer == null) 
                { 
                    FileUtils.EnsureDirExists(Path.GetDirectoryName(writerPoint)); 
                    var fileName = FileUtils.CreateUniqueFileWithDateMark( 
                        Path.GetDirectoryName(writerPoint), 
                        Path.GetFileName(writerPoint), 
                        "log", 
                        6); 
                    writer = new StreamWriter(fileName, Encoding.GetEncoding(1251)) 
                    { 
                        AutoFlush = true 
                    }; 
                    s_logWriters[writerPoint] = writer; 
                } 
                return writer; 
            } 
        } 
    } 
}

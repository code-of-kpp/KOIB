using System; 
using System.Collections.Generic; 
using System.IO; 
using System.Linq; 
using System.Runtime.Serialization; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.FileSystem.Config; 
using Croc.Bpc.Utils; 
using Croc.Core; 
using Croc.Core.Configuration; 
using Croc.Core.Extensions; 
using Croc.Core.Utils; 
using Croc.Core.Utils.IO; 
using Mono.Unix; 
namespace Croc.Bpc.FileSystem 
{ 
    [SubsystemConfigurationElementTypeAttribute(typeof(FileSystemManagerConfig))] 
    public sealed class FileSystemManager : Subsystem, IFileSystemManager 
    { 
        #region private Поля 
        private static readonly object s_freeDiskSpacesSync = new object(); 
        private static readonly Dictionary<string, long> s_freeDiskSpaces = new Dictionary<string, long>(); 
        private Dictionary<FileType, DataDirectoryConfig> _dataDirectories; 
        private FileSystemManagerConfig _config; 
        private long _minDiskSpaceToFreeInBytes; 
        private string _lastArchivePath; 
        #endregion 
        #region Override Subsystem 
        public override void Init(SubsystemConfig config) 
        { 
            DeleteTempFiles(); 
            InitInternal(config); 
            ReserveDiskSpace("./", _config.SystemReservedSpaceKb.Value); 
        } 
        public override void ApplyNewConfig(SubsystemConfig newConfig) 
        { 
            InitInternal(newConfig); 
        } 
        private void InitInternal(SubsystemConfig config) 
        { 
            _config = (FileSystemManagerConfig)config; 
            _minDiskSpaceToFreeInBytes = _config.MinDiskSpaceToFreeMb.Value * FileUtils.BYTES_IN_MB; 
            _dataDirectories = new Dictionary<FileType, DataDirectoryConfig>(); 
            FileUtils.EnsureDirExists(_config.DataDirectoriesRoot.Value); 
            foreach (DataDirectoryConfig dir in _config.DataDirectories) 
            { 
                if (_dataDirectories.ContainsKey(dir.FileType)) 
                    throw new Exception("В конфигурации два или более элементов " + 
                        "DataDirectory с одинаковым типом файлов"); 
                _dataDirectories.Add(dir.FileType, dir); 
                FileUtils.EnsureDirExists(Path.Combine(_config.DataDirectoriesRoot.Value, dir.Path)); 
            } 
            if (!_dataDirectories.ContainsKey(FileType.Log)) 
            { 
                var rootDir = new DirectoryInfo(_config.DataDirectoriesRoot.Value); 
                var logDir = new DirectoryInfo(CoreApplication.Instance.LogFileFolder); 
                if (!logDir.FullName.Contains(rootDir.FullName)) 
                    throw new ApplicationException("Папка с логами не содержится в корне архивируемых данных"); 
                var logConfig = new DataDirectoryConfig 
                { 
                    Archive = true, 
                    Path = logDir.FullName.Replace(rootDir.FullName, "").Trim('\\').Trim('/'), 
                    FileTypeStr = "Log" 
                }; 
                _dataDirectories.Add(FileType.Log, logConfig); 
            } 
        } 
        #endregion 
        #region IFileSystemManager 
        public bool ReserveDiskSpace(string path, int requiredSizeKb) 
        { 
            long availableSize; 
            return ReserveDiskSpace(path, requiredSizeKb, requiredSizeKb, out availableSize); 
        } 
        public bool ReserveDiskSpace( 
            string path, int requiredSizeKb, int minSizeKb, out long availableSize) 
        { 
            long requiredSize = requiredSizeKb * FileUtils.BYTES_IN_KB; 
            long minSize = minSizeKb * FileUtils.BYTES_IN_KB; 
            return ReserveDiskSpace(path, requiredSize, minSize, out availableSize); 
        } 
        private bool ReserveDiskSpace( 
            string path, long requiredSize, long minSize, out long availableSize) 
        { 
            return ReserveDiskSpace(path, requiredSize, minSize, out availableSize, true); 
        } 
        private bool ReserveDiskSpace( 
            string path, long requiredSize, long minSize, out long availableSize, bool doCleanUp) 
        { 
            minSize = Math.Max(minSize, _minDiskSpaceToFreeInBytes); 
            lock (s_freeDiskSpacesSync) 
            { 
                var rootFullName = GetDirectoryDriveName(path); 
                if (!s_freeDiskSpaces.ContainsKey(rootFullName)) 
                { 
                    availableSize = GetAvailableFreeSpace(rootFullName); 
                    s_freeDiskSpaces.Add(rootFullName, availableSize); 
                    Logger.LogInfo(Message.FileSystemDiscSpace, rootFullName, path, availableSize); 
                } 
                else 
                { 
                    availableSize = s_freeDiskSpaces[rootFullName]; 
                    if (availableSize < minSize) 
                    { 
                        availableSize = GetAvailableFreeSpace(rootFullName); 
                        s_freeDiskSpaces[rootFullName] = availableSize; 
                    } 
                } 
                if (availableSize < minSize && doCleanUp) 
                    if (!ClearDisk(rootFullName, minSize)) 
                        return false; 
                s_freeDiskSpaces[rootFullName] -= requiredSize; 
                return true; 
            } 
        } 
        public bool WriteTextToFile(string path, FileMode mode, string text, bool doCleanUp) 
        { 
            try 
            { 
                long requiredSize = text.Length * 2; 
                long availableSize; 
                if (!ReserveDiskSpace(new FileInfo(path).DirectoryName, 
                    requiredSize, requiredSize, out availableSize)) 
                    return false; 
                using (var fileStream = File.Open(path, mode, FileAccess.Write, FileShare.None)) 
                { 
                    using (var writer = new StreamWriter(fileStream)) 
                        writer.Write(text); 
                } 
                return true; 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.FileSystemWriteToFileError, ex, path); 
                return false; 
            } 
        } 
        public void ArchiveFiles(string archivePrefix) 
        { 
            _lastArchivePath = GetNewArchiveName(archivePrefix); 
            foreach (var directory in _dataDirectories.Values.Where(dc => !dc.Archive).ToArray()) 
            { 
                DeleteFolder(GetDataDirectoryPath(directory.FileType)); 
            } 
            if (PlatformDetector.IsUnix) 
            { 
                ProcessHelper.StartProcessAndWaitForFinished 
                    ("mv", 
                     string.Format(" {0} {1}", _config.DataDirectoriesRoot.Value, _lastArchivePath), 
                     delegate(ProcessOutputProcessorState state) 
                     { 
                         if (state.LineNumber > 0) 
                             throw new ApplicationException("Ошибка при архивировании файлов приложения " + state.Line); 
                         return true; 
                     }, 
                     null); 
            } 
            else 
            { 
                MoveFolder(_config.DataDirectoriesRoot.Value, _lastArchivePath); 
                CoreApplication.Instance.Exited += MoveLastLogs; 
            } 
            FilesArchived.RaiseEvent(this, new FilesArchivedEventArgs(_lastArchivePath)); 
        } 
        public event EventHandler<FilesArchivedEventArgs> FilesArchived; 
        public string GetDataDirectoryPath(FileType type) 
        { 
            if (_dataDirectories.ContainsKey(type)) 
                return Path.Combine(_config.DataDirectoriesRoot.Value, _dataDirectories[type].Path); 
            throw new Exception(String.Format("Не найдена папка для хранения файлов типа {0}", type)); 
        } 
        public string GetTempFileName(string path) 
        { 
            var tempFile = new FileInfo(Path.GetTempFileName()); 
            return Path.Combine(path, tempFile.Name); 
        } 
        #region Сериализация и десериализация 
        private const string BACKUP_EXT = ".bkp"; 
        private enum SafeSerializationStage 
        { 
            Start, 
            CreateBackup, 
            Serialization, 
            MoveTmpToTarget, 
            UpdateBackup, 
            Sync, 
        } 
        public bool SafeSerialization( 
            object objectToSerialize, IFormatter formatter, string fullName, bool makeBackup, bool doCleanUp) 
        { 
            CodeContract.Requires(formatter != null); 
            CodeContract.Requires(!string.IsNullOrEmpty(fullName)); 
            var backupFileName = fullName + BACKUP_EXT; 
            var fullPath = new FileInfo(fullName).DirectoryName; 
            var tempFileName = GetTempFileName(fullPath); 
            var requiredSize = _config.MinFreeSpaceToSaveSerializedObjectKb.Value; 
            long avalibleSize; 
            if (!ReserveDiskSpace(fullPath, requiredSize, requiredSize, out avalibleSize)) 
                return false; 
            var stage = SafeSerializationStage.Start; 
            try 
            { 
                if (makeBackup) 
                { 
                    stage = SafeSerializationStage.CreateBackup; 
                    if (File.Exists(fullName)) 
                    { 
                        if (File.Exists(backupFileName)) 
                            File.Delete(backupFileName); 
                        File.Move(fullName, backupFileName); 
                    } 
                } 
                stage = SafeSerializationStage.Serialization; 
                using (var stream = File.Open(tempFileName, FileMode.Create, FileAccess.Write, FileShare.Read)) 
                { 
                    formatter.Serialize(stream, objectToSerialize); 
                    stream.Flush(); 
                } 
                stage = SafeSerializationStage.MoveTmpToTarget; 
                File.Move(tempFileName, fullName); 
                stage = SafeSerializationStage.Sync; 
                SystemHelper.SyncFileSystem(); 
                if (makeBackup) 
                { 
                    stage = SafeSerializationStage.UpdateBackup; 
                    try 
                    { 
                        if (File.Exists(backupFileName)) 
                            File.Delete(backupFileName); 
                        File.Copy(fullName, backupFileName); 
                    } 
                    catch (Exception ex) 
                    { 
                        Logger.LogWarning( 
                            Message.FileSystemSafeSerializationFailed, ex, objectToSerialize.GetType().FullName, stage); 
                    } 
                } 
                return true; 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError( 
                    Message.FileSystemSafeSerializationFailed, ex, objectToSerialize.GetType().FullName, stage); 
                return false; 
            } 
        } 
        private enum SafeDeserializationStage 
        { 
            RestoreBackup, 
            Deserialization, 
        } 
        public bool SafeDeserialization<T>(out T deserializedObject, IFormatter formatter, string fullName) 
        { 
            CodeContract.Requires(formatter != null); 
            CodeContract.Requires(!string.IsNullOrEmpty(fullName)); 
            deserializedObject = default(T); 
            var stage = SafeDeserializationStage.RestoreBackup; 
            try 
            { 
                var file = new FileInfo(fullName); 
                if (!file.Exists || file.Length <= 0) 
                { 
                    if (file.Exists && file.Length <= 0) 
                    { 
                        Logger.LogWarning(Message.FileSystemFileHasZeroSize, file.FullName); 
                        try 
                        { 
                            file.Delete(); 
                        } 
                        catch (Exception ex) 
                        { 
                            Logger.LogError(Message.FileSystemFileDeleteError, ex, file.FullName); 
                        } 
                    } 
                    var backupFilePath = fullName + BACKUP_EXT; 
                    if (!File.Exists(backupFilePath)) 
                        return true; 
                    File.Copy(backupFilePath, fullName); 
                    SystemHelper.SyncFileSystem(); 
                    file = new FileInfo(fullName); 
                    if (!file.Exists) 
                        return true; 
                } 
                stage = SafeDeserializationStage.Deserialization; 
                using (var stream = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)) 
                { 
                    deserializedObject = (T)formatter.Deserialize(stream); 
                } 
                return true; 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError( 
                    Message.FileSystemSafeDeserializationFailed, ex, fullName, stage); 
                return false; 
            } 
        } 
        #endregion 
        #endregion 
        private long GetAvailableFreeSpace(string rootName) 
        { 
            try 
            { 
                DriveInfo drive = DriveInfo.GetDrives().First(d => d.Name == rootName); 
                return drive.AvailableFreeSpace; 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.FileSystemCheckFreeSpaceError, ex, rootName); 
                return -1; 
            } 
        } 
        private static void DeleteTempFiles() 
        { 
            foreach (var file in Directory.GetFiles(Path.GetTempPath(), "tmp*.tmp")) 
            { 
                try 
                { 
                    File.Delete(file); 
                } 
                catch 
                { 
                } 
            } 
        } 
        private string GetNewArchiveName(string format) 
        { 
            var rootDir = new DirectoryInfo(_config.DataDirectoriesRoot.Value); 
            if (rootDir.Parent == null) 
                throw new ApplicationException("Корневая папка с данными не может быть корнем диска"); 
            var lastArchive = Directory.GetDirectories(rootDir.Parent.FullName, "*_*_*_*") 
                .OrderBy(d => d).LastOrDefault(); 
            var archiveNumber = lastArchive == null ? 1 : int.Parse(Path.GetFileName(lastArchive).Split('_')[0]) + 1; 
            var archiveName = string.Format(format, archiveNumber); 
            return Path.Combine(rootDir.Parent.FullName, archiveName); 
        } 
        private string GetDirectoryDriveName(string directoryPath) 
        { 
            var dir = new DirectoryInfo(directoryPath); 
            DriveInfo drive; 
            if (!PlatformDetector.IsUnix) 
            { 
                drive = DriveInfo.GetDrives().First(d => 
                    dir.FullName.StartsWith(d.RootDirectory.FullName, StringComparison.InvariantCultureIgnoreCase)); 
                return drive.Name; 
            } 
            var di = new UnixDirectoryInfo(directoryPath); 
            while (di.FullName != "/") 
            { 
                var temp = ResolveSymbolicLinks(di.FullName); 
                if (di.FullName != temp.FullName) 
                { 
                    di = temp; 
                    break; 
                } 
                di = di.Parent; 
            } 
            if (di.FullName != "/") 
            { 
                dir = new DirectoryInfo(di.FullName); 
            } 
            drive = DriveInfo.GetDrives().FirstOrDefault(d => 
                dir.FullName.StartsWith(d.RootDirectory.FullName) && d.RootDirectory.FullName != "/"); 
            if (drive == null) 
                return "/"; 
            return drive.Name; 
        } 
        private UnixDirectoryInfo ResolveSymbolicLinks(string directoryName) 
        { 
            var di = new UnixDirectoryInfo(directoryName); 
            while (true) 
            { 
                try 
                { 
                    var si = new UnixSymbolicLinkInfo(di.FullName); 
                    di = new UnixDirectoryInfo(si.ContentsPath); 
                } 
                catch 
                { 
                    break; 
                } 
            } 
            return di; 
        } 
        private static long DirectorySize(DirectoryInfo d) 
        { 
            if (!d.Exists) 
                return 0; 
            FileInfo[] fileInfos = d.GetFiles(); 
            long size = fileInfos.Sum(fi => fi.Length); 
            DirectoryInfo[] directoryInfos = d.GetDirectories(); 
            size += directoryInfos.Sum(di => DirectorySize(di)); 
            return size; 
        } 
        private bool AreDirsOnSameDisk(string firstDirPath, string secondDirPath) 
        { 
            var drive1 = GetDirectoryDriveName(firstDirPath); 
            var drive2 = GetDirectoryDriveName(secondDirPath); 
            if (drive1 == drive2) 
                return true; 
            return false; 
        } 
        private void FreeDiskSpace(string path, long sizeToFree) 
        { 
            var rootFullName = GetDirectoryDriveName(path); 
            lock (s_freeDiskSpacesSync) 
            { 
                if (s_freeDiskSpaces.ContainsKey(rootFullName)) 
                { 
                    s_freeDiskSpaces[rootFullName] += sizeToFree; 
                } 
            } 
        } 
        private bool ClearDisk(string directoryRoot, long requiredSize) 
        { 
            if (string.IsNullOrEmpty(_config.CleanOrder.Value)) 
                return false; 
            var cleanOrder = _config.CleanOrder.Value.Split(','); 
            if (AreDirsOnSameDisk(directoryRoot, _config.DataDirectoriesRoot.Value)) 
                if (ClearArchives(directoryRoot, requiredSize, cleanOrder)) 
                    return true; 
            if (ClearCurrentData(directoryRoot, requiredSize, cleanOrder)) 
                return true; 
            return false; 
        } 
        private bool ClearArchives(string directoryRoot, long requiredSize, IEnumerable<string> cleanOrder) 
        { 
            var rootDir = new DirectoryInfo(_config.DataDirectoriesRoot.Value); 
            foreach (var archive in rootDir.Parent.GetDirectories("*_*_*_*") 
                .OrderBy(dirInfo => dirInfo.Name.Contains("train") ? 0 : 1).ToArray()) 
            { 
                foreach (var fileTypeStr in cleanOrder) 
                { 
                    var deletedPath = GetArchiveTypedFolderName(fileTypeStr); 
                    if (DeleteFolder(Path.Combine(archive.FullName, deletedPath))) 
                        if (s_freeDiskSpaces[directoryRoot] > requiredSize) 
                            return true; 
                } 
            } 
            return false; 
        } 
        private bool ClearCurrentData(string directoryRoot, long requiredSize, IEnumerable<string> cleanOrder) 
        { 
            foreach (var fileTypeStr in cleanOrder) 
            { 
                var deletedPath = GetDataDirectoryPath((FileType)Enum.Parse(typeof(FileType), fileTypeStr)); 
                if (!Directory.Exists(deletedPath) || !AreDirsOnSameDisk(deletedPath, directoryRoot)) 
                    continue; 
                if (Directory.GetFiles(deletedPath).Where(DeleteFile) 
                    .Any(file => s_freeDiskSpaces[directoryRoot] > requiredSize)) 
                    return true; 
                if (Directory.GetDirectories(deletedPath).Where(DeleteFolder) 
                    .Any(folder => s_freeDiskSpaces[directoryRoot] > requiredSize)) 
                    return true; 
            } 
            return false; 
        } 
        private bool DeleteFolder(string path) 
        { 
            var di = new DirectoryInfo(path); 
            if (!di.Exists) 
                return true; 
            try 
            { 
                var dirSize = DirectorySize(di); 
                di.Delete(true); 
                FreeDiskSpace(path, dirSize); 
                return true; 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.FileSystemDeleteArchiveDirectoryError, ex); 
                return false; 
            } 
        } 
        public bool DeleteFile(string path) 
        { 
            var fi = new FileInfo(path); 
            if (!fi.Exists) 
                return true; 
            try 
            { 
                var fileSize = fi.Length; 
                fi.Delete(); 
                FreeDiskSpace(path, fileSize); 
                return true; 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.FileSystemDeleteFileError, ex, path); 
                return false; 
            } 
        } 
        private string GetArchiveTypedFolderName(string fileTypeStr) 
        { 
            var fileType = (FileType)Enum.Parse(typeof(FileType), fileTypeStr); 
            return _dataDirectories[fileType].Path; 
        } 
        #region Архивирование логов 
        private void MoveLastLogs(object sender, EventArgs e) 
        { 
            var logPath = GetDataDirectoryPath(FileType.Log); 
            var logDirInfo = new DirectoryInfo(logPath); 
            var logSize = DirectorySize(logDirInfo); 
            long avalibleSize; 
            if (ReserveDiskSpace(_lastArchivePath, logSize, logSize, out avalibleSize)) 
                MoveFolder(logPath, Path.Combine(_lastArchivePath, Path.GetFileName(logPath))); 
        } 
        private void MoveFolder(string folderPath, string destinationPath) 
        { 
            FileUtils.EnsureDirExists(destinationPath); 
            foreach (var file in Directory.GetFiles(folderPath)) 
            { 
                try 
                { 
                    var newFilePath = Path.Combine(destinationPath, Path.GetFileName(file)); 
                    DeleteFile(newFilePath); 
                    File.Move(file, newFilePath); 
                } 
                catch (Exception) 
                { 
                    File.Copy(file, Path.Combine(destinationPath, Path.GetFileName(file)), true); 
                } 
            } 
            foreach (var directory in Directory.GetDirectories(folderPath)) 
            { 
                var newDestinationDir = Path.Combine(destinationPath, Path.GetFileName(directory)); 
                MoveFolder(directory, newDestinationDir); 
            } 
            try 
            { 
                Directory.Delete(folderPath); 
            } 
            catch { } 
        } 
        #endregion 
    } 
}

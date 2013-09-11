using System; 

using System.Collections; 

using System.Collections.Generic; 

using System.IO; 

using System.Linq; 

using System.Runtime.Serialization; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Bpc.FileSystem.Config; 

using Croc.Core; 

using Croc.Core.Configuration; 

using Croc.Core.Utils; 

using Croc.Core.Utils.IO; 

using Mono.Unix; 

 

 

namespace Croc.Bpc.FileSystem 

{ 

    /// <summary> 

    /// Менеджер файловой системы 

    /// </summary> 

    [SubsystemConfigurationElementTypeAttribute(typeof(FileSystemManagerConfig))] 

    public sealed class FileSystemManager : Subsystem, IFileSystemManager 

    { 

		/// <summary> 

		/// Число байт в килобайте 

		/// </summary> 

		private const int BYTES_IN_KB = 1024; 

 

 

        /// <summary> 

        /// Расширение для резервных копий метода SafeSerialization 

        /// </summary> 

        private const string BACKUP_EXT = ".bkp"; 

 

 

        /// <summary> 

        /// Объект синхронизации для работы со словарем свободного места 

        /// </summary> 

        private static object s_freeDiskSpacesSync = new object(); 

        /// <summary> 

        /// Словарь свободного места [корневая папка диска, свободное место на диске] 

        /// </summary> 

        private static Dictionary<string, long> s_freeDiskSpaces = new Dictionary<string, long>(); 

		/// <summary> 

		/// Словарь директорий для файлов определенного типа [тип файлов, Config директории] 

		/// </summary> 

		private Dictionary<FileType, DataDirectoryConfig> _dataDirectories; 

		/// <summary> 

		/// Конфиг менеджера 

		/// </summary> 


		private FileSystemManagerConfig _config; 

		/// <summary> 

		/// Путь к последнему архиву 

		/// </summary> 

		private string _lastArchivePath; 

 

 

        /// <summary> 

        /// Инициализация 

        /// </summary> 

        /// <param name="config">конфиг FileManager</param> 

        public override void Init(SubsystemConfig config) 

        { 

            _config = (FileSystemManagerConfig)config; 

			_dataDirectories = new Dictionary<FileType, DataDirectoryConfig>(); 

 

 

			// заполним словарь директорий для хранения файлов 

			foreach (DataDirectoryConfig dir in _config.DataDirectories) 

			{ 

				// если этот тип файлов уже содержится в словаре, ругнемся 

				if (_dataDirectories.ContainsKey(dir.FileType)) 

					throw new Exception("В конфиге FileSystemManager два или более элементов " + 

						"DataDirectory с одинаковым типом файлов"); 

 

 

				_dataDirectories.Add(dir.FileType, dir); 

				FileUtils.EnsureDirExists(dir.Path); 

			} 

 

 

			// добавим папку для лога(все логи содержатся в Application.LogFileFolder), если ее еще нет 

			if (!_dataDirectories.ContainsKey(FileType.Log)) 

				_dataDirectories.Add(FileType.Log,  

					new DataDirectoryConfig  

                    {  

                        FileTypeStr = "Log",  

                        NeedToArchive = true,  

                        Path = Application.LogFileFolder  

                    }); 

        } 

 

 

		/// <summary> 

		/// Получение нового конфига 

		/// </summary> 

		/// <param name="newConfig">новый конфиг</param> 

		public override void ApplyNewConfig(SubsystemConfig newConfig) 

		{ 

			// инициализируемся с новым конфигом 


			Init(newConfig); 

		} 

 

 

        /// <summary> 

        /// Возвращает размер свободного места на диске по указанному корневому пути 

        /// </summary> 

        /// <param name="rootName">имя диска</param> 

        /// <returns>-1 - определить не удалось, иначе - размер свободного места</returns> 

        private long GetAvailableFreeSpace(string rootName) 

        { 

            try 

            { 

                // найдем точку монтирования (диск) с соответствующим названием корневой директории 

                DriveInfo drive = DriveInfo.GetDrives().First( 

                    d => d.Name == rootName); 

 

 

                return drive.AvailableFreeSpace; 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.FileSystemCheckFreeSpaceError, ex, rootName); 

                return -1; 

            } 

        } 

 

 

        /// <summary> 

        /// Получить имя Диска(точки монтирования) с директорией 

        /// </summary> 

        /// <param name="directoryName"></param> 

        /// <returns></returns> 

        private string GetDirectoryDriveName(string directoryName) 

        { 

            var dir = new DirectoryInfo(directoryName); 

            DriveInfo drive = null; 

 

 

            if (PlatformDetector.IsUnix) 

            { 

                // попытка разрешить все символические линки 

                // идем по всем каталогам, поднимаясь вверх  

                // останавливаемся на первом символическом линке или если дошли до корня 

                UnixDirectoryInfo di = new UnixDirectoryInfo(directoryName); 

                while (di.FullName != "/") 

                { 

                    UnixDirectoryInfo temp = ResolveSymbolicLinks(di.FullName); 

                    if (di.FullName != temp.FullName) 

                    { 


                        di = temp; 

                        break; 

                    } 

 

 

                    di = di.Parent; 

                } 

 

 

                // если у нас не было символических ссылок -  

                // значит используем исходное имя для получения "диска" 

                if (di.FullName != "/") 

                { 

                    dir = new DirectoryInfo(di.FullName); 

                } 

 

 

                // найдем точку монтирования (диск) с соответствующим названием корневой директории 

                // в UNIX для любой директории для Root.FullName вернется "/", поэтому из всех точек монтировани берем такую, 

                // что ее полный путь содержится в названии директории (кроме "/", так как он везде содержится) 

                drive = DriveInfo.GetDrives().FirstOrDefault(d => 

                    dir.FullName.StartsWith(d.RootDirectory.FullName) && d.RootDirectory.FullName != "/"); 

            } 

            else 

            { 

                // поиск диска для Windows 

                drive = DriveInfo.GetDrives().FirstOrDefault(d => 

                    dir.FullName.StartsWith(d.RootDirectory.FullName, StringComparison.InvariantCultureIgnoreCase)); 

            } 

 

 

            // если не нашли среди не рутовых дисков вернем root 

            if (drive == null) 

                return "/"; 

 

 

            return drive.Name; 

        } 

 

 

        /// <summary> 

        /// Метод разрешает все символические линки переданного каталога 

        /// </summary> 

        /// <param name="directoryName">Полный путь</param> 

        /// <returns>Разрешенное имя</returns> 

        private UnixDirectoryInfo ResolveSymbolicLinks(string directoryName) 

        { 

            UnixDirectoryInfo di = new UnixDirectoryInfo(directoryName); 

            // пока текущий каталог символическая ссылка 

            while (true) 


            { 

                try 

                { 

                    // разрешаем ссылки 

                    UnixSymbolicLinkInfo si = new UnixSymbolicLinkInfo(di.FullName); 

                    di = new UnixDirectoryInfo(si.ContentsPath); 

                } 

                catch 

                { 

                    // если исключение - значит разрешили все символические линки 

                    break; 

                } 

            } 

 

 

            return di; 

        } 

 

 

        #region IFileSystemManager 

 

 

        /// <summary> 

        /// Выполнить сброс буферов файловой системы на диски 

        /// </summary> 

        public void Sync() 

        { 

            try 

            { 

                Mono.Unix.Native.Syscall.sync(); 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.FileSystemSyncError, ex); 

#if !DEBUG 

                //throw ex; 

#endif 

            } 

        } 

 

 

        /// <summary> 

        /// Зарезервировать место на диске для создания файла по указанному пути с заданным размером 

        /// </summary> 

        /// <param name="fileType">тип файла, для которого нужно зарезервировать место</param> 

        /// <param name="path">путь, по которому будет создан файл</param> 

        /// <param name="requiredSize">требуемый размер свободного места (размер файла)</param> 

        /// <param name="minSize">минимально необходимый размер свободного места</param> 

        /// <param name="availableSize">текущий размер доступного места на диске</param> 

        /// <returns>true - место зарезервировано, false - недостаточно места на диске</returns> 


        public bool ReserveDiskSpace( 

            FileType fileType, string path, long requiredSize, long minSize, out long availableSize) 

        { 

            // если кончилось место, то писать в лог 

            // пишем в лог об окончание места на диске, но только один раз 

            // Logger.LogError(Message.RecognizerSaveImageNotEnoughFreeSpace,  

            //    _config.ImageDirectory.Path, availableSize, requiredSize); 

 

 

            lock (s_freeDiskSpacesSync) 

            { 

                var rootFullName = GetDirectoryDriveName(path); 

                availableSize = 0; 

 

 

                if (!s_freeDiskSpaces.ContainsKey(rootFullName)) 

                { 

                    // выполним определение свободного места 

                    availableSize = GetAvailableFreeSpace(rootFullName); 

                    s_freeDiskSpaces.Add(rootFullName, availableSize); 

 

 

                    Logger.LogInfo(Message.FileSystemDiscSpace, rootFullName, path, availableSize); 

                } 

                else 

                { 

                    availableSize = s_freeDiskSpaces[rootFullName]; 

 

 

                    // если свободного места не хватает 

                    if (availableSize < minSize) 

                    { 

                        // то попробуем еще раз его получить - вдруг место появилось 

                        availableSize = GetAvailableFreeSpace(rootFullName); 

                        s_freeDiskSpaces[rootFullName] = availableSize; 

                    } 

                } 

 

 

                // если свободного места не хватает 

                if (availableSize < minSize) 

                    // попробуем освободить место, если не удалось, то все плохо 

					if(!ClearDisk(fileType, rootFullName, minSize)) 

						return false; 

 

 

                // иначе, вычтем запрашиваемый размер 

                s_freeDiskSpaces[rootFullName] -= requiredSize; 

                return true; 

            } 


        } 

 

 

        /// <summary> 

        /// Записать текст в файл 

        /// </summary> 

        /// <param name="fileType">тип файла, в который нужно записать текст</param> 

        /// <param name="path">путь к файлу</param> 

        /// <param name="mode">режим записи в файл</param> 

        /// <param name="text">текст для записи</param> 

        /// <returns>true - текст записан, false - ошибка записи или недостаточно места на диске</returns> 

        public bool WriteTextToFile(FileType fileType, string path, FileMode mode, string text) 

        { 

            try 

            { 

                var requiredSize = text.Length * 2; 

                long availableSize; 

                if (!ReserveDiskSpace(fileType, new FileInfo(path).DirectoryName, requiredSize, requiredSize, out availableSize)) 

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

                Logger.LogException(Message.FileSystemWriteToFileError, ex, path); 

                return false; 

            } 

        } 

 

 

        /// <summary> 

        /// Архивировать файлы, которые были созданы во время работы 

        /// </summary> 

		/// <param name="archivePrefix">Префикс в названии архива</param> 

        /// <remarks>Менеджер файловой системы создает папку архива, в которую переносит 

        /// файлы, подлежащие архивированию</remarks> 

        public void ArchiveFiles(string archivePrefix) 

        { 

			FileUtils.EnsureDirExists(_config.ArchivesFolderPath.Value); 

			// создадим директорию архива с уникальным именем 

			_lastArchivePath =  

				FileUtils.CreateUniqueFolder(_config.ArchivesFolderPath.Value, archivePrefix + "archive", 6); 


 
 

			// список директорий для архивации, упорядоченный по полному пути 

			SortedList<string, DataDirectoryConfig> archiveDirs = new SortedList<string, DataDirectoryConfig>(); 

			// сформируем список 

			foreach (var directory in _dataDirectories.Values) 

			{ 

				// если архивировать не надо, то пропустим 

				if (!directory.NeedToArchive) 

					continue; 

				DirectoryInfo di = new DirectoryInfo(directory.Path); 

				if (di.Exists) 

				{ 

					archiveDirs.Add(di.FullName, directory); 

				} 

			} 

 

 

			// если есть вложенные директории, по сначала переместим их, а затем те, в которые они вложены 

 			for (var i = archiveDirs.Count - 1; i >= 0; i--) 

			{ 

				bool archiveResult = false; 

				// для лога отдельная архивация 

				if (archiveDirs.Values[i].FileType == FileType.Log) 

					archiveResult = ArchiveLogs(_lastArchivePath, archiveDirs.Values[i].Path); 

				// архивируем директорию 

				else 

					archiveResult = ArchiveDirectory(archiveDirs.Values[i], _lastArchivePath); 

 

 

				if(!archiveResult)	 

					Logger.LogError(Message.Exception,  

                        "Не удалось выполнить архивацию директории " + archiveDirs.Values[i].Path); 

			} 

        } 

 

 

		/// <summary> 

		/// Архивирует директорию с файлами определенного типа 

		/// </summary> 

		/// <param name="dirConfig">конфиг директории, которую копируем</param> 

		/// <param name="archiveFolderPath">Путь к архиву</param> 

		/// <returns></returns> 

		private bool ArchiveDirectory(DataDirectoryConfig dirConfig, string archiveFolderPath) 

		{ 

			var directory = new DirectoryInfo(dirConfig.Path); 

			// путь к директории в архиве 

			var newDirectoryPath = Path.Combine(archiveFolderPath, Path.GetFileName(dirConfig.Path)); 

 

 


			long availableSize; 

			// получим размер директории 

			var dirSize = DirSize(directory); 

			// попробуем зарезервировать место на диске 

			if (!ReserveDiskSpace(dirConfig.FileType, newDirectoryPath, dirSize, dirSize, out availableSize)) 

				return false; 

 

 

            var movingDirectoryPath = dirConfig.Path; 

 

 

			// переместим директорию в архив 

			try 

			{ 

				Directory.Move(movingDirectoryPath, newDirectoryPath); 

				// если успешно переместили освободим, занимаемое директорией место,  

				FreeDiskSpace(movingDirectoryPath, dirSize); 

                return true; 

			} 

			catch (Exception ex) 

			{ 

				Logger.LogException(Message.Exception, ex); 

				// освободим зарезервированный размер под диру 

				FreeDiskSpace(newDirectoryPath, dirSize); 

                return false; 

			} 

		} 

 

 

		/// <summary> 

		/// Получение пути к директории содержащей файлы определенного типа 

		/// </summary> 

		/// <param name="type">тип файла для которого хотим получить директорию</param> 

		/// <returns>Путь к директории</returns> 

		/// <exception cref="System.Exception"></exception> 

		public string GetDataDirectoryPath(FileType type) 

		{ 

			if (_dataDirectories.ContainsKey(type)) 

				return _dataDirectories[type].Path; 

 

 

			throw new Exception(String.Format("Не найдена папка для хранения файлов типа {0}", type)); 

		} 

 

 

        /// <summary> 

        /// Стадия безопасной сериализации 

        /// </summary> 

        private enum SafeSerializationStage 

        { 


            /// <summary> 

            /// Сериализация 

            /// </summary> 

            Serialization, 

            /// <summary> 

            /// Синхронизация файловой системы 

            /// </summary> 

            Sync, 

            /// <summary> 

            /// Удаление 

            /// </summary> 

            Delete, 

            /// <summary> 

            /// Переименование 

            /// </summary> 

            Move, 

            /// <summary> 

            /// Все выполнено 

            /// </summary> 

            AllDone, 

        } 

 

 

        /// <summary> 

        /// Безопасная (с точки зрения атомарности файловых операций) сериализация объекта в файл 

        /// </summary> 

        /// <param name="fileType">Тип файла</param> 

        /// <param name="objectToSerialize">Сереализуемый объект</param> 

        /// <param name="formatter">Форматтер</param> 

        /// <param name="fullName">Полный путь к файлу</param> 

        /// <returns>Сохранен/не сохранен</returns> 

        public bool SafeSerialization(FileType fileType, object objectToSerialize, IFormatter formatter, string fullName) 

        { 

            // получить временный файл 

            string backupFileName = fullName + BACKUP_EXT; 

            string fullPath = new FileInfo(fullName).DirectoryName; 

            string tempFileName = GetTempFileName(fullPath); 

 

 

            // считаем, что для сериализации объекта нам достаточно 256Кбайт 

            const long requiredSize = 1024 * 256; 

 

 

            long avalibleSize; 

            if (ReserveDiskSpace(fileType, fullPath, requiredSize, requiredSize, out avalibleSize)) 

            { 

                // стадия сохранения (для протоколирования ошибки) 

                SafeSerializationStage stage = SafeSerializationStage.Serialization; 

                try 

                { 


                    // записываем в файл сериализованный объект 

                    using (var stream = File.Open(tempFileName, FileMode.Create, FileAccess.Write, FileShare.Read)) 

                    { 

                        formatter.Serialize(stream, objectToSerialize); 

                        stream.Flush(); 

                    } 

 

 

                    stage = SafeSerializationStage.Sync; 

 

 

                    // синхронизация файловой системы 

                    Sync(); 

 

 

                    stage = SafeSerializationStage.Delete; 

 

 

                    if (File.Exists(fullName)) 

                    { 

                        // если есть резервная копия 

                        if(File.Exists(backupFileName)) 

                        { 

                            // удалим ее 

                            File.Delete(backupFileName); 

                        } 

                        // скопируем файл в резервную копию 

                        File.Move(fullName, backupFileName); 

                    } 

 

 

                    stage = SafeSerializationStage.Move; 

 

 

                    File.Move(tempFileName, fullName); 

 

 

                    stage = SafeSerializationStage.AllDone; 

 

 

                    // обновим бакап 

                    if(File.Exists(backupFileName)) 

                    { 

                        File.Delete(backupFileName); 

                    } 

                    File.Copy(fullName, backupFileName); 

 

 

                    // все ок 

                    return true; 


                } 

                catch (Exception ex) 

                { 

                    // запротоколировать неудачу 

                    Logger.LogException(Message.FileSystemSafeSerializationFailed, ex, objectToSerialize.GetType().FullName, stage); 

                } 

            } 

 

 

            // нет места для сохранения файла или произошла ошибка при сохранении 

            return false; 

        } 

 

 

 

 

        /// <summary> 

        /// Возвращает имя временного файла по указанному пути 

        /// </summary> 

        /// <param name="path">Путь</param> 

        /// <returns>Имя временного файла</returns> 

        public string GetTempFileName(string path) 

        { 

            FileInfo tempFile = new FileInfo(Path.GetTempFileName()); 

            return Path.Combine(path, tempFile.Name); 

        } 

 

 

        #endregion 

 

 

		/// <summary> 

		/// Определяет размер директории 

		/// </summary> 

		/// <param name="d">директория</param> 

		/// <returns>размер в байтах</returns> 

		private static long DirSize(DirectoryInfo d) 

		{ 

			long size = 0; 

			// если нет директории вернем 0 

			if (!d.Exists) 

				return 0; 

 

 

			// размер файлов  

			FileInfo[] fis = d.GetFiles(); 

			foreach (FileInfo fi in fis) 

			{ 

				size += fi.Length; 

			} 


 
 

			// размер поддиректорий 

			DirectoryInfo[] dis = d.GetDirectories(); 

			foreach (DirectoryInfo di in dis) 

			{ 

				size += DirSize(di); 

			} 

 

 

			return size; 

		} 

 

 

		/// <summary> 

		/// Проверка, что директории расположены на одном диске 

		/// </summary> 

		/// <param name="firstDirPath">путь первой директории</param> 

		/// <param name="secondDirPath">путь второй директории</param> 

		/// <returns>да/нет</returns> 

		private bool AreDirsOnSameDisk(string firstDirPath, string secondDirPath) 

		{ 

            var drive1 = GetDirectoryDriveName(firstDirPath); 

            var drive2 = GetDirectoryDriveName(secondDirPath); 

 

 

            if (drive1 == drive2) 

				return true; 

 

 

			return false; 

		} 

 

 

		/// <summary> 

		/// Освобождает занимаемое место 

		/// </summary> 

		/// <param name="path">путь к ресурсу, который освободил место</param> 

		/// <param name="sizeToFree">освобожденное место</param> 

		private void FreeDiskSpace(string path, long sizeToFree) 

		{ 

			lock (s_freeDiskSpacesSync) 

			{ 

                var rootFullName = GetDirectoryDriveName(path); 

				if (s_freeDiskSpaces.ContainsKey(rootFullName)) 

				{ 

					s_freeDiskSpaces[rootFullName] += sizeToFree; 

				} 

			} 

		} 


 
 

		/// <summary> 

		/// Очистка места на диске 

		/// </summary> 

		/// <param name="type">тип файла пославший запрос на очистку</param> 

		/// <param name="directoryRoot">корневая директория очищаемого диска</param> 

		/// <param name="requiredSize">необходимый размер для очистки</param> 

		/// <returns>удалось ли очистить необходимое место</returns> 

		private bool ClearDisk(FileType type, string directoryRoot, long requiredSize) 

		{ 

			// если ничего удалять нельзя 

			if (string.IsNullOrEmpty(_config.CleanOrder.Value)) 

				return false; 

			// получим порядок удаления архивных папок, для освобождения места 

			var cleanOrder = _config.CleanOrder.Value.Split(','); 

 

 

			// если необходимый для очистки размер меньше чем минимальный, очищаем минимальный 

			if (requiredSize < _config.MinDiskSpaceToFreeMb.Value * BYTES_IN_KB * BYTES_IN_KB) 

                requiredSize = _config.MinDiskSpaceToFreeMb.Value * BYTES_IN_KB * BYTES_IN_KB; 

 

 

			// если директория с архивами есть и она на том же диске что и очищаемая 

			if (Directory.Exists(_config.ArchivesFolderPath.Value)  

				&& AreDirsOnSameDisk(directoryRoot, _config.ArchivesFolderPath.Value)) 

				// почистим архивы 

				if (ClearArchives(directoryRoot, requiredSize, cleanOrder)) 

					return true; 

 

 

			// попробуем почистить текущие файлы 

			if (ClearCurrentData(directoryRoot, requiredSize, cleanOrder)) 

				return true; 

 

 

			return false; 

		} 

 

 

		/// <summary> 

		/// Удаляем файлы из архивов для освобождения места 

		/// </summary> 

		/// <param name="directoryRoot">корневая директория освобождаемого диска</param> 

		/// <param name="requiredSize">необходимый размер свободного места</param> 

		/// <param name="cleanOrder">порядок удаления файлов</param> 

		/// <returns>успех освобождения</returns> 

		private bool ClearArchives(string directoryRoot, long requiredSize, string[] cleanOrder) 

		{ 

			// получим существующие архивы, отсортированные по имени папки 


			SortedList<string, string> archives = new SortedList<string, string>(); 

			foreach (var dir in Directory.GetDirectories(_config.ArchivesFolderPath.Value)) 

			{ 

				archives.Add(Path.GetFileName(dir), dir); 

			} 

 

 

			// пройдемся по архивам(от старых к более новым) и почистим их 

			foreach (var archive in archives) 

			{ 

				foreach (var fileTypeStr in cleanOrder) 

				{ 

					var deletedPath = GetArchiveTypedFolderName(fileTypeStr); 

					// если удаление прошло успешно, проверим место на диске 

					if (DeleteFolder(Path.Combine(archive.Value, deletedPath))) 

						if (s_freeDiskSpaces[directoryRoot] > requiredSize) 

							return true; 

				} 

			} 

 

 

			return false; 

		} 

 

 

		/// <summary> 

		/// Очищение текущей информации 

		/// </summary> 

		/// <param name="directoryRoot">корневая директория освобождаемого диска</param> 

		/// <param name="requiredSize">необходимый размер свободного места</param> 

		/// <param name="cleanOrder">порядок удаления файлов</param> 

		/// <returns></returns> 

		private bool ClearCurrentData(string directoryRoot, long requiredSize, string[] cleanOrder) 

		{ 

			foreach (var fileTypeStr in cleanOrder) 

			{ 

                var deletedPath = GetDataDirectoryPath((FileType)Enum.Parse(typeof(FileType), fileTypeStr)); 

				// если на разных дисках или удалена то продолжаем 

				if (!Directory.Exists(deletedPath) || !AreDirsOnSameDisk(deletedPath, directoryRoot)) 

					continue; 

 

 

				// сначала удалим файлы 

				foreach (var file in Directory.GetFiles(deletedPath)) 

				{ 

					if(DeleteFile(file)) 

						if (s_freeDiskSpaces[directoryRoot] > requiredSize) 

							return true; 

				} 

 


 
				// потом удалим папки 

				foreach (var folder in Directory.GetDirectories(deletedPath)) 

				{ 

					if (DeleteFolder(folder)) 

						if (s_freeDiskSpaces[directoryRoot] > requiredSize) 

							return true; 

				} 

			} 

 

 

			return false; 

		} 

 

 

		/// <summary> 

		/// Удаление директории 

		/// </summary> 

		/// <param name="path">путь к директории</param> 

		/// <returns>успех операции</returns> 

		private bool DeleteFolder(string path) 

		{ 

			DirectoryInfo di = new DirectoryInfo(path); 

			// если директории уже нет, все хорошо 

			if (!di.Exists) 

				return true; 

 

 

			try 

			{ 

				var dirSize = DirSize(di); 

				di.Delete(true); 

 

 

				// освободим место на диске 

				FreeDiskSpace(path, dirSize); 

 

 

				return true; 

			} 

			catch (Exception ex) 

			{ 

				Logger.LogException(Message.Exception, ex, "Не удалось удалить архивную директорию"); 

				return false; 

			} 

		} 

 

 

		/// <summary> 

		/// Удаление файла 


		/// </summary> 

		/// <param name="path">путь к файлу</param> 

		/// <returns>успех операции</returns> 

		public bool DeleteFile(string path) 

		{ 

			FileInfo fi = new FileInfo(path); 

			// если директории уже нет, все хорошо 

			if (!fi.Exists) 

				return true; 

 

 

			try 

			{ 

				var fileSize = fi.Length; 

				fi.Delete(); 

 

 

				// освободим место на диске 

				FreeDiskSpace(path, fileSize); 

 

 

				return true; 

			} 

			catch (Exception ex) 

			{ 

				Logger.LogException(Message.Exception, ex, "Не удалось удалить файл " + path); 

				return false; 

			} 

		} 

 

 

		/// <summary> 

		/// Получает имя директории с определенным типом файлов в архиве 

		/// </summary> 

		/// <param name="fileTypeStr">тип файла</param> 

		/// <returns>имя директории в архиве</returns> 

		private string GetArchiveTypedFolderName(string fileTypeStr) 

		{ 

			FileType fileType = (FileType)Enum.Parse(typeof(FileType), fileTypeStr); 

			return Path.GetFileName(_dataDirectories[fileType].Path); 

		} 

 

 

		#region Архивирование логов 

 

 

		/// <summary> 

		/// Архивирование логов(файл текущего лога держится приложением, поэтому переместить его сразу нельзя 

		/// , нужно подождать события закрытия приложения и перекопировать его отдельно) 

		/// </summary> 


		private bool ArchiveLogs(string archivePath, string logPath) 

		{ 

			var logDirInfo = new DirectoryInfo(logPath); 

			var logSize = DirSize(logDirInfo); 

			long avalibleSize; 

 

 

			if (!ReserveDiskSpace(FileType.Log, archivePath, logSize, logSize, out avalibleSize)) 

				return false; 

 

 

			MoveFolder(logPath, Path.Combine(archivePath, Path.GetFileName(logPath))); 

			var newLogSize = DirSize(logDirInfo); 

 

 

			// если какие-то логи переместились освободим место 

			if (logSize - newLogSize > 0) 

				FreeDiskSpace(logPath, logSize - newLogSize); 

 

 

			// подпишемся на событие закрытия приложения, чтобы перезаписать, те логи которые были недоступны 

			CoreApplication.Instance.Exited += new EventHandler(MoveLastLogs); 

 

 

			return true; 

		} 

 

 

		/// <summary> 

		/// Допереместить логи в архив при закрытии приложения  

		/// </summary> 

		/// <param name="sender">CoreApplication</param> 

		/// <param name="e">параметры события</param> 

		private void MoveLastLogs(object sender, EventArgs e) 

		{ 

			// путь к логам 

			var logPath = _dataDirectories[FileType.Log].Path; 

			var logDirInfo = new DirectoryInfo(logPath); 

			// необходимый размер для логов 

			var logSize = DirSize(logDirInfo); 

			long avalibleSize; 

 

 

			// если удастся зарезервировать место, выполним перезапись 

			if (ReserveDiskSpace(FileType.Log, _lastArchivePath, logSize, logSize, out avalibleSize)) 

				MoveFolder(logPath, Path.Combine(_lastArchivePath, Path.GetFileName(logPath))); 

		} 

 

 

		/// <summary> 


		/// Пытается переместить директорию из одного места в другое, при неудаче перемещения какого либо 

		/// файла просто копирует его. 

		/// </summary> 

		/// <param name="folderPath">Перемещаемая директория</param> 

		/// <param name="destinationPath">Куда перемещать</param> 

		private void MoveFolder(string folderPath, string destinationPath) 

		{ 

			FileUtils.EnsureDirExists(destinationPath); 

 

 

			foreach (var file in Directory.GetFiles(folderPath)) 

			{ 

				try 

				{ 

					var newFilePath = Path.Combine(destinationPath, Path.GetFileName(file)); 

					// удалим файл, если он был иначе Move упадет 

					DeleteFile(newFilePath); 

 

 

					File.Move(file, newFilePath); 

				} 

				catch (Exception) 

				{ 

					// если не удалось переместить, скопируем с перезаписью 

					File.Copy(file, Path.Combine(destinationPath, Path.GetFileName(file)), true); 

				} 

			} 

 

 

			foreach (var directory in Directory.GetDirectories(folderPath)) 

			{ 

				var newDestinationDir = Path.Combine(destinationPath, Path.GetFileName(directory)); 

				// вызовем перемещение 

				MoveFolder(directory, newDestinationDir); 

			} 

 

 

			// попытаемся удалить перемещенную директорию 

			try 

			{ 

				Directory.Delete(folderPath); 

			} 

			// ничего не делаем 

			catch { } 

		} 

 

 

		#endregion 

	} 

}



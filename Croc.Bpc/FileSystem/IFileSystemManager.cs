using System.IO; 

using System.Runtime.Serialization; 

using Croc.Core; 

 

 

namespace Croc.Bpc.FileSystem 

{ 

    /// <summary> 

    /// Интерфейс менеджера файловой системы 

    /// </summary> 

    public interface IFileSystemManager : ISubsystem 

    { 

        /// <summary> 

        /// Выполнить сброс буферов файловой системы на диски 

        /// </summary> 

        void Sync(); 

 

 

        /// <summary> 

        /// Зарезервировать место на диске для создания файла по указанному пути с заданным размером 

        /// </summary> 

        /// <param name="fileType">тип файла, для которого нужно зарезервировать место</param> 

        /// <param name="path">путь, по которому будет создан файл</param> 

        /// <param name="requiredSize">требуемый размер свободного места (размер файла)</param> 

        /// <param name="minSize">минимально необходимый размер свободного места</param> 

        /// <param name="availableSize">текущий размер доступного места на диске</param> 

        /// <returns>true - место зарезервировано, false - недостаточно места на диске</returns> 

        bool ReserveDiskSpace( 

            FileType fileType, string path, long requiredSize, long minSize, out long availableSize); 

 

 

        /// <summary> 

        /// Записать текст в файл 

        /// </summary> 

        /// <param name="fileType">тип файла, в который нужно записать текст</param> 

        /// <param name="path">путь к файлу</param> 

        /// <param name="mode">режим записи в файл</param> 

        /// <param name="text">текст для записи</param> 

        /// <returns>true - текст записан, false - ошибка записи или недостаточно места на диске</returns> 

        bool WriteTextToFile(FileType fileType, string path, FileMode mode, string text); 

 

 

        /// <summary> 

        /// Архивировать файлы, которые были созданы во время работы 

        /// </summary> 

		/// <param name="archivePrefix">Префикс в названии архива</param> 

		/// <remarks>Менеджер файловой системы создает папку архива, в которую переносит 

        /// файлы, подлежащие архивированию</remarks> 

		void ArchiveFiles(string archivePrefix); 

 


 
		/// <summary> 

		/// Получение пути к директории содержащей файлы определенного типа 

		/// </summary> 

		/// <param name="type">тип файла для которого хотим получить директорию</param> 

		/// <returns>Путь к директории</returns> 

		string GetDataDirectoryPath(FileType type); 

 

 

        /// <summary> 

        /// Безопасная (с точки зрения атомарности файловых операций) сериализация объекта в файл 

        /// </summary> 

        /// <param name="fileType">Тип файла</param> 

        /// <param name="objectToSerialize">Сереализуемый объект</param> 

        /// <param name="formatter">Форматтер</param> 

        /// <param name="fullName">Полный путь к файлу</param> 

        /// <returns>Сохранен/не сохранен</returns> 

        bool SafeSerialization(FileType fileType, object objectToSerialize, IFormatter formatter, string fullName); 

 

 

        /// <summary> 

        /// Возвращает имя временного файла по указанному пути 

        /// </summary> 

        /// <param name="path">Путь</param> 

        /// <returns>Имя временного файла</returns> 

        string GetTempFileName(string path); 

    } 

}



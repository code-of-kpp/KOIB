using System; 
using System.IO; 
using System.Runtime.Serialization; 
using Croc.Core; 
namespace Croc.Bpc.FileSystem 
{ 
    public interface IFileSystemManager : ISubsystem 
    { 
        bool ReserveDiskSpace(string path, int requiredSizeKb); 
        bool ReserveDiskSpace( 
            string path, int requiredSizeKb, int minSizeKb, out long availableSize); 
        bool WriteTextToFile(string path, FileMode mode, string text, bool doCleanUp); 
        void ArchiveFiles(string archivePrefix); 
        string GetDataDirectoryPath(FileType type); 
        string GetTempFileName(string path); 
        event EventHandler<FilesArchivedEventArgs> FilesArchived; 
        #region Сериализация и десериализация 
        bool SafeSerialization(object objectToSerialize, IFormatter formatter, string fullName, bool makeBackup, bool doCleanUp); 
        bool SafeDeserialization<T>(out T deserializedObject, IFormatter formatter, string fullName); 
        #endregion 
    } 
}

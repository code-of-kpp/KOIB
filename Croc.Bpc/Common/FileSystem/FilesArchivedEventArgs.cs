using System; 
namespace Croc.Bpc.FileSystem 
{ 
    public class FilesArchivedEventArgs : EventArgs 
    { 
        public readonly string ArchiveName; 
        public FilesArchivedEventArgs(string archiveName) 
        { 
            ArchiveName = archiveName; 
        } 
    } 
}

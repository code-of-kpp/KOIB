using System; 
using System.IO; 
using System.Linq; 
namespace Croc.Core.Utils.IO 
{ 
    public class FileUtils 
    { 
        public const int BYTES_IN_KB = 1024; 
        public const int BYTES_IN_MB = BYTES_IN_KB * BYTES_IN_KB; 
        public static FileStream CreateUniqueFileWithDateMark( 
            String folder, String fileNamePrefix, String fileExtension, Int32 fileNumberLength) 
        { 
            return CreateUniqueFile( 
                folder, 
                String.Format("{0}.{1:yyyyMMdd}.", fileNamePrefix, DateTime.Now), 
                fileExtension, 
                fileNumberLength); 
        } 
        public static FileStream CreateUniqueFile( 
            String folder, String fileNamePrefix, String fileExtension, Int32 fileNumberLength) 
        { 
            const Int32 MAX_TRIES = 3; 
            Int32 tryCount = 0; 
            while (true) 
            { 
                try 
                { 
                    String fileName = GetUniqueName(folder, fileNamePrefix, fileExtension, fileNumberLength); 
                    return new FileStream( 
                        Path.Combine(folder, fileName), 
                        FileMode.CreateNew, 
                        FileAccess.Write, 
                        FileShare.Read); 
                } 
                catch (IOException) 
                { 
                    tryCount++; 
                    if (tryCount >= MAX_TRIES) 
                        throw; 
                } 
            } 
        } 
        public static string CreateUniqueFolder(String folder, String folderNamePrefix, Int32 folderNumberLength) 
        { 
            const Int32 MAX_TRIES = 3; 
            folderNamePrefix = String.Format("{0}_{1:yyyyMMdd}.", folderNamePrefix, DateTime.Now); 
            Int32 tryCount = 0; 
            while (true) 
            { 
                try 
                { 
                    String folderName = GetUniqueName(folder, folderNamePrefix, "", folderNumberLength); 
                    folderName = Path.Combine(folder, folderName); 
                    Directory.CreateDirectory(folderName); 


                    return folderName; 
                } 
                catch (IOException) 
                { 
                    tryCount++; 
                    if (tryCount >= MAX_TRIES) 
                        throw; 
                } 
            } 
        } 
        public static string GetUniqueName( 
            String folder, String namePrefix, String fileExtension, Int32 numberLength) 
        { 
            if (!String.IsNullOrEmpty(fileExtension)) 
                fileExtension = "." + fileExtension; 
            String searchPattern = namePrefix + new String('?', numberLength) + fileExtension; 
            var files = Directory.GetFileSystemEntries(folder, searchPattern); 
            String lastFile = files.Length == 0 ? null : files.Max(); 
            Int32 orderNumber = 0; 
            if (lastFile != null) 
            { 
                String sLogNumber = Path.GetFileName(lastFile).Substring(namePrefix.Length, numberLength); 
                Int32.TryParse(sLogNumber, out orderNumber); 
            } 
            orderNumber++; 
            String name = 
                namePrefix + 
                orderNumber.ToString(new String('0', numberLength)) + 
                fileExtension; 
            return name; 
        } 
        public static String GetRelativePath(String folder, String path) 
        { 
            if (string.IsNullOrEmpty(path)) 
                return String.Empty; 
            if (!Path.IsPathRooted(path)) 
                return path; 
            var lastChar = folder[folder.Length - 1]; 
            if (lastChar != Path.DirectorySeparatorChar && lastChar != Path.AltDirectorySeparatorChar) 
                folder += Path.DirectorySeparatorChar; 
            var uri1 = new Uri(folder); 
            var uri2 = new Uri(path); 
            var relPath = Uri.UnescapeDataString(uri1.MakeRelativeUri(uri2).ToString()); 
            return relPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar); 
        } 
        public static void EnsureDirExists(string path) 
        { 
            if (!Directory.Exists(path)) 
                Directory.CreateDirectory(path); 
        } 
    } 
}

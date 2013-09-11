using System; 
using System.Configuration; 
using Croc.Core.Configuration; 
namespace Croc.Bpc.FileSystem.Config 
{ 
    public class DataDirectoryConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("fileType", IsRequired = true)] 
        public string FileTypeStr 
        { 
            get 
            { 
                return (string)this["fileType"]; 
            } 
            set 
            { 
                this["fileType"] = value; 
            } 
        } 
        [ConfigurationProperty("path", IsRequired = true)] 
        public string Path 
        { 
            get 
            { 
                return (string)this["path"]; 
            } 
            set 
            { 
                this["path"] = value; 
            } 
        } 
        [ConfigurationProperty("archive", IsRequired = true)] 
        public bool Archive 
        { 
            get 
            { 
                return (bool)this["archive"]; 
            } 
            set 
            { 
                this["archive"] = value; 
            } 
        } 
        public FileType FileType 
        { 
            get 
            { 
                if(Enum.IsDefined(typeof(FileType), FileTypeStr)) 
                    return (FileType)Enum.Parse(typeof(FileType), FileTypeStr, true); 
                throw new ArgumentException( 
                    String.Format("Тип файлов {0} не содержится в перечислении FileType", FileTypeStr)); 
            } 
        } 
    } 
}

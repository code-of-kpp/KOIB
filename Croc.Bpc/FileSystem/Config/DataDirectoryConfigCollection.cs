using System; 
using System.Configuration; 
namespace Croc.Bpc.FileSystem.Config 
{ 
    public class DataDirectoryConfigCollection : ConfigurationElementCollection 
    { 
        protected override ConfigurationElement CreateNewElement() 
        { 
            return new DataDirectoryConfig(); 
        } 
        protected override Object GetElementKey(ConfigurationElement element) 
        { 
            return ((DataDirectoryConfig)element).FileTypeStr; 
        } 
    } 
}

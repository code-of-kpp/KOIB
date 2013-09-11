using System.Configuration; 
using Croc.Core.Configuration; 
namespace Croc.Bpc.FileSystem.Config 
{ 
    public class FileSystemManagerConfig : SubsystemConfig 
    { 
        [ConfigurationProperty("dataDirectories", IsDefaultCollection = false, IsRequired = true)] 
        [ConfigurationCollection(typeof(DataDirectoryConfigCollection), AddItemName = "dataDirectory")] 
        public DataDirectoryConfigCollection DataDirectories 
        { 
            get 
            { 
                return (DataDirectoryConfigCollection)base["dataDirectories"]; 
            } 
        } 
        [ConfigurationProperty("minDiskSpaceToFreeMb", IsRequired = true)] 
        public ValueConfig<int> MinDiskSpaceToFreeMb 
        { 
            get 
            { 
                return (ValueConfig<int>)base["minDiskSpaceToFreeMb"]; 
            } 
        } 
        [ConfigurationProperty("dataDirectoriesRoot", IsRequired = true)] 
        public ValueConfig<string> DataDirectoriesRoot 
        { 
            get 
            { 
                return (ValueConfig<string>)base["dataDirectoriesRoot"]; 
            } 
        } 
        [ConfigurationProperty("minFreeSpaceToSaveSerializedObjectKb", IsRequired = true)] 
        public ValueConfig<int> MinFreeSpaceToSaveSerializedObjectKb 
        { 
            get 
            { 
                return (ValueConfig<int>)base["minFreeSpaceToSaveSerializedObjectKb"]; 
            } 
        } 
        [ConfigurationProperty("systemReservedSpaceKb", IsRequired = true)] 
        public ValueConfig<int> SystemReservedSpaceKb 
        { 
            get 
            { 
                return (ValueConfig<int>)base["systemReservedSpaceKb"]; 
            } 
        } 
        [ConfigurationProperty("cleanOrder", IsRequired = true)] 
        public ValueConfig<string> CleanOrder 
        { 
            get 
            { 
                return (ValueConfig<string>)base["cleanOrder"]; 
            } 
        } 
    } 
}

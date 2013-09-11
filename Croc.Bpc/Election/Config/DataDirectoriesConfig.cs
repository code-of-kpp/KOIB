using System; 
using System.Configuration; 
using Croc.Core.Configuration; 
namespace Croc.Bpc.Election.Config 
{ 
    public class DataDirectoriesConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("sourceDataDirName", IsRequired = true)] 
        public string SourceDataDirName 
        { 
            get 
            { 
                return (string)this["sourceDataDirName"]; 
            } 
            set 
            { 
                this["sourceDataDirName"] = value; 
            } 
        } 
        [ConfigurationProperty("votingResultDirName", IsRequired = true)] 
        public string VotingResultDirName 
        { 
            get 
            { 
                return (string)this["votingResultDirName"]; 
            } 
            set 
            { 
                this["votingResultDirName"] = value; 
            } 
        } 
        [ConfigurationProperty("root", IsDefaultCollection = false, IsRequired = true)] 
        [ConfigurationCollection(typeof(ValueConfig<string>), AddItemName = "path")] 
        public PathConfigCollection RootPaths 
        { 
            get 
            { 
                return (PathConfigCollection)base["root"]; 
            } 
        } 
    } 
}

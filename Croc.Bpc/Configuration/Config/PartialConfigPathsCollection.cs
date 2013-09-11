using System.Collections.Generic; 
using System.Configuration; 
using Croc.Core.Configuration; 
namespace Croc.Bpc.Configuration.Config 
{ 
    public class PartialConfigurationPathsConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("includeSourceDataPaths", IsRequired = true)] 
        public bool IncludeSourceDataPaths 
        { 
            get 
            { 
                return (bool)this["includeSourceDataPaths"]; 
            } 
            set 
            { 
                this["includeSourceDataPaths"] = value; 
            } 
        } 
        [ConfigurationProperty("fileName", IsRequired = true)] 
        public string FileName 
        { 
            get 
            { 
                return (string)this["fileName"]; 
            } 
            set 
            { 
                this["fileName"] = value; 
            } 
        } 
        [ConfigurationProperty("root", IsDefaultCollection = false, IsRequired = true)] 
        [ConfigurationCollection(typeof(ValueConfig<string>), AddItemName = "path")] 
        public ValueConfigCollection<string> RootPaths 
        { 
            get 
            { 
                return (ValueConfigCollection<string>)base["root"]; 
            } 
        } 
        public List<string> ToList() 
        { 
            return RootPaths.ToList(); 
        } 
    } 
}

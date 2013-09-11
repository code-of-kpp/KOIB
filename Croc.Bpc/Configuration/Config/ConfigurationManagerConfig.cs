using Croc.Core.Configuration; 
using System.Configuration; 
namespace Croc.Bpc.Configuration.Config 
{ 
    public class ConfigurationManagerConfig : SubsystemConfig 
    { 
        [ConfigurationProperty("workingConfigFile", IsRequired = true)] 
        public ValueConfig<string> WorkingConfigFilePath 
        { 
            get 
            { 
                return (ValueConfig<string>)this["workingConfigFile"]; 
            } 
            set 
            { 
                this["workingConfigFile"] = value; 
            } 
        } 
        [ConfigurationProperty("partialConfigFile", IsRequired = true)] 
        public PartialConfigurationPathsConfig PartialConfigFileLocations 
        { 
            get 
            { 
                return (PartialConfigurationPathsConfig)base["partialConfigFile"]; 
            } 
        } 
        [ConfigurationProperty("privateConfigElements", IsDefaultCollection = false, IsRequired = false)] 
        [ConfigurationCollection(typeof(ValueConfig<string>), AddItemName = "xpath")] 
        public ValueConfigCollection<string> PrivateConfigElementXPaths 
        { 
            get 
            { 
                return (ValueConfigCollection<string>)base["privateConfigElements"]; 
            } 
        } 
    } 
}

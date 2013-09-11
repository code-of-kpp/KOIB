using System.Configuration; 
namespace Croc.Core.Configuration 
{ 
    public class EnabledConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("enabled", IsRequired = true)] 
        public bool Enabled 
        { 
            get 
            { 
                return (bool)this["enabled"]; 
            } 
            set 
            { 
                this["enabled"] = value; 
            } 
        } 
    } 
}

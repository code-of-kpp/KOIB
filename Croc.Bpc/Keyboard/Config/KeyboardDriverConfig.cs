using System.Configuration; 
using Croc.Core.Configuration; 
using System; 
namespace Croc.Bpc.Keyboard.Config 
{ 
    public class KeyboardDriverConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("type", IsRequired = true)] 
        public string Type 
        { 
            get 
            { 
                return (string)this["type"]; 
            } 
            set 
            { 
                this["type"] = value; 
            } 
        } 
        [ConfigurationProperty("settings", IsDefaultCollection = false, IsRequired = false)] 
        [ConfigurationCollection(typeof(SettingConfigCollection), AddItemName = "add")] 
        public SettingConfigCollection Settings 
        { 
            get 
            { 
                return (SettingConfigCollection)base["settings"]; 
            } 
        } 
    } 
}

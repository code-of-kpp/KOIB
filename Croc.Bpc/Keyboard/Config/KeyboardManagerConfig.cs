using System; 
using Croc.Core.Configuration; 
using System.Configuration; 
namespace Croc.Bpc.Keyboard.Config 
{ 
    public class KeyboardManagerConfig : SubsystemConfig 
    { 
        [ConfigurationProperty("driver", IsRequired = true)] 
        public KeyboardDriverConfig Driver 
        { 
            get 
            { 
                return (KeyboardDriverConfig)this["driver"]; 
            } 
            set 
            { 
                this["driver"] = value; 
            } 
        } 
        [ConfigurationProperty("keys", IsDefaultCollection = false, IsRequired = false)] 
        [ConfigurationCollection(typeof(KeyConfigCollection), AddItemName = "key")] 
        public KeyConfigCollection Keys 
        { 
            get 
            { 
                return (KeyConfigCollection)base["keys"]; 
            } 
        } 
    } 
}

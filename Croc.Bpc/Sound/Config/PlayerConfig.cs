using System; 
using System.Configuration; 
namespace Croc.Bpc.Sound.Config 
{ 
    public class PlayerConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("format", IsRequired = true)] 
        public string FormatString 
        { 
            get 
            { 
                return (string)this["format"]; 
            } 
            set 
            { 
                this["format"] = value; 
            } 
        } 
        public SoundPlayerType Format 
        { 
            get 
            { 
                return (SoundPlayerType)Enum.Parse(typeof(SoundPlayerType), FormatString, true); 
            } 
        } 
        [ConfigurationProperty("deviceLatency", IsRequired = true)] 
        public int DeviceLatency 
        { 
            get 
            { 
                return (int)this["deviceLatency"]; 
            } 
            set 
            { 
                this["deviceLatency"] = value; 
            } 
        } 
    } 
}

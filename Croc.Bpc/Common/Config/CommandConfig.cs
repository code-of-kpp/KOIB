using System.Configuration; 
namespace Croc.Bpc.Config 
{ 
    public class CommandConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("command", IsRequired = true)] 
        public string Command 
        { 
            get 
            { 
                return (string)this["command"]; 
            } 
            set 
            { 
                this["command"] = value; 
            } 
        } 
        [ConfigurationProperty("params", IsRequired = false)] 
        public string Params 
        { 
            get 
            { 
                return (string)this["params"]; 
            } 
            set 
            { 
                this["params"] = value; 
            } 
        } 
        [ConfigurationProperty("sleep", IsRequired = false)] 
        public int SleepInterval 
        { 
            get 
            { 
                return (int)this["sleep"]; 
            } 
            set 
            { 
                this["sleep"] = value; 
            } 
        } 
    } 
}

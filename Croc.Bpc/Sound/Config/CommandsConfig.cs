using System; 
using System.Configuration; 
using Croc.Bpc.Config; 
namespace Croc.Bpc.Sound.Config 
{ 
    public class CommandsConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("beforePlaying", IsRequired = false)] 
        public CommandConfig BeforePlaying 
        { 
            get 
            { 
                return (CommandConfig)this["beforePlaying"]; 
            } 
            set 
            { 
                this["beforePlaying"] = value; 
            } 
        } 
        [ConfigurationProperty("afterPlaying", IsRequired = false)] 
        public CommandConfig AfterPlaying 
        { 
            get 
            { 
                return (CommandConfig)this["afterPlaying"]; 
            } 
            set 
            { 
                this["afterPlaying"] = value; 
            } 
        } 
    } 
}

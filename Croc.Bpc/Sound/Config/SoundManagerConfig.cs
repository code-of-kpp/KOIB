using System.Configuration; 
using Croc.Core.Configuration; 
namespace Croc.Bpc.Sound.Config 
{ 
    public class SoundManagerConfig : SubsystemConfig 
    { 
        [ConfigurationProperty("player", IsRequired = true)] 
        public PlayerConfig Player 
        { 
            get 
            { 
                return (PlayerConfig)this["player"]; 
            } 
            set 
            { 
                this["player"] = value; 
            } 
        } 
        [ConfigurationProperty("commands", IsRequired = false)] 
        public CommandsConfig Commands 
        { 
            get 
            { 
                return (CommandsConfig)this["commands"]; 
            } 
            set 
            { 
                this["commands"] = value; 
            } 
        } 
    } 
}

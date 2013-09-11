using System; 
using System.Configuration; 
using Croc.Bpc.Voting; 
namespace Croc.Bpc.Election.Config 
{ 
    public class VotingModeTimeConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("mode", IsRequired = true)] 
        public VotingMode Mode 
        { 
            get 
            { 
                return (VotingMode)this["mode"]; 
            } 
            set 
            { 
                this["mode"] = value; 
            } 
        } 
        [ConfigurationProperty("time", IsRequired = false)] 
        public TimeSpan Time 
        { 
            get 
            { 
                return (TimeSpan)this["time"]; 
            } 
            set 
            { 
                this["time"] = value; 
            } 
        } 
    } 
}

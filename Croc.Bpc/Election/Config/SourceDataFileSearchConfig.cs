using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using System.Configuration; 
namespace Croc.Bpc.Election.Config 
{ 
    public class SourceDataFileSearchConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("maxTryCount", IsRequired = true)] 
        public uint MaxTryCount 
        { 
            get 
            { 
                return (uint)this["maxTryCount"]; 
            } 
            set 
            { 
                this["maxTryCount"] = value; 
            } 
        } 
        [ConfigurationProperty("delay", IsRequired = true)] 
        public TimeSpan Delay 
        { 
            get 
            { 
                return (TimeSpan)this["delay"]; 
            } 
            set 
            { 
                this["delay"] = value; 
            } 
        } 
    } 
}

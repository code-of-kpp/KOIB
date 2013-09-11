using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using System.Configuration; 
namespace Croc.Bpc.Synchronization.Config 
{ 
    public class CallPropertiesConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("timeout", IsRequired = true)] 
        public int Timeout 
        { 
            get 
            { 
                return (int)this["timeout"]; 
            } 
            set 
            { 
                this["timeout"] = value; 
            } 
        } 
        [ConfigurationProperty("maxTryCount", IsRequired = true)] 
        public int MaxTryCount 
        { 
            get 
            { 
                return (int)this["maxTryCount"]; 
            } 
            set 
            { 
                this["maxTryCount"] = value; 
            } 
        } 
        [ConfigurationProperty("retryDelay", IsRequired = true)] 
        public int RetryDelay 
        { 
            get 
            { 
                return (int)this["retryDelay"]; 
            } 
            set 
            { 
                this["retryDelay"] = value; 
            } 
        } 
    } 
}

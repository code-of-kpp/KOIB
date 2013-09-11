using System.Configuration; 
namespace Croc.Core.Configuration 
{ 
    public class MinMaxConfig<T> : ConfigurationElement 
    { 
        [ConfigurationProperty("min", IsRequired = true)] 
        public T Min 
        { 
            get 
            { 
                return (T)this["min"]; 
            } 
            set 
            { 
                this["min"] = value; 
            } 
        } 
        [ConfigurationProperty("max", IsRequired = true)] 
        public T Max 
        { 
            get 
            { 
                return (T)this["max"]; 
            } 
            set 
            { 
                this["max"] = value; 
            } 
        } 
    } 
}

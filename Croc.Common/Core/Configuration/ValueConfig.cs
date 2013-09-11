using System.Configuration; 
namespace Croc.Core.Configuration 
{ 
    public class ValueConfig<T> : ConfigurationElement 
    { 
        [ConfigurationProperty("value", IsRequired = true)] 
        public T Value 
        { 
            get 
            { 
                return (T)this["value"]; 
            } 
            set 
            { 
                this["value"] = value; 
            } 
        } 
    } 
}

using System.Configuration; 
namespace Croc.Core.Configuration 
{ 
    public class TypedConfigElement : ConfigurationElement 
    { 
        [ConfigurationProperty("type", IsRequired = true)] 
        public string TypeName 
        { 
            get 
            { 
                return (string)this["type"]; 
            } 
            set 
            { 
                this["type"] = value; 
            } 
        } 
    } 
}

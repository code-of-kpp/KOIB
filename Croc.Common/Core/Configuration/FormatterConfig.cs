using System.Configuration; 
namespace Croc.Core.Configuration 
{ 
    public class FormatterConfig : ConfigurableClassConfig 
    { 
        [ConfigurationProperty("raw", IsRequired = false, DefaultValue = false)] 
        public bool Raw 
        { 
            get 
            { 
                return (bool)this["raw"]; 
            } 
            set 
            { 
                this["raw"] = value; 
            } 
        } 
    } 
}

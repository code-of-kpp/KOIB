using System.Configuration; 
namespace Croc.Core.Configuration 
{ 
    public class ConfigurableClassConfig : TypedConfigElement 
    { 
        [ConfigurationProperty("props", IsDefaultCollection = false, IsRequired = false)] 
        public NameValueConfigurationCollection Props 
        { 
            get { return (NameValueConfigurationCollection)base["props"] ?? new NameValueConfigurationCollection(); } 
        } 
    } 
}

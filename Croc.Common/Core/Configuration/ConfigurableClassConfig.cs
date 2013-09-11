using System.Configuration; 

 

 

namespace Croc.Core.Configuration 

{ 

    /// <summary> 

    /// Конфигурация подключаемого класса 

    /// </summary> 

    public class ConfigurableClassConfig : TypedConfigElement 

    { 

        /// <summary> 

        /// Дополнительные свойства 

        /// </summary> 

        [ConfigurationProperty("props", IsDefaultCollection = false, IsRequired = false)] 

        public NameValueConfigurationCollection Props 

        { 

            get { return (NameValueConfigurationCollection)base["props"] ?? new NameValueConfigurationCollection(); } 

        } 

    } 

}



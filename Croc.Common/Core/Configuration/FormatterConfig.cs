using System.Configuration; 

 

 

namespace Croc.Core.Configuration 

{ 

    /// <summary> 

    /// Конфигурация форматировщика 

    /// </summary> 

    public class FormatterConfig : ConfigurableClassConfig 

    { 

        /// <summary> 

        /// Признак вывода в протокол без форматирования сообщений 

        /// </summary> 

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



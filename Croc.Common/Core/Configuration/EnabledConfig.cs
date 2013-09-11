using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

 

 

namespace Croc.Core.Configuration 

{ 

    /// <summary> 

    /// Конфиг-элемент, который содержит атрибут enabled 

    /// </summary> 

    public class EnabledConfig : ConfigurationElement 

    { 

        /// <summary> 

        /// Включен или выключен 

        /// </summary> 

        [ConfigurationProperty("enabled", IsRequired = true)] 

        public bool Enabled 

        { 

            get 

            { 

                return (bool)this["enabled"]; 

            } 

            set 

            { 

                this["enabled"] = value; 

            } 

        } 

    } 

}



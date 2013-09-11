using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

 

 

namespace Croc.Core.Configuration 

{ 

    /// <summary> 

    /// Конфиг-элемент, которые содержит строковую настройку с заданным ключом 

    /// </summary> 

    public class SettingConfig : ConfigurationElement 

    { 

        [ConfigurationProperty("key", IsRequired = true)] 

        public string Key 

        { 

            get 

            { 

                return (string)this["key"]; 

            } 

            set 

            { 

                this["key"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("value", IsRequired = true)] 

        public string Value 

        { 

            get 

            { 

                return (string)this["value"]; 

            } 

            set 

            { 

                this["value"] = value; 

            } 

        } 

    } 

}



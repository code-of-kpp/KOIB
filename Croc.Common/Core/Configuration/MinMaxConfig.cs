using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

 

 

namespace Croc.Core.Configuration 

{ 

    /// <summary> 

    /// Конфиг-элемент, который содержит атрибуты min и max типа Т 

    /// </summary> 

    public class MinMaxConfig<T> : ConfigurationElement 

    { 

        /// <summary> 

        /// Значение Min 

        /// </summary> 

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

 

 

        /// <summary> 

        /// Значение Max 

        /// </summary> 

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



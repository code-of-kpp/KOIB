using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Printing.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент шрифта 

    /// </summary> 

    public class FontConfig : ConfigurationElement 

    { 

        /// <summary> 

        /// Наименование шрифта 

        /// </summary> 

        [ConfigurationProperty("name", IsRequired = true)] 

        public string Name 

        { 

            get 

            { 

                return (string)this["name"]; 

            } 

            set 

            { 

                this["name"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Размер шрифта 

        /// </summary> 

        [ConfigurationProperty("size", IsRequired = true)] 

        public int Size 

        { 

            get 

            { 

                return (int)this["size"]; 

            } 

            set 

            { 

                this["size"] = value; 

            } 

        } 

    } 

}



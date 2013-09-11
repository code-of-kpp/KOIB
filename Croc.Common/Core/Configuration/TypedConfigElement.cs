using System.Configuration; 

 

 

namespace Croc.Core.Configuration 

{ 

    /// <summary> 

    /// Конфигурационный элемент, содержащий указание загружаемого типа 

    /// </summary> 

    public class TypedConfigElement : ConfigurationElement 

    { 

        /// <summary> 

        /// Наименование типа в виде "(полное имя типа), (неймспейс)" 

        /// </summary> 

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



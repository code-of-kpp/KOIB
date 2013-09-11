using System; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Keyboard.Config 

{ 

    public class KeyboardConfig : ConfigurationElement 

    { 

        [ConfigurationProperty("type", IsRequired = true)] 

        public string TypeStr 

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

 

 

        /// <summary> 

        /// Тип клавиатуры  

        /// </summary> 

        public KeyboardType Type 

        { 

            get 

            { 

                return (KeyboardType)Enum.Parse(typeof(KeyboardType), TypeStr, true); 

            } 

        } 

 

 

        [ConfigurationProperty("keys", IsDefaultCollection = false, IsRequired = false)] 

        [ConfigurationCollection(typeof(KeyConfigCollection), AddItemName = "key")] 

        public KeyConfigCollection Keys 

        { 

            get 

            { 

                return (KeyConfigCollection)base["keys"]; 

            } 

        } 

    } 

}



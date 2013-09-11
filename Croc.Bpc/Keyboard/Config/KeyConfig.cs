using System; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Keyboard.Config 

{ 

    public class KeyConfig : ConfigurationElement 

    { 

        [ConfigurationProperty("code", IsRequired = true)] 

        public int ScanCode 

        { 

            get 

            { 

                return (int)this["code"]; 

            } 

            set 

            { 

                this["code"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("type", IsRequired = true)] 

        public KeyType Type 

        { 

            get 

            { 

                return (KeyType)this["type"]; 

            } 

            set 

            { 

                this["type"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("value", IsRequired = false, DefaultValue = 0)] 

        public int Value 

        { 

            get 

            { 

                return (int)this["value"]; 

            } 

            set 

            { 

                this["value"] = value; 

            } 

        } 

    } 

}



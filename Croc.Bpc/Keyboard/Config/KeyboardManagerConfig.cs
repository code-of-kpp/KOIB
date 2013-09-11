using System; 

using Croc.Core.Configuration; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Keyboard.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент менеджера клавиатуры 

    /// </summary> 

    public class KeyboardManagerConfig : SubsystemConfig 

    { 

        [ConfigurationProperty("keyboard", IsRequired = true)] 

        public KeyboardConfig Keyboard 

        { 

            get 

            { 

                return (KeyboardConfig)this["keyboard"]; 

            } 

            set 

            { 

                this["keyboard"] = value; 

            } 

        } 

    } 

}



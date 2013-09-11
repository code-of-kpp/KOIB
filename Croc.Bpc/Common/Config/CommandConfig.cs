using System; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Common.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент команды 

    /// </summary> 

    public class CommandConfig : ConfigurationElement 

    { 

        /// <summary> 

        /// Имя команды 

        /// </summary> 

        [ConfigurationProperty("command", IsRequired = true)] 

        public string Command 

        { 

            get 

            { 

                return (string)this["command"]; 

            } 

            set 

            { 

                this["command"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Параметры команды 

        /// </summary> 

        [ConfigurationProperty("params", IsRequired = false)] 

        public string Params 

        { 

            get 

            { 

                return (string)this["params"]; 

            } 

            set 

            { 

                this["params"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Пауза в микросекундах после выполнения команды 

        /// </summary> 

        [ConfigurationProperty("sleep", IsRequired = false)] 

        public int SleepInterval 


        { 

            get 

            { 

                return (int)this["sleep"]; 

            } 

            set 

            { 

                this["sleep"] = value; 

            } 

        } 

    } 

}



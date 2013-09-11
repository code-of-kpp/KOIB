using System; 

using System.Configuration; 

using Croc.Bpc.Common.Config; 

 

 

namespace Croc.Bpc.Printing.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент списка команд 

    /// </summary> 

    public class CommandsConfig : ConfigurationElement 

    { 

        /// <summary> 

        /// Команда для выполнения перед печатью 

        /// </summary> 

        [ConfigurationProperty("beforePrinting", IsRequired = false)] 

        public CommandConfig BeforePrinting 

        { 

            get 

            { 

                return (CommandConfig)this["beforePrinting"]; 

            } 

            set 

            { 

                this["beforePrinting"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Команда для выполнения после печати 

        /// </summary> 

        [ConfigurationProperty("afterPrinting", IsRequired = false)] 

        public CommandConfig AfterPrinting 

        { 

            get 

            { 

                return (CommandConfig)this["afterPrinting"]; 

            } 

            set 

            { 

                this["afterPrinting"] = value; 

            } 

        } 

    } 

}



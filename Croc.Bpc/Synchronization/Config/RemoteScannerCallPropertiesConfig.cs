using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Synchronization.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент настроек вызова удаленного сканера 

    /// </summary> 

    public class RemoteScannerCallPropertiesConfig : ConfigurationElement 

    { 

        /// <summary> 

        /// Общие настройки для всех вызовов 

        /// </summary> 

        [ConfigurationProperty("common", IsRequired = true)] 

        public CallPropertiesConfig Common 

        { 

            get 

            { 

                return (CallPropertiesConfig)this["common"]; 

            } 

            set 

            { 

                this["common"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Настройки вызовов для синхронизации 

        /// </summary> 

        [ConfigurationProperty("synchronization", IsRequired = true)] 

        public CallPropertiesConfig Synchronization 

        { 

            get 

            { 

                return (CallPropertiesConfig)this["synchronization"]; 

            } 

            set 

            { 

                this["synchronization"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Настройки для выполнения удаленной печати 


        /// </summary> 

        [ConfigurationProperty("printing", IsRequired = true)] 

        public CallPropertiesConfig Printing 

        { 

            get 

            { 

                return (CallPropertiesConfig)this["printing"]; 

            } 

            set 

            { 

                this["printing"] = value; 

            } 

        } 

    } 

}



using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Synchronization.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент настроек вызова 

    /// </summary> 

    public class CallPropertiesConfig : ConfigurationElement 

    { 

        /// <summary> 

        /// Таймаут вызова (мсек) 

        /// </summary> 

        [ConfigurationProperty("timeout", IsRequired = true)] 

        public int Timeout 

        { 

            get 

            { 

                return (int)this["timeout"]; 

            } 

            set 

            { 

                this["timeout"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Максимальное кол-во попыток выполнить вызов 

        /// </summary> 

        [ConfigurationProperty("maxTryCount", IsRequired = true)] 

        public int MaxTryCount 

        { 

            get 

            { 

                return (int)this["maxTryCount"]; 

            } 

            set 

            { 

                this["maxTryCount"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Задержка между неудачной попыткой вызова и следующей попыткой (мсек) 


        /// </summary> 

        [ConfigurationProperty("retryDelay", IsRequired = true)] 

        public int RetryDelay 

        { 

            get 

            { 

                return (int)this["retryDelay"]; 

            } 

            set 

            { 

                this["retryDelay"] = value; 

            } 

        } 

    } 

}



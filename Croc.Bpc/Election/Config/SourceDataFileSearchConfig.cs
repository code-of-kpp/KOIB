using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Election.Config 

{ 

    /// <summary> 

    /// Параметры поиска файла с ИД 

    /// </summary> 

    public class SourceDataFileSearchConfig : ConfigurationElement 

    { 

        /// <summary> 

        /// Максимальное кол-во попыток найти файл 

        /// </summary> 

        [ConfigurationProperty("maxTryCount", IsRequired = true)] 

        public uint MaxTryCount 

        { 

            get 

            { 

                return (uint)this["maxTryCount"]; 

            } 

            set 

            { 

                this["maxTryCount"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Задержка между попытками (формат: Ч:М:С) 

        /// </summary> 

        [ConfigurationProperty("delay", IsRequired = true)] 

        public TimeSpan Delay 

        { 

            get 

            { 

                return (TimeSpan)this["delay"]; 

            } 

            set 

            { 

                this["delay"] = value; 

            } 

        } 

    } 

}



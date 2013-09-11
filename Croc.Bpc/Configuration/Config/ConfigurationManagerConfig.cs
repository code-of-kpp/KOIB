using System; 

using Croc.Core.Configuration; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Configuration.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент менеджера конфигурации 

    /// </summary> 

    public class ConfigurationManagerConfig : SubsystemConfig 

    { 

        /// <summary> 

        /// Путь к рабочему конфиг-файлу 

        /// </summary> 

        [ConfigurationProperty("workingConfigFile", IsRequired = true)] 

        public ValueConfig<string> WorkingConfigFilePath 

        { 

            get 

            { 

                return (ValueConfig<string>)this["workingConfigFile"]; 

            } 

            set 

            { 

                this["workingConfigFile"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Пути к файлам, которые являются частными конфиг-файлами 

        /// </summary> 

        [ConfigurationProperty("partialConfigFile", IsDefaultCollection = false, IsRequired = false)] 

        [ConfigurationCollection(typeof(ValueConfig<string>), AddItemName = "path")] 

        public PartialConfigPathsCollection PartialConfigFileLocations 

        { 

            get 

            { 

				return (PartialConfigPathsCollection)base["partialConfigFile"]; 

            } 

        } 

 

 

        /// <summary> 

        /// XPath-ы к элементам конфига, которые являются личными настройками данного приложения 

        /// </summary> 

        [ConfigurationProperty("privateConfigElements", IsDefaultCollection = false, IsRequired = false)] 

        [ConfigurationCollection(typeof(ValueConfig<string>), AddItemName = "xpath")] 

        public ValueConfigCollection<string> PrivateConfigElementXPaths 

        { 


            get 

            { 

                return (ValueConfigCollection<string>)base["privateConfigElements"]; 

            } 

        } 

    } 

}



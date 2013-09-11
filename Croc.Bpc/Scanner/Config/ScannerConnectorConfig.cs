using System; 

using System.Configuration; 

using Croc.Core.Configuration; 

 

 

namespace Croc.Bpc.Scanner.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент, содержащий параметры коннектора сканера 

    /// </summary> 

    public class ScannerConnectorConfig : ConfigurationElement 

    { 

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

 

 

        [ConfigurationProperty("broadcast", IsRequired = true)] 

        public string BroadcastIPAddress 

        { 

            get 

            { 

                return (string)this["broadcast"]; 

            } 

            set 

            { 

                this["broadcast"] = value; 

            } 

        } 

 

 

        [ConfigurationProperty("settings", IsDefaultCollection = false, IsRequired = false)] 

        [ConfigurationCollection(typeof(SettingConfigCollection), AddItemName = "add")] 

        public SettingConfigCollection Settings 

        { 

            get 

            { 

                return (SettingConfigCollection)base["settings"]; 

            } 

        } 

    } 


}



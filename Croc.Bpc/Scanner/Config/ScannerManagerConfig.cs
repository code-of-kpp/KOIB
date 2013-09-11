using System; 

using Croc.Core.Configuration; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Scanner.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент менеджера сканера 

    /// </summary> 

    public class ScannerManagerConfig : SubsystemConfig 

    { 

        /// <summary> 

        /// Параметры коннектора сканера 

        /// </summary> 

        [ConfigurationProperty("scannerConnector", IsRequired = true)] 

        public ScannerConnectorConfig ScannerConnector 

        { 

            get 

            { 

                return (ScannerConnectorConfig)this["scannerConnector"]; 

            } 

            set 

            { 

                this["scannerConnector"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Версия драйвера сканера 

        /// </summary> 

        [ConfigurationProperty("driverVersion", IsRequired = true)] 

        public ValueConfig<int> DriverVersion 

        { 

            get 

            { 

                return (ValueConfig<int>)this["driverVersion"]; 

            } 

            set 

            { 

                this["driverVersion"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Нужно ли выполнять диагностику оборудования (лампы, индикатор, моторы) при инициализации 

        /// </summary> 

        [ConfigurationProperty("checkHardware", IsRequired = true)] 


        public EnabledConfig CheckHardware 

        { 

            get 

            { 

                return (EnabledConfig)this["checkHardware"]; 

            } 

            set 

            { 

                this["checkHardware"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Параметры сканеров для разных версий 

        /// </summary> 

        [ConfigurationProperty("scannerParameters", IsDefaultCollection = false, IsRequired = true)] 

        [ConfigurationCollection(typeof(ScannerParametersConfig), AddItemName = "version")] 

        public ScannerParametersConfigCollection ScannerParametersVersions 

        { 

            get 

            { 

                return (ScannerParametersConfigCollection)base["scannerParameters"]; 

            } 

        } 

 

 

        /// <summary> 

        /// Предупреждения 

        /// </summary> 

        [ConfigurationProperty("alerts", IsRequired = true)] 

        public AlertsConfig Alerts 

        { 

            get 

            { 

                return (AlertsConfig)this["alerts"]; 

            } 

            set 

            { 

                this["alerts"] = value; 

            } 

        } 

    } 

}



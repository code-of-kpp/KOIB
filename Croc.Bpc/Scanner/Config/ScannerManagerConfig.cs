using System; 
using Croc.Core.Configuration; 
using System.Configuration; 
namespace Croc.Bpc.Scanner.Config 
{ 
    public class ScannerManagerConfig : SubsystemConfig 
    { 
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
        [ConfigurationProperty("scannerParameters", IsDefaultCollection = false, IsRequired = true)] 
        [ConfigurationCollection(typeof(ScannerParametersConfig), AddItemName = "version")] 
        public ScannerParametersConfigCollection ScannerParametersVersions 
        { 
            get 
            { 
                return (ScannerParametersConfigCollection)base["scannerParameters"]; 
            } 
        } 
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

using System; 
using System.Configuration; 
using Croc.Core.Configuration; 
namespace Croc.Bpc.Scanner.Config 
{ 
    public class ScannerParametersConfig : ConfigurationElement 
    { 
        [ConfigurationProperty("name", IsRequired = true)] 
        public string VersionName 
        { 
            get 
            { 
                return (string)this["name"]; 
            } 
            set 
            { 
                this["name"] = value; 
            } 
        } 
        [ConfigurationProperty("workZone", IsRequired = true)] 
        public WorkZoneConfig WorkZone 
        { 
            get 
            { 
                return (WorkZoneConfig)this["workZone"]; 
            } 
            set 
            { 
                this["workZone"] = value; 
            } 
        } 
        [ConfigurationProperty("checkFormat", IsRequired = true)] 
        public EnabledConfig CheckFormat 
        { 
            get 
            { 
                return (EnabledConfig)this["checkFormat"]; 
            } 
            set 
            { 
                this["checkFormat"] = value; 
            } 
        } 
        [ConfigurationProperty("tuning", IsRequired = true)] 
        public EnabledConfig Tuning 
        { 
            get 
            { 
                return (EnabledConfig)this["tuning"]; 
            } 
            set 
            { 
                this["tuning"] = value; 
            } 
        } 
        [ConfigurationProperty("blankPaperTypes", IsDefaultCollection = false, IsRequired = true)] 
        [ConfigurationCollection(typeof(BlankPaperTypeConfig), AddItemName = "blankPaperType")] 
        public BlankPaperTypeConfigCollection BlankPaperTypes 
        { 
            get 
            { 
                return (BlankPaperTypeConfigCollection)base["blankPaperTypes"]; 
            } 
        } 
        [ConfigurationProperty("doubleSheetSensor", IsRequired = true)] 
        public DoubleSheetSensorConfig DoubleSheetSensor 
        { 
            get 
            { 
                return (DoubleSheetSensorConfig)this["doubleSheetSensor"]; 
            } 
            set 
            { 
                this["doubleSheetSensor"] = value; 
            } 
        } 
    } 
}

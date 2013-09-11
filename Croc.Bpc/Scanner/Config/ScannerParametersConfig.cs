using System; 

using System.Configuration; 

using Croc.Core.Configuration; 

 

 

namespace Croc.Bpc.Scanner.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент, содержащий параметры сканера 

    /// </summary> 

    public class ScannerParametersConfig : ConfigurationElement 

    { 

        /// <summary> 

        /// Название версии сканера 

        /// </summary> 

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

 

 

        [ConfigurationProperty("paperDensity", IsRequired = true)] 

        public PaperDensityConfig PaperDensity 

        { 

            get 

            { 

                return (PaperDensityConfig)this["paperDensity"]; 

            } 

            set 

            { 

                this["paperDensity"] = value; 

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

 

 

        [ConfigurationProperty("dirtDetection", IsRequired = true)] 

        public EnabledConfig DirtDetection 

        { 

            get 

            { 

                return (EnabledConfig)this["dirtDetection"]; 

            } 

            set 

            { 

                this["dirtDetection"] = value; 

            } 

        } 

    } 

}



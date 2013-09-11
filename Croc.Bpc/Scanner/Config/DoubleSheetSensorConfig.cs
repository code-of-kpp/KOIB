using System; 
using Croc.Core.Configuration; 
using System.Configuration; 
namespace Croc.Bpc.Scanner.Config 
{ 
    public class DoubleSheetSensorConfig : EnabledConfig 
    { 
        [ConfigurationProperty("levelLeft", IsRequired = true)] 
        public short LevelLeft 
        { 
            get 
            { 
                return (short)this["levelLeft"]; 
            } 
            set 
            { 
                this["levelLeft"] = value; 
            } 
        } 
        [ConfigurationProperty("levelRigth", IsRequired = true)] 
        public short LevelRigth 
        { 
            get 
            { 
                return (short)this["levelRigth"]; 
            } 
            set 
            { 
                this["levelRigth"] = value; 
            } 
        } 
        [ConfigurationProperty("thick", IsRequired = true)] 
        public short Thick 
        { 
            get 
            { 
                return (short)this["thick"]; 
            } 
            set 
            { 
                this["thick"] = value; 
            } 
        } 
        [ConfigurationProperty("thin", IsRequired = true)] 
        public short Thin 
        { 
            get 
            { 
                return (short)this["thin"]; 
            } 
            set 
            { 
                this["thin"] = value; 
            } 
        } 
    } 
}

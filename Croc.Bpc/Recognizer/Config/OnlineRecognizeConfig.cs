using System.Configuration; 
using Croc.Core.Configuration; 
using Croc.Bpc.Recognizer.Ocr; 
namespace Croc.Bpc.Recognizer.Config 
{ 
    public class OnlineRecognizeConfig : EnabledConfig 
    { 
        [ConfigurationProperty("level", IsRequired = true)] 
        public OnlineLevel Level 
        { 
            get 
            { 
                return (OnlineLevel)this["level"]; 
            } 
            set 
            { 
                this["level"] = value; 
            } 
        } 
        [ConfigurationProperty("runZone", IsRequired = true)] 
        public LineZoneConfig RunZone 
        { 
            get 
            { 
                return (LineZoneConfig)this["runZone"]; 
            } 
            set 
            { 
                this["runZone"] = value; 
            } 
        } 
        [ConfigurationProperty("blankTestZone", IsRequired = true)] 
        public LineZoneConfig BlankTestZone 
        { 
            get 
            { 
                return (LineZoneConfig)this["blankTestZone"]; 
            } 
            set 
            { 
                this["blankTestZone"] = value; 
            } 
        } 
    } 
}

using System.Configuration; 
namespace Croc.Bpc.Recognizer.Config 
{ 
    public class LineZoneConfig : ConfigurationElement 
    { 
        public const int INFINITY = -1; 
        [ConfigurationProperty("startAtLine", IsRequired = true)] 
        public int StartAtLine 
        { 
            get 
            { 
                return (int)this["startAtLine"]; 
            } 
            set 
            { 
                this["startAtLine"] = value; 
            } 
        } 
        [ConfigurationProperty("stopAtLine", IsRequired = true)] 
        public int StopAtLine 
        { 
            get 
            { 
                return (int)this["stopAtLine"]; 
            } 
            set 
            { 
                this["stopAtLine"] = value; 
            } 
        } 
    } 
}

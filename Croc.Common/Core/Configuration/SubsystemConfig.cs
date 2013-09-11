using System.Configuration; 
using System.Xml; 
namespace Croc.Core.Configuration 
{ 
    public class SubsystemConfig : ConfigurationElement 
    { 
        protected override bool SerializeToXmlElement(XmlWriter writer, string elementName) 
        { 
            this["name"] = SubsystemName; 
            this["type"] = SubsystemTypeName; 
            this["logFileFolder"] = LogFileFolder; 
            this["traceLevel"] = TraceLevelName; 
            this["separateLog"] = SeparateLog; 
            this["disposeOrder"] = DisposeOrder; 
            return base.SerializeToXmlElement(writer, elementName); 
        } 
        [ConfigurationProperty("name", IsRequired = false, IsKey = true)] 
        public string SubsystemName 
        { 
            get; 
            set; 
        } 
        [ConfigurationProperty("type", IsRequired = false)] 
        public string SubsystemTypeName 
        { 
            get; 
            set; 
        } 
        [ConfigurationProperty("logFileFolder", IsRequired = false)] 
        public string LogFileFolder 
        { 
            get; 
            set; 
        } 
        [ConfigurationProperty("traceLevel", IsRequired = false)] 
        public string TraceLevelName 
        { 
            get; 
            set; 
        } 
        [ConfigurationProperty("separateLog", IsRequired = false, DefaultValue = false)] 
        public bool SeparateLog 
        { 
            get; 
            set; 
        } 
        public const int DISPOSE_ORDER_UNDEFINED = 1000; 
        [ConfigurationProperty("disposeOrder", IsRequired = false, DefaultValue = DISPOSE_ORDER_UNDEFINED)] 
        public int DisposeOrder 
        { 
            get; 
            set; 
        } 
    } 
}

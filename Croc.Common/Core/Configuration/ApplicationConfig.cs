using System; 
using System.Configuration; 
using System.IO; 
using System.Xml; 
using Croc.Core.Utils.Xml; 
namespace Croc.Core.Configuration 
{ 
    public class ApplicationConfig : ConfigurationSection 
    { 
        public const string SECTION_NAME = "croc.application"; 
        public static ApplicationConfig FromXml(string xml) 
        { 
            try 
            { 
                var config = new ApplicationConfig(); 
                using (var stream = new StringReader(xml)) 
                using (var reader = XmlReader.Create(stream)) 
                    config.DeserializeSection(reader); 
                return config; 
            } 
            catch (Exception ex) 
            { 
                throw new Exception("Ошибка загрузки конфиг-секции из xml: " + xml, ex); 
            } 
        } 
        public string ToXml() 
        { 
            using (var memStream = new MemoryStream()) 
            { 
                using (var xmlWriter = new PrettyPrintXmlWriter(memStream)) 
                { 
                    SerializeToXmlElement(xmlWriter, SECTION_NAME); 
                    return xmlWriter.ToFormatString(); 
                } 
            } 
        } 
        [ConfigurationProperty("name", IsRequired = false)] 
        public String Name 
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
        [ConfigurationProperty("logFileFolder", IsRequired = false)] 
        public string LogFileFolder 
        { 
            get 
            { 
                return (string)this["logFileFolder"]; 
            } 
            set 
            { 
                this["logFileFolder"] = value; 
            } 
        } 
        [ConfigurationProperty("traceLevel", IsRequired = false)] 
        public string TraceLevelName 
        { 
            get 
            { 
                return (string)this["traceLevel"]; 
            } 
            set 
            { 
                this["traceLevel"] = value; 
            } 
        } 
        [ConfigurationProperty("diagnostics", IsRequired = false)] 
        public DiagnosticsConfig DiagnosticsConfig 
        { 
            get 
            { 
                return (DiagnosticsConfig)this["diagnostics"]; 
            } 
        } 
        [ConfigurationProperty("subsystems", IsDefaultCollection = true)] 
        [ConfigurationCollection(typeof(SubsystemConfigCollection), AddItemName = "subsystem")] 
        public SubsystemConfigCollection Subsystems 
        { 
            get 
            { 
                return (SubsystemConfigCollection)base["subsystems"]; 
            } 
        } 
    } 
}

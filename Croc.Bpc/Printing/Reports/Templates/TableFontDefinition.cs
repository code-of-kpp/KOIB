using System.Xml.Serialization; 
namespace Croc.Bpc.Printing.Reports.Templates 
{ 
    public abstract class TableFontDefinition : FontDefinition 
    { 
        [XmlAttribute("dotted")] 
        public string IsDotted; 
    } 
}

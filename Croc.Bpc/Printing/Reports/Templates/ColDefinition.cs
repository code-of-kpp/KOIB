using System.Xml.Serialization; 
namespace Croc.Bpc.Printing.Reports.Templates 
{ 
    public class ColDefinition : FontDefinition 
    { 
        [XmlAttribute("width")] 
        public int Width; 
        [XmlAttribute("count")] 
        public string Count; 
        public override Lines ConstructContent(ReportTemplateParser parser) 
        { 
            return new Lines(); 
        } 
    } 
}

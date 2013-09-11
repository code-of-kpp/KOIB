using System; 
using System.Collections.Generic; 
using System.Text; 
using System.Xml.Serialization; 
namespace Croc.Bpc.Printing.Reports.Templates 
{ 
    [Serializable, XmlType("Part")] 
    public class LinePart 
    { 
        [XmlAttribute("condition")] 
        public string Condition; 
        [XmlAttribute("not")] 
        public bool Not; 
        [XmlAttribute("each")] 
        public string Each; 
        [XmlAttribute("in")] 
        public string In; 
        [XmlArray("Parts")] 
        [XmlArrayItem("Part", typeof(LinePart))] 
        public LinePart[] Part; 
        [XmlText] 
        public string Text; 
        public string GetLine(ReportTemplateParser parser) 
        { 
            return ConstructLine(parser, Part, Text ?? String.Empty); 
        } 
        public static string ConstructLine(ReportTemplateParser parser, LinePart[] parts, string defaultText) 
        { 
            if (parts != null && parts.Length > 0) 
            { 
                defaultText = ""; 
                foreach (LinePart part in parts) 
                { 
                    if (parser.Check(part.Condition, part.Not)) 
                    { 
                        defaultText += part.GetLine(parser); 
                    } 
                } 
            } 
            return defaultText; 
        } 
    } 
}

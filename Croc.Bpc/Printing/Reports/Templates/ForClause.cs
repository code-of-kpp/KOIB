using System; 
using System.Xml.Serialization; 
namespace Croc.Bpc.Printing.Reports.Templates 
{ 
    [Serializable, XmlType("For")] 
    public class ForClause : BasePlainElement 
    { 
        [XmlAttribute("each")] 
        public string Each; 
        [XmlAttribute("in")] 
        public string In; 
        [XmlArrayItem("Line", typeof(LineClause))] 
        [XmlArrayItem("For", typeof(ForClause))] 
        [XmlArrayItem("If", typeof(IfClause))] 
        [XmlArrayItem("SetCurrentRow", typeof(SetCurrentRow))] 
        public BasePlainElement[] Body; 
        public override Lines ConstructContent(ReportTemplateParser parser) 
        { 
            var lines = new Lines(); 
            try 
            { 
                if (Body != null) 
                { 
                    parser.RunFor(Each, In, 
                                  delegate 
                                      { 
                                          foreach (BasePlainElement element in Body) 
                                          { 
                                              lines.AddRange(element.ConstructContent(parser)); 
                                          } 
                                      }); 
                } 
            } 
            catch (ReportTemplateParserException pex) 
            { 
                lines.Add(new ReportLine( 
                    new[] {"-+- FOR: " + pex.Reason + " (" + pex.Type + "." + pex.Name + ") -!-" },  
                    LineAlign.Left, "0", false, false, false, false, -1)); 
            } 
            return lines; 
        } 
    } 
}

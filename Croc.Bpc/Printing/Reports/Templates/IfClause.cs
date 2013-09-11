using System.Xml.Serialization; 
namespace Croc.Bpc.Printing.Reports.Templates 
{ 
    public class IfClause : BasePlainElement 
    { 
        [XmlAttribute("condition")] 
        public string Condition; 
        [XmlAttribute("not")] 
        public bool Not; 
        [XmlArrayItem("Line", typeof(LineClause))] 
        [XmlArrayItem("For", typeof(ForClause))] 
        [XmlArrayItem("If", typeof(IfClause))] 
        [XmlArrayItem("SetCurrentRow", typeof(SetCurrentRow))] 
        public BasePlainElement[] Then; 
        [XmlArrayItem("Line", typeof(LineClause))] 
        [XmlArrayItem("For", typeof(ForClause))] 
        [XmlArrayItem("If", typeof(IfClause))] 
        [XmlArrayItem("SetCurrentRow", typeof(SetCurrentRow))] 
        public BasePlainElement[] Else; 
        public override Lines ConstructContent(ReportTemplateParser parser) 
        { 
            var lines = new Lines(); 
            try 
            { 
                BasePlainElement[] branch = parser.Check(Condition, Not) ? Then : Else; 
                if (branch != null) 
                { 
                    foreach (BasePlainElement element in branch) 
                    { 
                        lines.AddRange(element.ConstructContent(parser)); 
                    } 
                } 
            } 
            catch (ReportTemplateParserException pex) 
            { 
                lines.Add(new ReportLine( 
                    new[]{"-+- IF: " + pex.Reason + " (" + pex.Type + "." + pex.Name + ") -!-"},  
                    LineAlign.Left, "0", false, false, false, false, -1)); 
            } 
            return lines; 
        } 
    } 
}

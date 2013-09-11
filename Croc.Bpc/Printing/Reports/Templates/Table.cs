using System; 
using System.Xml.Serialization; 
namespace Croc.Bpc.Printing.Reports.Templates 
{ 
    [Serializable, XmlType("Table")] 
    public class Table : BaseTableHolder 
    { 
        [XmlArray("Header")] 
        [XmlArrayItem("Col")] 
        public ColDefinition[] Columns; 
        [XmlArrayItem("Line", typeof(LineClause))] 
        [XmlArrayItem("For", typeof(ForClause))] 
        [XmlArrayItem("If", typeof(IfClause))] 
        [XmlArrayItem("SetCurrentRow", typeof(SetCurrentRow))] 
        public BasePlainElement[] Body; 
        [XmlAttribute("dotted")] 
        public bool IsDotted; 
        [XmlIgnore] 
        public Lines Lines 
        { 
            get; set; 
        } 
        public override Table[] GetTables(ReportTemplateParser parser) 
        { 
            Lines = new Lines(); 
            Lines.AddRange(ReportTemplate.ConstructHeader(Body)); 
            return new[] {this}; 
        } 
    } 
}

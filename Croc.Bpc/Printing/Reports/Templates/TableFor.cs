using System; 
using System.Collections.Generic; 
using System.Xml.Serialization; 
namespace Croc.Bpc.Printing.Reports.Templates 
{ 
    [Serializable, XmlType("TableFor")] 
    public class TableFor : BaseTableHolder 
    { 
        [XmlAttribute("each")] 
        public string Each; 
        [XmlAttribute("in")] 
        public string In; 
        [XmlArrayItem("Table", typeof(Table))] 
        public Table[] Body; 
        public override Table[] GetTables(ReportTemplateParser parser) 
        { 
            var resultTables = new List<Table>(); 
            try 
            { 
                if (Body != null) 
                { 
                    parser.RunFor(Each, In, 
                                  delegate 
                                  { 
                                      foreach (Table tableTemplate in Body) 
                                      { 
                                          var table = new Table 
                                                          { 
                                                              Columns = tableTemplate.Columns, 
                                                              IsDotted = tableTemplate.IsDotted, 
                                                              Lines = new Lines() 
                                                          }; 
                                        table.Lines.AddRange(ReportTemplate.ConstructHeader(tableTemplate.Body)); 
                                          resultTables.Add(table); 
                                      } 
                                  }); 
                } 
            } 
            catch (ReportTemplateParserException pex) 
            { 
                throw new ApplicationException("Ошибка построения тела таблицы", pex); 
            } 
            return resultTables.ToArray(); 
        } 
    } 
}

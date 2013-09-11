using System; 
using System.Xml.Serialization; 
namespace Croc.Bpc.Printing.Reports.Templates 
{ 
    [Serializable] 
    public class SetCurrentRow : BasePlainElement 
    { 
        [XmlText] 
        public string Value; 
        public override Lines ConstructContent(ReportTemplateParser parser) 
        { 
            Lines lines = new Lines(); 
            int rowNumber = 0; 
            if (!Int32.TryParse(Value, out rowNumber)) 
            { 
                if (!Int32.TryParse(parser.Format(Value), out rowNumber)) 
                { 
                    rowNumber = 0; 
                } 
            } 
            lines.Add(new ServiceLine(rowNumber)); 
            return lines; 
        } 
    } 
}

using System; 
using System.Collections; 
using System.Xml.Serialization; 
namespace Croc.Bpc.Printing.Reports.Templates 
{ 
    [Serializable, XmlType("Line")] 
    public class LineClause : FontDefinition 
    { 
        [XmlAttribute("condition")] 
        public string Condition; 
        [XmlAttribute("not")] 
        public bool Not; 
        [XmlAttribute("split")] 
        public string SplitChar; 
        [XmlAttribute("trim")] 
        public bool NeedTrim; 
        [XmlAttribute("newPage")] 
        public bool NewPage; 
        [XmlAttribute("resetPageNumber")] 
        public bool ResetPageNumber; 
        [XmlAttribute("isLineDotted")] 
        public int IsLineDotted = -1; 
        [XmlArray("Parts")] 
        [XmlArrayItem("Part", typeof(LinePart))] 
        public LinePart[] Part; 
        [XmlArray("Cols")] 
        [XmlArrayItem("Col", typeof(LinePart))] 
        public LinePart[] Columns; 
        [XmlText]  
        public string Text; 
        public LineClause() {} 
        public LineClause(string text, LineAlign align, int fontSize, bool isBold, bool isItalic) 
        { 
            Text = text; 
            Align = align; 
            RelativeFontSize = fontSize.ToString(); 
            IsBold = isBold; 
            IsItalic = isItalic; 
        } 
        public LineClause(string[] columns, int fontSize, bool isBold, bool isItalic, bool newPage) 
        { 
            Columns = new LinePart[columns.Length]; 
            for (int i = 0; i < columns.Length; i++) 
            { 
                Columns[i] = new LinePart(); 
                Columns[i].Text = columns[i]; 
            } 
            RelativeFontSize = fontSize.ToString(); 
            IsBold = isBold; 
            IsItalic = isItalic; 
            NewPage = newPage; 
        } 
        public override Lines ConstructContent(ReportTemplateParser parser) 
        { 
            var lines = new Lines(); 
            if (parser.Check(Condition, Not)) 
            { 
                if (Columns == null || Columns.Length <= 0) 
                { 
                    var text = new[]  
                    { 
                        parser.Format(LinePart.ConstructLine(parser, Part, Text ?? String.Empty)) 
                    }; 
                    if (!string.IsNullOrEmpty(SplitChar)) 
                    { 
                        text = text[0].Split(SplitChar.ToCharArray()); 
                    } 
                    foreach (string s in text) 
                    { 
                        lines.Add( 
                            new ReportLine(new[] {NeedTrim ? s.Trim() : s}, 
                                           Align ?? LineAlign.Left, 
                                           FontSize, IsBold, IsItalic, false, false, IsLineDotted)); 
                    } 
                } 
                else 
                { 
                    var columns = new ArrayList(); 
                    foreach (LinePart t in Columns) 
                    { 
                        if (parser.Check(t.Condition, t.Not)) 
                        { 
                            if (t.In != null && t.Each != null) 
                            { 
                                parser.RunFor(t.Each, t.In, 
                                              () => columns.Add(parser.Format( 
                                                  LinePart.ConstructLine 
                                                      (parser, t.Part, t.Text ?? String.Empty)))); 
                            } 
                            else 
                            { 
                                columns.Add(parser.Format( 
                                    LinePart.ConstructLine(parser, t.Part, t.Text ?? String.Empty))); 
                            } 
                        } 
                    } 
                    var arr = new string[columns.Count]; 
                    columns.CopyTo(arr); 
                    lines.Add(new ReportLine(arr, 
                                             Align ?? LineAlign.Left, 
                                             FontSize, IsBold, IsItalic, NewPage, ResetPageNumber, IsLineDotted)); 
                } 
            } 
            return lines; 
        } 
    } 
}

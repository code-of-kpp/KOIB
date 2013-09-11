using System; 

using System.Collections; 

using System.Collections.Generic; 

using System.Runtime.Serialization; 

using System.Text; 

using System.Xml.Serialization; 

using Croc.Bpc.Election.Voting; 

 

 

namespace Croc.Bpc.Printing.Reports.Templates 

{ 

    [Serializable, XmlType("Line")] 

    public class LineClause : FontDefinition 

    { 

        /// <summary> 

        /// ??????? ?????? ?????? 

        /// </summary> 

        [XmlAttribute("condition")] 

        public string Condition; 

 

 

        /// <summary> 

        /// ???????? ??????? 

        /// </summary> 

        [XmlAttribute("not")] 

        public bool Not; 

 

 

        /// <summary> 

        /// ???????-??????????? ??? ????????? ?????? ?? ????? 

        /// </summary> 

        [XmlAttribute("split")] 

        public string SplitChar; 

 

 

        /// <summary> 

        /// ??????? ???????? ???????? ? ????? ?????? ?????? 

        /// </summary> 

        [XmlAttribute("trim")] 

        public bool NeedTrim; 

 

 

        /// <summary> 

        /// ??? ????????? ?????? ??????? ?????? ????? ???????? 

        /// </summary> 

        [XmlAttribute("newPage")] 

        public bool NewPage; 

 

 

        /// <summary> 


        /// ????? ?????? 

        /// </summary> 

        [XmlArray("Parts")] 

        [XmlArrayItem("Part", typeof(LinePart))] 

        public LinePart[] Part; 

 

 

        /// <summary> 

        /// ??????? 

        /// </summary> 

        [XmlArray("Cols")] 

        [XmlArrayItem("Col", typeof(LinePart))] 

        public LinePart[] Columns; 

 

 

        /// <summary> 

        /// ????? ?????? 

        /// </summary> 

        [XmlText]  

        public string Text; 

 

 

        /// <summary> 

        /// ??????????? ?? ????????? 

        /// </summary> 

        public LineClause() {} 

 

 

        /// <summary> 

        /// ??????????? ??? ????? ?????????? 

        /// </summary> 

        /// <param name="text">????? ??????</param> 

        /// <param name="align">????????????</param> 

        /// <param name="fontSize">?????? ??????</param> 

        /// <param name="isBold">??????</param> 

        /// <param name="isItalic">??????</param> 

        public LineClause(string text, LineAlign align, int fontSize, bool isBold, bool isItalic) 

        { 

            Text = text; 

            Align = align; 

            m_fontSize = fontSize.ToString(); 

            IsBold = isBold; 

            IsItalic = isItalic; 

        } 

 

 

        /// <summary> 

        /// ??????????? ??? ????????? ?????  

        /// </summary> 

        /// <param name="columns">????? ??????</param> 


        /// <param name="fontSize">?????? ??????</param> 

        /// <param name="isBold">??????</param> 

        /// <param name="isItalic">??????</param> 

        /// <param name="newPage">??????? ???????? ?? ???? ????????</param> 

        public LineClause(string[] columns, int fontSize, bool isBold, bool isItalic, bool newPage) 

        { 

            Columns = new LinePart[columns.Length]; 

            for (int i = 0; i < columns.Length; i++) 

            { 

                Columns[i] = new LinePart(); 

                Columns[i].Text = columns[i]; 

            } 

            m_fontSize = fontSize.ToString(); 

            IsBold = isBold; 

            IsItalic = isItalic; 

            NewPage = newPage; 

        } 

 

 

        public override Lines ConstructContent(ReportTemplateParser parser) 

        { 

            Lines lines = new Lines(); 

 

 

            if (parser.Check(Condition, Not)) 

            { 

                if (Columns == null || Columns.Length <= 0) 

                { 

                    // ??? ??????? ?????? 

                    string[] text = new string[] {parser.Format(LinePart.ConstructLine(parser, Part, Text ?? String.Empty))}; 

 

 

                    if (!string.IsNullOrEmpty(SplitChar)) 

                    { 

                        text = text[0].Split(SplitChar.ToCharArray()); 

                    } 

 

 

                    foreach (string s in text) 

                    { 

                        // NOTE: ??????? ?? ????? ?????? ?? ?????????????? ??? ?????????? 

                        lines.Add(new ReportLine(new[] {NeedTrim ? s.Trim() : s}, Align, FontSize, IsBold, IsItalic, false)); 

                    } 

                } 

                else 

                { 

                    // ????????? ?????? 

                    ArrayList columns = new ArrayList(); 

 

 


                    for (int i = 0; i < Columns.Length; i++) 

                    { 

                        // ???????? ??????? ?? ??????? 

                        if (parser.Check(Columns[i].Condition, Columns[i].Not)) 

                        { 

                            if (Columns[i].In != null && Columns[i].Each != null) 

                            { 

                                // ?????????? ???? 

                                parser.RunFor(Columns[i].Each, Columns[i].In, 

                                    delegate() 

                                    { 

                                        // ???????????? ??????? 

                                        columns.Add(parser.Format(LinePart.ConstructLine(parser, Columns[i].Part, Columns[i].Text ?? String.Empty))); 

                                    }); 

                            } 

                            else 

                            { 

                                // ???????????? ??????? 

                                columns.Add(parser.Format(LinePart.ConstructLine(parser, Columns[i].Part, Columns[i].Text ?? String.Empty))); 

                            } 

                        } 

                    } 

 

 

                    string[] arr = new string[columns.Count]; 

                    columns.CopyTo(arr); 

                    lines.Add(new ReportLine(arr, Align, FontSize, IsBold, IsItalic, NewPage)); 

                } 

            } 

 

 

            return lines; 

        } 

    } 

}



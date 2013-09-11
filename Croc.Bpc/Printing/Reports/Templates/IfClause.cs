using System; 

using System.Collections.Generic; 

using System.Text; 

using System.Xml.Serialization; 

using Croc.Bpc.Election.Voting; 

 

 

namespace Croc.Bpc.Printing.Reports.Templates 

{ 

    /// <summary> 

    /// ??????? (???????????? ??????????) 

    /// </summary> 

    public class IfClause : BasePlainElement 

    { 

        /// <summary> 

        /// ???????  

        /// </summary> 

        [XmlAttribute("condition")] 

        public string Condition; 

 

 

        /// <summary> 

        /// ???????? ??????? 

        /// </summary> 

        [XmlAttribute("not")] 

        public bool Not; 

 

 

        /// <summary> 

        /// ????? ???? ??????? ??????????? 

        /// </summary> 

        [XmlArrayItem("Line", typeof(LineClause))] 

        [XmlArrayItem("For", typeof(ForClause))] 

        [XmlArrayItem("If", typeof(IfClause))] 

        [XmlArrayItem("SetCurrentRow", typeof(SetCurrentRow))] 

        public BasePlainElement[] Then; 

 

 

        /// <summary> 

        /// ????? "?????" 

        /// </summary> 

        [XmlArrayItem("Line", typeof(LineClause))] 

        [XmlArrayItem("For", typeof(ForClause))] 

        [XmlArrayItem("If", typeof(IfClause))] 

        [XmlArrayItem("SetCurrentRow", typeof(SetCurrentRow))] 

        public BasePlainElement[] Else; 

 

 

        public override Lines ConstructContent(ReportTemplateParser parser) 

        { 


            Lines lines = new Lines(); 

 

 

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

                    LineAlign.Left, 0, false, false, false)); 

            } 

 

 

            return lines; 

        } 

    } 

}



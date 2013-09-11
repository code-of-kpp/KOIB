using System; 

using System.Collections; 

using System.Collections.Generic; 

using System.Text; 

using System.Xml.Serialization; 

using Croc.Bpc.Election.Voting; 

 

 

namespace Croc.Bpc.Printing.Reports.Templates 

{ 

    [Serializable, XmlType("For")] 

    public class ForClause : BasePlainElement 

    { 

        /// <summary> 

        /// ???????? 

        /// </summary> 

        [XmlAttribute("each")] 

        public string Each; 

        /// <summary> 

        /// ????????? ?? ??????? ??????????? ???? 

        /// </summary> 

        [XmlAttribute("in")] 

        public string In; 

 

 

        [XmlArrayItem("Line", typeof(LineClause))] 

        [XmlArrayItem("For", typeof(ForClause))] 

        [XmlArrayItem("If", typeof(IfClause))] 

        [XmlArrayItem("SetCurrentRow", typeof(SetCurrentRow))] 

        public BasePlainElement[] Body; 

 

 

        /// <summary> 

        ///  

        /// </summary> 

        /// <returns></returns> 

        public override Lines ConstructContent(ReportTemplateParser parser) 

        { 

            Lines lines = new Lines(); 

 

 

            try 

            { 

                if (Body != null) 

                { 

                    parser.RunFor(Each, In,  

                        delegate() { 

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

                    LineAlign.Left, 0, false, false, false)); 

            } 

 

 

            return lines; 

        } 

    } 

}



using System; 

using System.Collections.Generic; 

using System.Text; 

using System.Xml.Serialization; 

 

 

namespace Croc.Bpc.Printing.Reports.Templates 

{ 

    /// <summary> 

    /// ????? ?????? 

    /// </summary> 

    [Serializable, XmlType("Part")] 

    public class LinePart 

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

        /// ???????? 

        /// </summary> 

        [XmlAttribute("each")] 

        public string Each; 

        /// <summary> 

        /// ????????? ?? ??????? ??????????? ???? 

        /// </summary> 

        [XmlAttribute("in")] 

        public string In; 

 

 

        /// <summary> 

        /// ????? ?????? 

        /// </summary> 

        [XmlArray("Parts")] 

        [XmlArrayItem("Part", typeof(LinePart))] 

        public LinePart[] Part; 

 

 

        /// <summary> 

        /// ????? ?????? 


        /// </summary> 

        [XmlText] 

        public string Text; 

 

 

        /// <summary> 

        /// ????????????????? ?????? 

        /// </summary> 

        public string GetLine(ReportTemplateParser parser) 

        { 

            return ConstructLine(parser, Part, Text ?? String.Empty); 

        } 

 

 

        /// <summary> 

        /// ???????? ?????? ?? ?????? 

        /// </summary> 

        /// <param name="parser">??????</param> 

        /// <param name="parts">????? ??????</param> 

        /// <param name="defaultText">????? ?? ?????????, ???? ?????? ???</param> 

        /// <returns></returns> 

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



using System; 

using System.Collections.Generic; 

using System.Text; 

using System.Xml.Serialization; 

using Croc.Bpc.Election.Voting; 

 

 

namespace Croc.Bpc.Printing.Reports.Templates 

{ 

    /// <summary> 

    /// ????????? ?????? 

    /// </summary> 

    [Serializable] 

    public abstract class FontDefinition : BasePlainElement 

    { 

        /// <summary> 

        /// ?????? ?????? 

        /// </summary> 

        [XmlAttribute("fontSize")] 

        public string m_fontSize; 

 

 

        /// <summary> 

        /// ????? ???????????? ???????? ?????? 

        /// </summary> 

        [XmlIgnore] 

        public int FontSize 

        { 

            get 

            { 

                int fontSize = Managers.PrintingManager.ReportConfig.Font.Size; 

                if (m_fontSize != null && m_fontSize.Trim().Length > 0) 

                { 

                    int size = Convert.ToInt32(m_fontSize.Trim()); 

                    if (size < 0 || m_fontSize.Trim()[0] == '+') 

                    { 

                        // ????????????? ?????? ?????? 

                        fontSize += size; 

                    } 

                    else 

                    { 

                        // ?????????? ?????? 

                        fontSize = size; 

                    } 

                } 

 

 

                return fontSize; 

            } 

        } 


        /// <summary> 

        /// ??????? ?????? ?????? ??????? 

        /// </summary> 

        [XmlAttribute("bold")] 

        public bool IsBold; 

        /// <summary> 

        /// ??????? ?????? ???????? 

        /// </summary> 

        [XmlAttribute("italic")] 

        public bool IsItalic; 

        /// <summary> 

        /// ???????????? 

        /// </summary> 

        [XmlAttribute("align")] 

        public LineAlign Align; 

    } 

}



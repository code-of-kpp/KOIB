using System; 

using System.Collections.Generic; 

using System.Text; 

using System.Xml.Serialization; 

 

 

namespace Croc.Bpc.Printing.Reports.Templates 

{ 

    /// <summary> 

    /// Таблица 

    /// </summary> 

    [Serializable, XmlType("Table")] 

    public class Table 

    { 

        /// <summary> 

        /// Параметры столбцов 

        /// </summary> 

        [XmlArray("Header")] 

        [XmlArrayItem("Col")] 

        public ColDefinition[] Columns; 

 

 

        /// <summary> 

        /// Общий заголовок отчета 

        /// </summary> 

        [XmlArrayItem("Line", typeof(LineClause))] 

        [XmlArrayItem("For", typeof(ForClause))] 

        [XmlArrayItem("If", typeof(IfClause))] 

        [XmlArrayItem("SetCurrentRow", typeof(SetCurrentRow))] 

        public BasePlainElement[] Body; 

 

 

        /// <summary> 

        /// Признак заполнения точками 

        /// </summary> 

        [XmlAttribute("dotted")] 

        public bool IsDotted; 

    } 

}



using System; 

using System.Collections.Generic; 

using System.Text; 

using System.Xml.Serialization; 

 

 

namespace Croc.Bpc.Printing.Reports.Templates 

{ 

    /// <summary> 

    /// Параметры колонки 

    /// </summary> 

    public abstract class TableFontDefinition : FontDefinition 

    { 

        /// <summary> 

        /// Дополнение строк точками 

        /// </summary> 

        [XmlAttribute("dotted")] 

        public string IsDotted; 

    } 

}



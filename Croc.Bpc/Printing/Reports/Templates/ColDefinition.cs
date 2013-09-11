using System.Xml.Serialization; 

 

 

namespace Croc.Bpc.Printing.Reports.Templates 

{ 

    /// <summary> 

    /// Определение параметров колонки 

    /// </summary> 

    public class ColDefinition : FontDefinition 

    { 

        /// <summary> 

        /// ширина колонки в процентах 

        /// </summary> 

        [XmlAttribute("width")] 

        public int Width; 

 

 

        /// <summary> 

        /// Задает выражение для расчета количества столбцов 

        /// </summary> 

        [XmlAttribute("count")] 

        public string Count; 

 

 

        /// <summary> 

        /// NOTE: Не вызывается 

        /// </summary> 

        public override Lines ConstructContent(ReportTemplateParser parser) 

        { 

            return new Lines(); 

        } 

    } 

}



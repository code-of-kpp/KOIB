using System; 

using System.Collections.Generic; 

using System.Text; 

using System.Xml.Serialization; 

 

 

namespace Croc.Bpc.Printing.Reports.Templates 

{ 

    /// <summary> 

    /// Установка номера текущей строки 

    /// </summary> 

    [Serializable] 

    public class SetCurrentRow : BasePlainElement 

    { 

        /// <summary> 

        /// Выражение, устанавливающее номер строки 

        /// </summary> 

        [XmlText] 

        public string Value; 

 

 

        /// <summary> 

        /// Конструирует элемент 

        /// </summary> 

        /// <param name="parser"></param> 

        /// <returns></returns> 

        public override Lines ConstructContent(ReportTemplateParser parser) 

        { 

            Lines lines = new Lines(); 

 

 

            int rowNumber = 0; 

            // пробуем обработать как константу 

            if (!Int32.TryParse(Value, out rowNumber)) 

            { 

                // теперь пробуем обработать как переменную 

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



using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Bpc.Scanner 

{ 

    /// <summary> 

    /// Ошибка обработки листа 

    /// </summary> 

    public class SheetProcessingError 

    { 

        /// <summary> 

        /// Код ошибки 

        /// </summary> 

        public readonly int Code; 

        /// <summary> 

        /// Описание ошибки 

        /// </summary> 

        public readonly string Description; 

        /// <summary> 

        /// Является ли данная ошибка причиной реверса 

        /// </summary> 

        public readonly bool IsReverseReason; 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="code"></param> 

        /// <param name="description"></param> 

        /// <param name="isReverseReason"></param> 

        public SheetProcessingError( 

            int code, string description, bool isReverseReason) 

        { 

            Code = code; 

            Description = description; 

            IsReverseReason = isReverseReason; 

        } 

 

 

        /// <summary> 

        /// Возвращает строковое представление ошибки 

        /// </summary> 

        /// <returns></returns> 

        public override string ToString() 

        { 

            return string.Format("[{0}] {1}", Code, Description); 

        } 


    } 

}



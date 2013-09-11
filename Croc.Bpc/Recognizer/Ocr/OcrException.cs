using System; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Исключение драйвера распознавали 

    /// </summary> 

    public class OcrException : Exception 

    { 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="message">Строка</param> 

        public OcrException(string message) 

            : base(message) 

        { 

        } 

    } 

}



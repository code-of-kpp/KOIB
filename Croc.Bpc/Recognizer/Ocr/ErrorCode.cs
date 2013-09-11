using System; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Коды ошибок 

    /// </summary> 

    public enum ErrorCode 

    { 

        /// <summary> 

        /// Неправильное использование - ошибка приложения 

        /// </summary> 

        IllegalUse = 1, 

        /// <summary> 

        /// Неожиданная ошибка 

        /// </summary> 

        UnexpectedError, 

        /// <summary> 

        /// Не удалось создать модель 

        /// </summary> 

        LinkModelError, 

        /// <summary> 

        /// Не удалось начать распознавание 

        /// </summary> 

        StartRecognitionFailed, 

    } 

}



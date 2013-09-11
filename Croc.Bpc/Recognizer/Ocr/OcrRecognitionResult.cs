using System; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Результаты распознавания 

    /// </summary> 

    public enum OcrRecognitionResult 

    { 

        /// <summary> 

        /// Распознавание выполнено успешно 

        /// </summary> 

        OK = 0, 

        /// <summary> 

        /// Общая ошибка (сейчас возврат подробнее)  

        /// </summary> 

        NUF = -1, 

        /// <summary> 

        /// если распознавание было прервано 

        /// </summary> 

        BRK = -2, 

        /// <summary> 

        /// Не удалось обнаружить или узнать маркер 

        /// </summary> 

        MARK = -3, 

        /// <summary> 

        /// Не удалось локализовать требуемые линии 

        /// </summary> 

        SKEW = -4, 

        /// <summary> 

        /// Не удалось локализовать опорные точки 

        /// </summary> 

        REFP = -5, 

        /// <summary> 

        /// Не удалось локализовать квадраты  

        /// </summary> 

        FSQR = -6, 

        /// <summary> 

        /// При удалении черного сверху съели почти все 

        /// </summary> 

        CLRTOP = -7, 

        /// <summary> 

        /// При удвлении черного снизу  съели почти все  

        /// </summary> 

        CLRBOT = -8, 

        /// <summary> 

        /// Ошибка вызова функции 

        /// </summary> 

        CALL = -9, 


        /// <summary> 

        /// Не удалось найти нижнюю линию секции 

        /// </summary> 

        SCTLINE = -10, 

        /// <summary> 

        /// Недопустимый номер бюллетеня 

        /// </summary> 

        BULNUM = -11, 

        /// <summary> 

        /// Ошибка распознавания 

        /// </summary> 

        ERROR = -100 

    } 

}



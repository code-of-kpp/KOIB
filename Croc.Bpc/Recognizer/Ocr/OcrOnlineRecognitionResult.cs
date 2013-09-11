using System; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Результаты online-распознавания 

    /// </summary> 

    public enum OcrOnlineRecognitionResult 

    { 

        /// <summary> 

        /// Нет стандартного маркера 

        /// </summary> 

        ALIEN = -1, 

        /// <summary> 

        /// Бланк оказался двусторонним (возможно слипание) 

        /// </summary> 

        DOUBLE = -2, 

        /// <summary> 

        /// Нет вертикальных линий 

        /// </summary> 

        LINE = -3, 

        /// <summary> 

        /// Бюллетень имеет слишком большой перекос 

        /// </summary> 

        SKEW = -4, 

        /// <summary> 

        /// Загрязнение на верхней линейке 

        /// </summary> 

        DIRT1 = -5, 

        /// <summary> 

        /// Загрязнение на нижней линейке 

        /// </summary> 

        DIRT0 = -6, 

        /// <summary> 

        /// Нет изображения с верхней линейки 

        /// </summary> 

        BADIMAGE1 = -7, 

        /// <summary> 

        /// Нет изображения с нижней линейки 

        /// </summary> 

        BADIMAGE0 = -8 

    } 

}



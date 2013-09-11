namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Результат online-распознавания маркера 

    /// &gt;=0 - номер маркера 

    /// &lt;0  - ошибка 

    /// </summary> 

    enum OnlineMarkerResult 

    { 

        /// <summary> 

        /// маркер не определен  

        /// </summary> 

        Undefined = -1, 

        /// <summary> 

        /// проверка на бюллетень еще не прошла (т.е. распознавалка ждет следующего буфера) 

        /// </summary> 

        InProgress = -2, 

        /// <summary> 

        /// недопустимый номер бюллетеня (определили маркер, которого нет в модели) 

        /// </summary> 

        Impossible = -3, 

    } 

}



using System; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Делегат функций обратного вызова 

    /// </summary> 

    public delegate int OcrCallBack(OcrCallBackType type, IntPtr data, int size); 

 

 

    /// <summary> 

    /// Тип callback-функции 

    /// </summary> 

    public enum OcrCallBackType 

    { 

        /// <summary> 

        /// Сохранение модели 

        /// </summary> 

        ModelSave = 1, 

        /// <summary> 

        /// Восстановление модели 

        /// </summary> 

        ModelRestore, 

        /// <summary> 

        /// Сохранение данных 

        /// </summary> 

        DataSave, 

        /// <summary> 

        /// Восстановление данных 

        /// </summary> 

        DataRestore, 

        /// <summary> 

        /// Получение данных о печати 

        /// </summary> 

        GetStamp, 

        /// <summary> 

        /// Получение данных о числе печатей 

        /// </summary> 

        GetStampCount, 

        /// <summary> 

        /// Получение данных о минимальной ширине линий печати  

        /// </summary> 

        GetStampMinLineWidth, 

        /// <summary> 

        /// Получение данных о максимальной ширине  

        /// </summary> 

        GetStampMaxLineWidth, 

        /// <summary> 

        /// Уровень распознавания печати 


        /// </summary> 

        GetStampTestLevel, 

        /// <summary> 

        /// Уровень проверки небюллетеня 

        /// </summary> 

        GetInlineRecognitionLevel, 

        /// <summary> 

        /// Разрешение по стороне листа 

        /// </summary> 

        GetSideResolution, 

        /// <summary> 

        /// Получение списка не распознаваемых квадратов 

        /// (в частности, снятые позиции в бюллетене) 

        /// </summary> 

        GetExcludedSquares, 

        /// <summary> 

        /// Установить номер бюллетеня 

        /// </summary> 

        PutBulletinNumber, 

        /// <summary> 

        /// Возврат результатов 

        /// </summary> 

        PutResults, 

        /// <summary> 

        /// Сохранение изображения 

        /// </summary> 

        SaveImage, 

        /// <summary> 

        /// Показ изображения 

        /// </summary> 

        ShowImage, 

        /// <summary> 

        /// Получение порогов бинаризации 

        /// </summary> 

        GetBinThreshold, 

        /// <summary> 

        /// Получение размера файла с моделью 

        /// </summary> 

        GetModelFileSize, 

        /// <summary> 

        /// Уведомление о процессе выполнения операции 

        /// </summary> 

        ReportProgress, 

        /// <summary> 

        /// Получить размер буфера под полутоновое изображение 

        /// </summary> 

        GetGrayRectBuffSize, 

        /// <summary> 

        /// Получить полутоновое изображение 

        /// </summary> 


        GetGrayRectImage, 

        /// <summary> 

        /// Получить путь к файлу с бинарной моделью 

        /// </summary> 

        GetPath2Data, 

        /// <summary> 

        /// Получить квадраты с цифрами 

        /// </summary> 

        GetDigitSquares, 

        /// <summary> 

        /// Выгрузить результаты распознавания цифр 

        /// </summary> 

        UnloadDigitOcrResult, 

    }; 

}



using System; 

 

 

namespace Croc.Bpc.FileSystem 

{ 

    /// <summary> 

    /// Тип файла 

    /// </summary> 

    public enum FileType 

    { 

        /// <summary> 

        /// Файлы лога 

        /// </summary> 

        Log, 

        /// <summary> 

        /// Изображения, получаемые в процессе сканирования 

        /// </summary> 

        ScanningImage, 

        /// <summary> 

        /// Результаты распознавания 

        /// </summary> 

        RecognitionResult, 

        /// <summary> 

        /// Файлы с данными, которые появляются на время выполнения приложения 

        /// </summary> 

        RuntimeData, 

        /// <summary> 

        /// Файлы состояния 

        /// </summary> 

        State, 

        /// <summary> 

        /// Протокол с результатами голосования 

        /// </summary> 

        VotingResultProtocol, 

        /// <summary> 

        /// Отчет (печатная форма) 

        /// </summary> 

        Report, 

    } 

}



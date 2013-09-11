using System; 

 

 

namespace Croc.Bpc.Sound 

{ 

    /// <summary> 

    /// Интерфейс плеера звуковых файлов 

    /// </summary> 

    internal interface ISoundPlayer : IDisposable 

	{ 

        /// <summary> 

        /// Формат файлов 

        /// </summary> 

        string FileExt { get; } 

 

 

        /// <summary> 

        /// Запустить воспроизведение звукового файла 

        /// </summary> 

        /// <param name="soundFilePath"></param> 

        void Play(string soundFilePath); 

 

 

        /// <summary> 

        /// Остановить воспроизведение 

        /// </summary> 

        void Stop(); 

 

 

        /// <summary> 

        /// Событие "воспроизведение остановлено" 

        /// </summary> 

        event EventHandler PlayingStopped; 

	} 

}



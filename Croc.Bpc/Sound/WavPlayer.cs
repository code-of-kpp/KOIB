using System; 

using System.IO; 

using Croc.Core.Diagnostics; 

 

 

namespace Croc.Bpc.Sound 

{ 

    /// <summary> 

    /// Проигрыватель WAVE-файлов 

    /// </summary> 

    public sealed class WavPlayer : AbstractWavPlayer 

    { 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        public WavPlayer(ILogger logger, int latency, int afterPlayDelay) 

            : base(logger, latency, afterPlayDelay) 

        { 

        } 

 

 

        /// <summary> 

        /// Расширение файла 

        /// </summary> 

        public override string FileExt 

        { 

            get { return ".wav"; } 

        } 

 

 

        /// <summary> 

        /// Загружает файл в поток  

        /// </summary> 

        /// <param name="filePath">Путь к файлу</param> 

        /// <returns>Поток в виде WAV файла</returns> 

        protected override Stream InternalReadFile(string filePath) 

        { 

            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read); 

        } 

    } 

}



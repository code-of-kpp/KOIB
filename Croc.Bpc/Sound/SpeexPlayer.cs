using System; 

using System.IO; 

using Croc.Core.Diagnostics; 

using cspeex; 

 

 

namespace Croc.Bpc.Sound 

{ 

    /// <summary> 

    /// Проигрыватель Speex-файлов 

    /// </summary> 

    public sealed class SpeexPlayer : AbstractWavPlayer 

    { 

        /// <summary> 

        /// Speex декодер 

        /// </summary> 

        private readonly JSpeexDec _decoder = new JSpeexDec(); 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        public SpeexPlayer(ILogger logger, int latency, int afterPlayDelay) 

            : base(logger, latency, afterPlayDelay) 

        { 

            // так как проигрывание рассчитано на WAVE, то будем создавать поток в виде wav файла 

            _decoder.setDestFormat(JSpeexDec.FILE_FORMAT_WAVE); 

        } 

 

 

        /// <summary> 

        /// Расширение файла 

        /// </summary> 

        public override string FileExt 

        { 

            get { return ".spx"; } 

        } 

 

 

        /// <summary> 

        /// Получает декодированный поток  

        /// </summary> 

        /// <param name="stream">Входной поток</param> 

        /// <returns>Выходной поток</returns> 

        private Stream GetDecodedStream(Stream stream) 

        { 

            MemoryStream memoryStream = new MemoryStream(); 

            _decoder.decode(new java.io.RandomInputStream(stream), new java.io.RandomOutputStream(memoryStream)); 

            return memoryStream; 

        } 


 
 

        /// <summary> 

        /// Загружает файл в поток с выполнением декодирования 

        /// </summary> 

        /// <param name="filePath">Путь к файлу</param> 

        /// <returns>Поток в виде WAV файла</returns> 

        protected override Stream InternalReadFile(string filePath) 

        { 

            return GetDecodedStream(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)); 

        } 

    } 

}



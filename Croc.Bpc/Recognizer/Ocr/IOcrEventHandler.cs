using System; 

using Croc.Bpc.Common; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Интерфейс для обработки событий драйвера распознавалки 

    /// </summary> 

    public interface IOcrEventHandler 

    { 

        /// <summary> 

        /// Получить полутон 

        /// </summary> 

        /// <param name="ocr">pOCR</param> 

        /// <param name="side">Сторона</param> 

        /// <param name="x">Координата по X</param> 

        /// <param name="y">Координата по Y</param> 

        /// <param name="height">Высота</param> 

        /// <param name="width">Ширина</param> 

        /// <param name="image">Буфер для полутона</param> 

        /// <returns>Результат выполнения</returns> 

        int GetHalfToneBuffer(IOcr ocr, short side, int x, int y, int height, int width, MemoryBlock image); 

        /// <summary> 

        /// Получить порог бинаризации 

        /// </summary> 

        /// <param name="ocr">pOCR</param> 

        /// <param name="side">Сторона</param> 

        /// <returns>порог бинаризации</returns> 

        int GetBinaryThreshold(IOcr ocr, short side); 

        /// <summary> 

        /// Передать ошибку распознавания 

        /// </summary> 

        /// <param name="ocr">pOCR</param> 

        /// <param name="errorCode">Код ошибки</param> 

        /// <param name="message">Сообщение</param> 

        void Error(IOcr ocr, int errorCode, string message); 

        /// <summary> 

        /// Передать отладочное сообщение 

        /// </summary> 

        /// <param name="ocr">pOCR</param> 

        /// <param name="message">Сообщение</param> 

        void AppendToLog(IOcr ocr, string message); 

    } 

}



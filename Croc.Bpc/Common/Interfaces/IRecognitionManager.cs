using System; 

using Croc.Core; 

 

 

namespace Croc.Bpc.Recognizer 

{ 

    /// <summary> 

    /// Интерфейс менеджера распознавания 

    /// </summary> 

    public interface IRecognitionManager : ISubsystem 

    { 

        /// <summary> 

        /// Инициализация распознавания 

        /// </summary> 

        void InitRecognition(); 

 

 

        /// <summary> 

        /// Запустить распознавание сканируемого листа 

        /// </summary> 

        /// <param name="lineWidth">ширина линейки сканера</param> 

        void RunRecognition(int lineWidth); 

 

 

        /// <summary> 

        /// Обработать следующий буфер сканируемого листа 

        /// </summary> 

        /// <param name="str0"></param> 

        /// <param name="str1"></param> 

        /// <param name="blankMarker">маркет бланка - заполняется в случае, когда метод возвращает true</param> 

        /// <returns>true - Бюллетень распознан в режиме Online; false - не распознан</returns> 

        bool ProcessNextBuffer(short str0, short str1, out int blankMarker); 

 

 

        /// <summary> 

        /// Завершить распознавание путем сброса состояния распознавалки 

        /// </summary> 

        void ResetRecognition(); 

 

 

        /// <summary> 

        /// Завершить распознавание отсканированного листа 

        /// </summary> 

        void EndRecognition(); 

 

 

		/// <summary> 

        /// Сохранить бинар последнего изображения при ошибке драйвера 

		/// </summary> 

		/// <param name="errorCode">код ошибки драйвера</param> 


        void SaveLastImageOnDriverError(int errorCode); 

 

 

		/// <summary> 

        /// Нужно ли сохранять изображения, когда драйвер реверсирует лист 

		/// </summary> 

		bool NeedSaveImageOnDriverReverse 

		{ 

			get; 

		} 

 

 

		/// <summary> 

		/// Разрешен ли контроль печати УИК 

		/// </summary> 

		bool StampControlEnabled{ get; set; } 

    } 

}



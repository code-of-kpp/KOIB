using System; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Режимы распознавания печати 

    /// </summary> 

	public enum StampTestLevel 

	{ 

        /// <summary> 

        /// нет контроля печати 

        /// </summary> 

		None = 1, 

        /// <summary> 

        /// тест на наличие печати 

        /// </summary> 

		Found = 2, 

        /// <summary> 

        /// распознавание номера печати 

        /// </summary> 

        Recognize = 3, 

        /// <summary> 

        /// новое распознавание печати 

        /// </summary> 

		RecognizeNew = 4, 

        /// <summary> 

        /// Полутоновое распознавание печати 

        /// </summary> 

        Halftone = 5, 

	}; 

}



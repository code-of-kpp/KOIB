using System; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Тип маркера 

    /// </summary> 

	public enum MarkerType 

	{ 

        /// <summary> 

        /// без анализа 

        /// </summary> 

		None = 0, 

        /// <summary> 

        /// документ со стандартным маркером 

        /// </summary> 

		Standard = 1, 

        /// <summary> 

        /// документ с цифровым маркером 

        /// </summary> 

		Digital = 2 

	}; 

}



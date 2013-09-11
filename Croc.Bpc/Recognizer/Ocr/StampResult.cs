using System; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Результаты распознавания печати 

    /// </summary> 

	public enum StampResult 

	{ 

        /// <summary> 

        /// Печать распознана 

        /// </summary> 

		YES = 1, 

        /// <summary> 

        /// вызов до распознавания или в случае его неудачи 

        /// </summary> 

		CALL_ERROR = -1,		 

        /// <summary> 

        /// отсутсвует вообще 

        /// </summary> 

		EMPTY = -2,			 

        /// <summary> 

        /// слишком бледная 

        /// </summary> 

		FAINT	= -3,			 

        /// <summary> 

        /// не обнаружены необходимые линии печати 

        /// </summary> 

		BADLINES = -4,		 

        /// <summary> 

        /// плохая печать 

        /// </summary> 

		BADPRINT = -5,		 

	}; 

}



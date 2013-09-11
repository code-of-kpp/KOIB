using System; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Режимы online распознавания бюллетеня 

    /// </summary> 

	public enum InlineLevel 

	{ 

        /// <summary> 

        /// не проверять 

        /// </summary> 

		None = 0,				 

        /// <summary> 

        /// проверять верт.линии 

        /// </summary> 

		Line = 1,				 

        /// <summary> 

        /// проверять штрих маркер 

        /// </summary> 

		Mark = 2,				 

        /// <summary> 

        /// проверять верт.линии + штрих маркер 

        /// </summary> 

		LineMark = 3,		 

        /// <summary> 

        /// проверять цифровой маркер 

        /// </summary> 

		Digital = 4,           

        /// <summary> 

        /// проверять верт.линии + цифровой маркер 

        /// </summary> 

		LineDigital = 5       

	}; 

}



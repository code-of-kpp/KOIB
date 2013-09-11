using System; 

using System.Runtime.InteropServices; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Результаты распознавания 

    /// </summary> 

    [StructLayout(LayoutKind.Sequential)] 

    struct	OcrResult 

    { 

        /// <summary> 

        /// номер выборов в текущем бюллетене 

        /// </summary> 

        public int		PollNum;	 

        /// <summary> 

        /// если > 0, то действительный результат 

        /// </summary> 

        public int		IsValid;	 

        /// <summary> 

        /// число помеченных квадратов 

        /// </summary> 

        public int		numChecked;	 

        /// <summary> 

        /// массив номеров помеченных квадратов (по возрастанию) 

        /// </summary> 

        public IntPtr	sqData;		 

        /// <summary> 

        /// значения распознанных цифр 

        /// </summary> 

        public IntPtr	piDigits;	 

    }; 

}



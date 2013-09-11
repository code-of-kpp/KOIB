using System; 

using System.Runtime.InteropServices; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Установленное разрешение 

    /// </summary> 

    [StructLayout(LayoutKind.Sequential)] 

    struct Resolution 

    { 

        /// <summary> 

        /// сторона листа 

        /// </summary> 

        public int side; 

        /// <summary> 

        /// координаты 

        /// </summary> 

        public float x, y; 

    }; 

}



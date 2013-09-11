using System; 

using System.Runtime.InteropServices; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Rectangle 

    /// </summary> 

    [StructLayout(LayoutKind.Sequential)] 

    public struct AlRect     

    { 

        /// <summary> 

        /// left-top vertex, width, height 

        /// </summary> 

        public int  x, y, w, h;            

    }; 

}



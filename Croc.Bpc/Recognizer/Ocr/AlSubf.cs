using System; 

using System.Runtime.InteropServices; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Defines rectangle patch of image 

    /// </summary> 

    [StructLayout(LayoutKind.Sequential)] 

    public struct AlSubf    

    { 

        /// <summary> 

        /// pointer to 1-st byte of image patch 

        /// </summary> 

        public IntPtr    @base;       

        /// <summary> 

        /// width of full image in bytes 

        /// </summary> 

        public int       width;       

        /// <summary> 

        /// 1-st meaning bit in 1-st byte of line 

        /// </summary> 

        public int       lbit; 

        /// <summary> 

        /// x-size of image patch in pixels 

        /// </summary> 

        public int       xs;   

        /// <summary> 

        /// y-size of image patch in pixels 

        /// </summary> 

        public int       ys;   

        /// <summary> 

        /// x-coord. of top-left point of patch 

        /// </summary> 

        public int       x;    

        /// <summary> 

        /// y-coord. of top-left point of patch 

        /// </summary> 

        public int       y;           

    }; 

}



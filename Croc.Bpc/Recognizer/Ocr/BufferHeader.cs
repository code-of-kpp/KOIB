using System; 

using System.Runtime.InteropServices; 

 

 

namespace Croc.Bpc.Recognizer.Ocr 

{ 

    /// <summary> 

    /// Описатель изображения для распознавалки 

    /// </summary> 

    [StructLayout(LayoutKind.Sequential)] 

    public struct BufferHeader 

	{ 

        /// <summary> 

        /// All size of buffer 

        /// </summary> 

		public uint	 size;		      

        /// <summary> 

        /// Size of binary buffer 

        /// </summary> 

		public uint	 bin_size;	      

        /// <summary> 

        /// Size of tone buffer 

        /// </summary> 

		public uint	 tone_size;	      

        /// <summary> 

        /// Буфер 

        /// </summary> 

		public IntPtr   all_bin_buf;          

        /// <summary> 

        /// Begin of binary buffer 

        /// </summary> 

        public IntPtr 	 bin_buf;	      

        /// <summary> 

        /// Begin of tone buffer 

        /// </summary> 

        public IntPtr 	 tone_buf;	      

        /// <summary> 

        /// width of image in pixel 

        /// </summary> 

		public int		 ImageWidth;          

        /// <summary> 

        /// width of image in bytes 

        /// </summary> 

		public int		 ByteImageWidth;  

        /// <summary> 

        /// Width of binary string 

        /// </summary> 

		public int		 bin_width;	 

        /// <summary> 

        /// Areas of tone stripes 


        /// </summary> 

        public GrayParam g_p0;	  

        /// <summary> 

        /// Areas of tone stripes 

        /// </summary> 

        public GrayParam g_p1;	  

        /// <summary> 

        /// Areas of tone stripes 

        /// </summary> 

        public GrayParam g_p2;	      

        /// <summary> 

        /// Внутренний флаг распознавалки 

        /// </summary> 

        public int		 flag;		     

        /// <summary> 

        /// actual scans number in buffer 

        /// </summary> 

		public int		 ActualScanNumber;    

        /// <summary> 

        /// actual processed scans number 

        /// </summary> 

		public int		 ScanCounter;         

        /// <summary> 

        /// if TRUE, then horz. line is found 

        /// </summary> 

		public int		 OutFlag;             

        /// <summary> 

        /// counter of horz. lines 

        /// </summary> 

		public int		 LineCount;           

        /// <summary> 

        /// first line data 

        /// </summary> 

		public int		 xl,yl,xr,yr;         

	} 

 

 

    /// <summary> 

    /// Параметры полутона 

    /// </summary> 

    [StructLayout(LayoutKind.Sequential)] 

    public struct GrayParam 

    { 

        /// <summary> 

        /// Смещение 

        /// </summary> 

        public int start; 

        /// <summary> 

        /// Длина 

        /// </summary> 


        public int len; 

    }; 

}



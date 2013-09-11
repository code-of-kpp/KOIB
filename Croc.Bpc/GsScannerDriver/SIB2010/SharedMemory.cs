using System.Runtime.InteropServices; 

 

 

namespace Croc.Bpc.GsScannerDriver.SIB2010 

{ 

    /// <summary> 

    /// Структура общей памяти, разделяемой между менеджером драйвера и классом управления сканером версии 2009 

    /// </summary> 

    public class SharedMemory : BaseSharedMemory 

    { 

        /// <summary> 

        /// максимальное число линий 

        /// </summary> 

        public const int MaxLines = 12000; 

 

 

        /// <summary> 

        /// число датчиков 

        /// </summary> 

        public const int DotsOneLine = 3456; 

 

 

        /// <summary> 

        /// число датчиков с одной стороны 

        /// </summary> 

        public const int DotsOneSide = DotsOneLine / 2; 

 

 

        /// <summary> 

        /// Размер полутонового буфера 

        /// </summary> 

        public const int SizeofHalftoneBuffer = DotsOneSide * MaxLines / DotsPerByteHalftone; 

 

 

        /// <summary> 

        /// Размер бинарного буфера 

        /// </summary> 

        public const int SizeofBinaryBuffer = DotsOneSide * MaxLines / DotsPerByteBinary; 

 

 

        /// <summary> 

        /// Структура разделяемой памяти 

        /// </summary> 

        [StructLayout(LayoutKind.Sequential)] 

        protected struct sharedData 

        { 

            public int writeStrings; 

            public int endScan; 

            public uint countPureStrips; 

            public uint comReverse; 


            public uint error, needReverse, resReverse; 

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SizeofHalftoneBuffer)] 

            public byte[] oneSide; 

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SizeofBinaryBuffer)] 

            public byte[] oneSideBin; 

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DotsOneSide)] 

            public byte[] oneSideCoefBin; 

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DotsOneSide)] 

            public byte[] oneSideCoefDirt; 

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DotsOneSide)] 

            public byte[] oneSideDirt; 

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DotsOneSide)] 

            public byte[] debugData; 

            public SListFormatsEqualSize listFormats; 

            public ManufProps props; 

            public ScannerProps sprops; 

            public uint noSound; 

            public uint magic; 

        } 

 

 

        /// <summary> 

        /// Подключиться к существующей общей памяти 

        /// </summary> 

        public SharedMemory() 

            : base(false, typeof(sharedData), true) 

        { 

        } 

    } 

}



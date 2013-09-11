using System.Runtime.InteropServices; 
namespace Croc.Bpc.GsScannerDriver.SIB2010 
{ 
    public class SharedMemory : BaseSharedMemory 
    { 
        public const int MaxLines = 5000; 
        public const int DotsOneLine = 3456; 
        public const int DotsOneSide = DotsOneLine / 2; 
        public const int SizeofHalftoneBuffer = DotsOneSide * MaxLines / DotsPerByteHalftone; 
        public const int SizeofBinaryBuffer = DotsOneSide * MaxLines / DotsPerByteBinary; 
        [StructLayout(LayoutKind.Sequential)] 
        protected struct sharedData 
        { 
            public int writeStrings; 
            public int endScan; 
            public uint expectedLength; 
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)] 
            public short[] doubleValuesL; 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)] 
            public short[] doubleValuesR; 
            public uint minVolt, maxVolt, sumVolt, countVolt; 
        } 
        public SharedMemory() 
            : base(false, typeof(sharedData), true) 
        { 
        } 
    } 
}

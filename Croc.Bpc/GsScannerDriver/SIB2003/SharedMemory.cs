using System.Runtime.InteropServices; 
namespace Croc.Bpc.GsScannerDriver.SIB2003 
{ 
    public class SharedMemory : BaseSharedMemory 
    { 
        public const int MaxLines = 4400; 
        public const int DotsOneLine = 5568; 
        public const int DotsOneSide = DotsOneLine / 2; 
        public const int SizeofHalftoneBuffer = DotsOneSide * MaxLines / DotsPerByteHalftone; 
        public const int SizeofBinaryBuffer = DotsOneSide * MaxLines / DotsPerByteBinary; 
        [StructLayout(LayoutKind.Sequential)] 
        protected struct sharedData 
        { 
            public int fds, fdp, fdi; 
            public int writeStrings; 
            public int endScan; 
            public uint countPureStrips; 
            public uint comReverse; 
            public uint error, needReverse, resReverse; 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SizeofHalftoneBuffer)] 
            public byte[] oneSide; 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SizeofHalftoneBuffer)] 
            public byte[] twoSide; 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SizeofBinaryBuffer)] 
            public byte[] oneSideBin; 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SizeofBinaryBuffer)] 
            public byte[] twoSideBin; 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DotsOneSide)] 
            public byte[] oneSideCoefBin; 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DotsOneSide)] 
            public byte[] twoSideCoefBin; 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DotsOneSide)] 
            public byte[] oneSideCoefDirt; 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DotsOneSide)] 
            public byte[] twoSideCoefDirt; 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DotsOneSide)] 
            public byte[] oneSideDirt; 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DotsOneSide)] 
            public byte[] twoSideDirt; 
            public SListFormatsEqualSize listFormats; 
            public SListShiftsEqualSize listShifts; 
            public SProps props; 
            public ScannerProps sprops; 
            public uint magic; 
        } 
        public SharedMemory() 
            : base(false, typeof(sharedData), false) 
        { 
        } 
    } 
}

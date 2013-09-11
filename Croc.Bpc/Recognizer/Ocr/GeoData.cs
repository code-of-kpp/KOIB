using System; 
using System.Runtime.InteropServices; 
namespace Croc.Bpc.Recognizer.Ocr 
{ 
    [StructLayout(LayoutKind.Sequential)] 
    public struct GeoData 
    { 
        public const int MAX_SQUARES = 127; 
        public int result; 
        public int topMarker; 
        public int topMarkerColor; 
        public int bottomMarker; 
        public int bottomMarkerColor; 
        public int markerQuality; 
        public int baseLineSkew; 
        public int baseLineColor; 
        public int baseLineQuality; 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_SQUARES + 1, ArraySubType = UnmanagedType.I4)] 
        public int[] squares; 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_SQUARES + 1, ArraySubType = UnmanagedType.I4)] 
        public int[] squaresSkewV; 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_SQUARES + 1, ArraySubType = UnmanagedType.I4)] 
        public int[] squaresSkewH; 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_SQUARES + 1, ArraySubType = UnmanagedType.I4)] 
        public int[] squaresSize; 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_SQUARES + 1, ArraySubType = UnmanagedType.I4)] 
        public int[] squaresColor; 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_SQUARES + 1, ArraySubType = UnmanagedType.I4)] 
        public int[] squaresQuality; 
        public int ColorQuality; 
    }; 
}

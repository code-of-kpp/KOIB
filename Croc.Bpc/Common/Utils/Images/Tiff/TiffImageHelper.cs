using System; 
using System.Runtime.InteropServices; 
namespace Croc.Bpc.Utils.Images.Tiff 
{ 
    public static class TiffImageHelper 
    { 
        public static void SaveToFile( 
            string fileName, ImageType imageType, MemoryBlock memBlock, int lineWidth, int lineCount) 
        { 
            if(lineCount <= 0) 
            { 
                return; 
            } 
            IntPtr memoryPtr = memBlock.ToPointer(); 
            IntPtr tiff = Open(fileName, "w"); 
            if (tiff == IntPtr.Zero) 
                throw new Exception("Unable write TIFF file: " + fileName); 
            try 
            { 
                int bitPerSample = (imageType == ImageType.Binary ? 1 : 8); 
                SetIntField(tiff, FieldName.IMAGEWIDTH, lineWidth); 
                SetIntField(tiff, FieldName.IMAGELENGTH, lineCount); 
                SetIntField(tiff, FieldName.BITSPERSAMPLE, bitPerSample); 
                SetIntField(tiff, FieldName.SAMPLESPERPIXEL, 1); 
                SetIntField(tiff, FieldName.ROWSPERSTRIP, lineCount); 
                if (imageType == ImageType.Binary) 
                { 
                    SetIntField(tiff, FieldName.COMPRESSION, (int)CompressionType.CCITTFAX4); 
                    SetIntField(tiff, FieldName.PHOTOMETRIC, (int)Photometric.MINISWHITE); 
                } 
                else 
                { 
                    SetIntField(tiff, FieldName.COMPRESSION, (int)CompressionType.LZW); 
                    SetIntField(tiff, FieldName.PHOTOMETRIC, (int)Photometric.MINISBLACK); 
                } 
                SetIntField(tiff, FieldName.FILLORDER, (int)Fillorder.MSB2LSB); 
                SetIntField(tiff, FieldName.PLANARCONFIG, (int)PlanarConfig.CONTIG); 
                SetFloatField(tiff, FieldName.XRESOLUTION, 200.0); 
                SetFloatField(tiff, FieldName.YRESOLUTION, 200.0); 
                SetIntField(tiff, FieldName.RESOLUTIONUNIT, (int)ResolutionUnit.INCH); 
                WriteEncodedStrip(tiff, 0, memoryPtr, lineWidth * lineCount * bitPerSample / 8); 
            } 
            finally 
            { 
                Close(tiff); 
            } 
        } 
        public static bool ReadFromFile(string fileName, ref MemoryBlock memBlock, ref TiffImageInfo tiffInfo) 
        { 
            IntPtr tiff = Open(fileName, "r"); 
            if (tiff == IntPtr.Zero) 
            { 
                throw new Exception("Unable to open TIFF file: " + fileName); 
            } 
            try 
            { 
                UInt16 i16buf = 0;   // буфер для чтения параметров 
                GetIntField(tiff, FieldName.PLANARCONFIG, ref i16buf); 
                tiffInfo.config = (PlanarConfig)i16buf; 
                GetIntField(tiff, FieldName.BITSPERSAMPLE, ref tiffInfo.bps); 
                GetIntField(tiff, FieldName.SAMPLESPERPIXEL, ref tiffInfo.spp); 
                if ((tiffInfo.bps != 1 && tiffInfo.bps != 8) || 
                    tiffInfo.spp != 1 || 
                    tiffInfo.config != PlanarConfig.CONTIG) 
                { 
                    throw new Exception("TIFF parameters do not meet requirements: bps = " + tiffInfo.bps + ", spp = " + tiffInfo.spp + ", config = " + tiffInfo.config); 
                } 
                GetIntField(tiff, FieldName.IMAGEWIDTH, ref tiffInfo.width); 
                GetIntField(tiff, FieldName.IMAGELENGTH, ref tiffInfo.height); 
                GetIntField(tiff, FieldName.PHOTOMETRIC, ref i16buf); 
                tiffInfo.photometric = (Photometric)i16buf; 
                if (tiffInfo.photometric != Photometric.MINISWHITE && 
                    tiffInfo.photometric != Photometric.MINISBLACK) 
                { 
                    tiffInfo.photometric = Photometric.MINISBLACK; 
                } 
                if (memBlock == null) 
                { 
                    memBlock = new MemoryBlock(); 
                } 
                int nSize = (int)((tiffInfo.width * tiffInfo.height * tiffInfo.bps) / 8); 
                if (memBlock.SizeOf < nSize) 
                { 
                    memBlock.Free(); 
                    memBlock.Alloc(nSize); 
                } 
                IntPtr pMemory = memBlock.ToPointer(); 
                int scanSize = GetScanlineSize(tiff); 
                IntPtr bufPtr = Marshal.AllocHGlobal(scanSize); 
                byte[] oTemp = new byte[nSize]; 
                try 
                { 
                    for (int row = 0; row < tiffInfo.height; row++) 
                    { 
                        ReadScanline(tiff, bufPtr, row, 0); 
                        int nDelta = (int)((tiffInfo.width * tiffInfo.bps) / 8 * row); 
                        Marshal.Copy(bufPtr, oTemp, nDelta, scanSize); 
                    } 
                    Marshal.Copy(oTemp, 0, pMemory, nSize); 
                    return true; 
                } 
                finally 
                { 
                    Marshal.FreeHGlobal(bufPtr); 
                } 
            } 
            finally 
            { 
                Close(tiff); 
            } 
        } 
        #region Extern-ы для работы с libtiff.dll 
        [DllImport("libtiff.dll", EntryPoint = "TIFFOpen")] 
        private static extern IntPtr Open(string fileName, string openMode); 
        [DllImport("libtiff.dll", EntryPoint = "TIFFClose")] 
        private static extern void Close(IntPtr tiff); 
        [DllImport("libtiff.dll", EntryPoint = "TIFFSetField")] 
        private static extern void SetIntField(IntPtr tiff, FieldName fieldName, int fieldValue); 
        [DllImport("libtiff.dll", EntryPoint = "TIFFSetField")] 
        private static extern void SetFloatField(IntPtr tiff, FieldName fieldName, double fieldValue); 
        [DllImport("libtiff.dll", EntryPoint = "TIFFGetField")] 
        private static extern int GetIntField(IntPtr tiff, FieldName fieldName, ref UInt32 fieldValue); 
        [DllImport("libtiff.dll", EntryPoint = "TIFFGetField")] 
        private static extern int GetIntField(IntPtr tiff, FieldName fieldName, ref UInt16 fieldValue); 
        [DllImport("libtiff.dll", EntryPoint = "TIFFScanlineSize")] 
        private static extern int GetScanlineSize(IntPtr tiff); 
        [DllImport("libtiff.dll", EntryPoint = "TIFFReadScanline")] 
        private static extern int ReadScanline(IntPtr tiff, IntPtr buf, int row, int sample); 
        [DllImport("libtiff.dll", EntryPoint = "TIFFWriteEncodedStrip")] 
        private static extern int WriteEncodedStrip(IntPtr tiff, Int16 stripNumber, IntPtr data, int dataSize); 
        #endregion 
    } 
}

using System; 

using System.Runtime.InteropServices; 

 

 

namespace Croc.Bpc.Common.Images.Tiff 

{ 

    /// <summary> 

    /// Класс для работы с картинками в формате TIFF 

    /// </summary> 

    public static class TiffImageHelper 

    { 

        /// <summary> 

        /// Сохранить участок  

        /// </summary> 

        /// <param name="memBlock">область памяти</param> 

        /// <param name="imageType">тип изображения</param> 

        /// <param name="fileName">имя файла</param> 

        /// <param name="lineWidth">ширина сохраняемой области в точках</param> 

        /// <param name="lineCount">высота сохраняемой области</param> 

        public static void SaveToFile( 

            string fileName, ImageType imageType, MemoryBlock memBlock, int lineWidth, int lineCount) 

        { 

            if(lineCount < 0) 

            { 

                // защитимся от попытке сохранить изображение без строк 

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

 

 

        /// <summary> 

		/// Загружает TIFF файл 

		/// </summary> 

		/// <param name="fileName">Имя файла</param> 

		/// <param name="pMemBlock">Область памяти</param> 

		/// <param name="tiffInfo">Параметры файла</param> 

		/// <returns>true в случае успешной загрузки</returns> 

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

 

 

                // прочтем тип хранения 

                GetIntField(tiff, FieldName.PLANARCONFIG, ref i16buf); 

                tiffInfo.config = (PlanarConfig)i16buf; 

                // прочтем колво бит на пиксел 

                GetIntField(tiff, FieldName.BITSPERSAMPLE, ref tiffInfo.bps); 

                // колво сэмплов на пиксел 


                GetIntField(tiff, FieldName.SAMPLESPERPIXEL, ref tiffInfo.spp); 

 

 

                if ((tiffInfo.bps != 1 && tiffInfo.bps != 8) || 

                    tiffInfo.spp != 1 || 

                    tiffInfo.config != PlanarConfig.CONTIG) 

                { 

                    // работаем только с бинарными black & white или полутоновыми 

                    throw new Exception("TIFF parameters do not meet requirements: bps = " + tiffInfo.bps + ", spp = " + tiffInfo.spp + ", config = " + tiffInfo.config); 

                } 

 

 

                // ширина изображения 

                GetIntField(tiff, FieldName.IMAGEWIDTH, ref tiffInfo.width); 

                // высота 

                GetIntField(tiff, FieldName.IMAGELENGTH, ref tiffInfo.height); 

                // прочитать какой цвет задается кодом 0x00 

                GetIntField(tiff, FieldName.PHOTOMETRIC, ref i16buf); 

                tiffInfo.photometric = (Photometric)i16buf; 

 

 

                if (tiffInfo.photometric != Photometric.MINISWHITE && 

                    tiffInfo.photometric != Photometric.MINISBLACK) 

                { 

                    // тем не менее получили какой-то бред 

                    // считаем тогда, что у нас случай  PHOTOMETRIC_MINISBLACK 

                    tiffInfo.photometric = Photometric.MINISBLACK; 

                } 

 

 

                // Проверим, что нам передали 

                if (memBlock == null) 

                { 

                    memBlock = new MemoryBlock(); 

                } 

 

 

                // Размер изображения в байтах 

                int nSize = (int)((tiffInfo.width * tiffInfo.height * tiffInfo.bps) / 8); 

 

 

                // если у нас блок меньшего размера, то выполним выделение заново 

                if (memBlock.SizeOf < nSize) 

                { 

                    memBlock.Free(); 

                    memBlock.Alloc(nSize); 

                } 

 

 

                IntPtr pMemory = memBlock.ToPointer(); 


 
 

                // размер скан строки в байтах 

                int scanSize = GetScanlineSize(tiff); 

 

 

                // выделим память для скан строки 

                IntPtr bufPtr = Marshal.AllocHGlobal(scanSize); 

                byte[] oTemp = new byte[nSize]; 

                try 

                { 

                    // читаем последовательно скан строки из TIFF 

                    for (int row = 0; row < tiffInfo.height; row++) 

                    { 

                        ReadScanline(tiff, bufPtr, row, 0); 

                        // и пишем строки в битмап 

                        int nDelta = (int)((tiffInfo.width * tiffInfo.bps) / 8 * row); 

                        Marshal.Copy(bufPtr, oTemp, nDelta, scanSize); 

                    } 

 

 

                    // Копирую изображение в буфер памяти 

                    Marshal.Copy(oTemp, 0, pMemory, nSize); 

 

 

                    return true; 

                } 

                finally 

                { 

                    // освободим память 

                    Marshal.FreeHGlobal(bufPtr); 

                } 

            } 

            finally 

            { 

                Close(tiff); 

            } 

        } 

 

 

        #region Extern-ы для работы с libtiff.dll 

 

 

        /// <summary> 

        /// Открыть файл с изображением 

        /// </summary> 

        /// <param name="fileName">имя файла</param> 

        /// <param name="openMode">режим открытия</param> 

        /// <returns>описатель файла</returns> 

        [DllImport("libtiff.dll", EntryPoint = "TIFFOpen")] 


        private static extern IntPtr Open(string fileName, string openMode); 

 

 

        /// <summary> 

        /// Закрыть файл 

        /// </summary> 

        /// <param name="tiff">описатель файла</param> 

        [DllImport("libtiff.dll", EntryPoint = "TIFFClose")] 

        private static extern void Close(IntPtr tiff); 

 

 

        /// <summary> 

        /// Установить значение поля 

        /// </summary> 

        /// <param name="tiff">описатель файла</param> 

        /// <param name="fieldName">имя поля</param> 

        /// <param name="fieldValue">задаваемое значение</param> 

        [DllImport("libtiff.dll", EntryPoint = "TIFFSetField")] 

        private static extern void SetIntField(IntPtr tiff, FieldName fieldName, int fieldValue); 

 

 

        /// <summary> 

        /// Установить значение поля 

        /// </summary> 

        /// <param name="tiff">описатель файла</param> 

        /// <param name="fieldName">имя поля</param> 

        /// <param name="fieldValue">задаваемое значение</param> 

        [DllImport("libtiff.dll", EntryPoint = "TIFFSetField")] 

        private static extern void SetFloatField(IntPtr tiff, FieldName fieldName, double fieldValue); 

 

 

        /// <summary> 

        /// Получить значение поля 

        /// </summary> 

        /// <param name="tiff">описатель файла</param> 

        /// <param name="fieldName">имя поля</param> 

        /// <param name="fieldValue">значение поля</param> 

        /// <returns>1 если тэг есть, 0 - тэга нет</returns> 

        [DllImport("libtiff.dll", EntryPoint = "TIFFGetField")] 

        private static extern int GetIntField(IntPtr tiff, FieldName fieldName, ref UInt32 fieldValue); 

 

 

        /// <summary> 

        /// Получить значение поля 

        /// </summary> 

        /// <param name="tiff">описатель файла</param> 

        /// <param name="fieldName">имя поля</param> 

        /// <param name="fieldValue">значение поля</param> 

        /// <returns>1 если тэг есть, 0 - тэга нет</returns> 

        [DllImport("libtiff.dll", EntryPoint = "TIFFGetField")] 


        private static extern int GetIntField(IntPtr tiff, FieldName fieldName, ref UInt16 fieldValue); 

 

 

        /// <summary> 

        /// Получить размер в байтах одной строки (скан-линии) файла 

        /// </summary> 

        /// <param name="tiff">описатель файла</param> 

        /// <returns>размер строки в байтах</returns> 

        [DllImport("libtiff.dll", EntryPoint = "TIFFScanlineSize")] 

        private static extern int GetScanlineSize(IntPtr tiff); 

 

 

        /// <summary> 

        /// Прочитать одну строку 

        /// </summary> 

        /// <param name="tiff">описатель файла</param> 

        /// <param name="buf">Буфер</param> 

        /// <param name="row">Номер строки</param> 

        /// <param name="sample">Номер сэмпла</param> 

        /// <returns>1 - ok, -1 - ошибка</returns> 

        [DllImport("libtiff.dll", EntryPoint = "TIFFReadScanline")] 

        private static extern int ReadScanline(IntPtr tiff, IntPtr buf, int row, int sample); 

 

 

        /// <summary> 

        /// Записать порцию данных в файл с использованием сжатия 

        /// </summary> 

        /// <param name="tiff">описатель файла</param> 

        /// <param name="stripNumber">номер стороны изображения</param> 

        /// <param name="data">указатель на обдасть памяти с данными</param> 

        /// <param name="dataSize">размер записываемых данных</param> 

        /// <returns>число записанных данных?</returns> 

        [DllImport("libtiff.dll", EntryPoint = "TIFFWriteEncodedStrip")] 

        private static extern int WriteEncodedStrip(IntPtr tiff, Int16 stripNumber, IntPtr data, int dataSize); 

 

 

        #endregion 

    } 

}



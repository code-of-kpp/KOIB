using System; 
using System.IO; 
using System.Text; 
using ICSharpCode.SharpZipLib.Zip; 
namespace Croc.Bpc.Utils 
{ 
    public static class ZipCompressor 
    { 
        public const int DEFAULT_COMPRESS_LEVEL = 5; 
        public static byte[] Compress(string dataStr) 
        { 
            return Compress(dataStr, "zip", DEFAULT_COMPRESS_LEVEL, string.Empty); 
        } 
        public static byte[] Compress(string dataStr, string archiveName, int compressLevel) 
        { 
            return Compress(dataStr, archiveName, compressLevel, string.Empty); 
        } 
        public static byte[] Compress(string dataStr, string archiveName, int compressLevel, string password) 
        { 
            var enc = Encoding.UTF8; 
            using (var outputMemStream = new MemoryStream()) 
            { 
                int len; 
                using (var outputZipStream = new ZipOutputStream(outputMemStream)) 
                { 
                    outputZipStream.SetLevel(compressLevel); 
                    if (!string.IsNullOrEmpty(password)) 
                    { 
                        outputZipStream.Password = password; 
                    } 
                    var data = enc.GetBytes(dataStr); 
                    var entry = new ZipEntry(archiveName) 
                                    { 
                                        Size = data.Length,  
                                        DateTime = DateTime.Now 
                                    }; 
                    outputZipStream.PutNextEntry(entry); 
                    var buffer = new byte[4096]; 
                    using (var tmpMemStream = new MemoryStream(data)) 
                    { 
                        int readedBytes; 
                        do 
                        { 
                            readedBytes = tmpMemStream.Read(buffer, 0, buffer.Length); 
                            outputZipStream.Write(buffer, 0, readedBytes); 
                        } 
                        while (readedBytes > 0); 
                    } 
                    outputZipStream.CloseEntry(); 
                    outputZipStream.Finish(); 
                    len = (int)outputMemStream.Length; 
                    outputZipStream.Close(); 
                } 
                var zipData = new byte[len]; 
                Array.Copy(outputMemStream.GetBuffer(), zipData, len); 
                return zipData; 
            } 
        } 
        public static Stream Compress(Stream inputStream, string archiveName) 
        { 
            var outputStream = new MemoryStream(); 
            var outputZipStream = new ZipOutputStream(outputStream); 
            outputZipStream.SetLevel(DEFAULT_COMPRESS_LEVEL); 
            var entry = new ZipEntry(archiveName) 
                            { 
                                Size = inputStream.Length, 
                                DateTime = DateTime.Now 
                            }; 
            outputZipStream.PutNextEntry(entry); 
            inputStream.Position = 0; 
            var buffer = new byte[4096]; 
            int readedBytes; 
            do 
            { 
                readedBytes = inputStream.Read(buffer, 0, buffer.Length); 
                outputZipStream.Write(buffer, 0, readedBytes); 
            } while (readedBytes > 0); 
            outputZipStream.CloseEntry(); 
            outputZipStream.Finish(); 
            return outputStream; 
        } 
        public static Stream Uncompress(byte[] zipData) 
        { 
            using (var memStream = new MemoryStream(zipData)) 
            { 
                return Uncompress(memStream); 
            } 
        } 
        public static Stream Uncompress(Stream dataStream) 
        { 
            var inputZipStream = new ZipInputStream(dataStream); 
            try 
            { 
                inputZipStream.GetNextEntry(); 
            } 
            catch (Exception ex) 
            { 
                throw new ArgumentException("Переданная строка не является zip-архивом", ex); 
            } 
            return inputZipStream; 
        } 
    } 
}

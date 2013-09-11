using System; 
using System.IO; 
using Croc.Core; 
namespace Croc.Bpc.Voting 
{ 
    [Serializable] 
    public class SourceDataFileDescriptor 
    { 
        public string FilePath { get; private set; } 
        public long FileSize { get; private set; } 
        public int Uik { get; private set; } 
        public string ScannerSerialNumner { get; private set; } 
        public SourceDataFileDescriptor(string filePath, long fileSize, int uik, string scannerSerialNumner) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(filePath)); 
            CodeContract.Requires(fileSize > 0); 
            CodeContract.Requires(0 < uik && uik <= 9999); 
            CodeContract.Requires(!string.IsNullOrEmpty(scannerSerialNumner)); 
            FilePath = filePath.Replace('\\', '/'); 
            FileSize = fileSize; 
            Uik = uik; 
            ScannerSerialNumner = scannerSerialNumner; 
        } 
        public bool IsPointToSameFile(SourceDataFileDescriptor other) 
        { 
            if (other == null) 
                return false; 
            return 
                string.CompareOrdinal(Path.GetFileName(FilePath), Path.GetFileName(other.FilePath)) == 0 && 
                FileSize == other.FileSize; 
        } 
        public bool IsPointToFile(string filePath, long fileSize) 
        { 
            if (string.IsNullOrEmpty(filePath)) 
                return false; 
            return 
                string.CompareOrdinal(FilePath, filePath.Replace('\\', '/')) == 0 && 
                FileSize == fileSize; 
        } 
        public override string ToString() 
        { 
            return string.Format("FilePath={0}; FileSize={1}; Uik={2}; ScannerSerialNumner={3}", 
                                 FilePath, FileSize, Uik, ScannerSerialNumner); 
        } 
    } 
}

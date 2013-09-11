using System.Collections.Generic; 
namespace Croc.Bpc.Scanner 
{ 
    public interface IScannersInfo 
    { 
        string LocalScannerSerialNumber { get; } 
        string RemoteScannerSerialNumber { get; } 
        List<ScannerInfo> GetScannerInfos(); 
    } 
}

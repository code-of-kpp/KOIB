using Croc.Bpc.Scanner; 
using Croc.Core; 
namespace Croc.Bpc.GsScannerDriver 
{ 
    public class ScannerConnector : Bpc.Scanner.ScannerConnector 
    { 
        protected override IScanner ConnectToScanner() 
        { 
            var scannerManager = CoreApplication.Instance.GetSubsystem<IScannerManager>(); 
            return new Scanner(scannerManager.Logger); 
        } 
    } 
}

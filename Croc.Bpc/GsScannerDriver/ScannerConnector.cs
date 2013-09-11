using System; 

using System.Net; 

using System.Net.Sockets; 

using System.Threading; 

using Croc.Bpc.Common; 

using Croc.Bpc.Scanner; 

using System.Collections.Specialized; 

using Croc.Core; 

 

 

namespace Croc.Bpc.GsScannerDriver 

{ 

	/// <summary> 

	/// ?????? ??????????? ? ?????????? ??????? ? ??????????? ?????? ???????? ? ???? 

	/// </summary> 

    public class ScannerConnector : Croc.Bpc.Scanner.ScannerConnector 

	{ 

        protected override IScanner ConnectToScanner() 

        { 

            var scannerManager = CoreApplication.Instance.GetSubsystem<IScannerManager>(); 

            return new Scanner(scannerManager.Logger); 

        } 

	} 

}



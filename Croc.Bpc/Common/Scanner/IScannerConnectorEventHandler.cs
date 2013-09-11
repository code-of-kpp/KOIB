namespace Croc.Bpc.Scanner 
{ 
    public interface IScannerConnectorEventHandler 
    { 
        void Connected(string serialNumber, string ipAddress); 
        void WantToConnect(string serialNumber, string ipAddress, int scannerStatus); 
        void Disconnected(string serialNumber); 
    } 
}

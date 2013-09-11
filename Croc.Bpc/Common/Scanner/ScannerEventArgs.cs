using System; 
namespace Croc.Bpc.Scanner 
{ 
    public class ScannerEventArgs : EventArgs 
    { 
        public readonly string SerialNumber; 
        public readonly string IpAddress; 
        public ScannerEventArgs(string serialNumber, string ipAddress) 
        { 
            SerialNumber = serialNumber; 
            IpAddress = ipAddress; 
        } 
    } 
}

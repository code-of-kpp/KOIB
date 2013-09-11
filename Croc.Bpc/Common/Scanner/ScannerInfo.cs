using Croc.Core; 
namespace Croc.Bpc.Scanner 
{ 
    public class ScannerInfo 
    { 
        public readonly string SerialNumber; 
        public readonly string IpAddress; 
        public ScannerInfo(ScannerInfo info) 
            : this(info.SerialNumber, info.IpAddress) 
        { 
        } 
        public ScannerInfo(string serialNumber, string ipAddress) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(serialNumber)); 
            SerialNumber = serialNumber; 
            IpAddress = ipAddress ?? string.Empty; 
        } 
        public override bool Equals(object obj) 
        { 
            var other = obj as ScannerInfo; 
            if (other == null) 
                return false; 
            return string.CompareOrdinal(other.SerialNumber, SerialNumber) == 0; 
        } 
        public override int GetHashCode() 
        { 
            return SerialNumber.GetHashCode(); 
        } 
        public override string ToString() 
        { 
            return SerialNumber; 
        } 
    } 
}

using System; 
namespace Croc.Bpc.Scanner 
{ 
    internal class ScannerBroadcastNotice 
    { 
        public const int DATA_LENGTH = 8; 
        public readonly string SerialNumber; 
        public readonly int Status; 
        public readonly byte[] Data; 
        public ScannerBroadcastNotice(string serialNumber, int status) 
        { 
            SerialNumber = serialNumber; 
            Status = status; 
            int sn = int.Parse(SerialNumber); 
            Data = new byte[DATA_LENGTH]; 
            Data[0] = (byte)(sn & 0xFF); 
            Data[1] = (byte)((sn >> 8) & 0xFF); 
            Data[2] = (byte)((sn >> 16) & 0xFF); 
            Data[3] = (byte)((sn >> 24) & 0xFF); 
            Data[4] = (byte)(Status & 0xFF); 
            Data[5] = (byte)((Status >> 8) & 0xFF); 
            Data[6] = (byte)((Status >> 16) & 0xFF); 
            Data[7] = (byte)((Status >> 24) & 0xFF); 
        } 
        public ScannerBroadcastNotice(byte[] data) 
        { 
            Data = data; 
            SerialNumber = (data[0] | data[1] << 8 | data[2] << 16 | data[3] << 24).ToString(); 
            Status = (data[4] | data[5] << 8 | data[6] << 16 | data[7] << 24); 
        } 
    } 
}

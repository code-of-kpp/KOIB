using System; 
namespace Croc.Bpc.Scanner 
{ 
    [Flags] 
    public enum ScannerStatus 
    { 
        OK = 0x0, 
        BAD_VER = 0x1, 
        BAD_CONF = 0x2, 
        BAD_LIGHT = 0x4, 
        BAD_TUNE = 0x8, 
        BAD_LEFT_DOUBLE_LIST = 0x10, 
        BAD_RIGHT_DOUBLE_LIST = 0x20, 
        BAD_VOLT = 0x40, 
    } 
}

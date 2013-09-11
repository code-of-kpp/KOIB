using System; 
namespace Croc.Bpc.Printing.Reports 
{ 
    [Flags] 
    public enum ServiceMode : int 
    { 
        PageBreak = 0x1, 
        ResetPageCounter = 0x2, 
    } 
}

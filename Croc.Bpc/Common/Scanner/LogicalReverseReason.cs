using System; 
namespace Croc.Bpc.Scanner 
{ 
    public enum LogicalReverseReason 
    { 
        SheetReceivingForbidden = 100, 
        InvalidBlankNumber = 101, 
        BlankHasNoCurrentVoteRegime = 102, 
    } 
}

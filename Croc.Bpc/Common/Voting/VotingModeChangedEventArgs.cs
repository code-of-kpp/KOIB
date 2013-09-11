using System; 
namespace Croc.Bpc.Voting 
{ 
    public class VotingModeChangedEventArgs : EventArgs 
    { 
        public readonly VotingMode OldMode; 
        public readonly VotingMode NewMode; 
        public readonly int BulletinCount; 
        public VotingModeChangedEventArgs(VotingMode oldMode, VotingMode newMode, int bulletinCount) 
        { 
            OldMode = oldMode; 
            NewMode = newMode; 
            BulletinCount = bulletinCount; 
        } 
    } 
}

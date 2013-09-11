using System; 
namespace Croc.Core 
{ 
    public class SubsystemStateEventArgs : EventArgs 
    { 
        public readonly ISubsystem Subsystem; 
        public readonly object State; 
        public SubsystemStateEventArgs(ISubsystem subsystem, object state) 
        { 
            CodeContract.Requires(subsystem != null); 
            Subsystem = subsystem; 
            State = state; 
        } 
    } 
}

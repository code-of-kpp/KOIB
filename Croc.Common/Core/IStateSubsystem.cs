using System; 
namespace Croc.Core 
{ 
    public interface IStateSubsystem : ISubsystem 
    { 
        event EventHandler<SubsystemStateEventArgs> StateChanged; 
        void RaiseStateChanged(); 
        void RestoreState(object state); 
        void ResetState(bool raiseStateChangedEvent); 
        object GetState(); 
        SubsystemStateAcceptanceResult AcceptNewState(object newState); 
    } 
}

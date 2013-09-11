using System; 
using Croc.Core.Extensions; 
namespace Croc.Core 
{ 
    public abstract class StateSubsystem : Subsystem, IStateSubsystem 
    { 
        public event EventHandler<SubsystemStateEventArgs> StateChanged; 
        public void RaiseStateChanged() 
        { 
            StateChanged.RaiseEvent(this, new SubsystemStateEventArgs(this, GetState())); 
        } 
        public virtual object GetState() 
        { 
            return null; 
        } 
        public virtual void RestoreState(object state) 
        { 
        } 
        public virtual SubsystemStateAcceptanceResult AcceptNewState(object newState) 
        { 
            return SubsystemStateAcceptanceResult.Accepted; 
        } 
        public void ResetState(bool raiseStateChangedEvent) 
        { 
            ResetStateInternal(); 
            if (raiseStateChangedEvent) 
                RaiseStateChanged(); 
        } 
        protected virtual void ResetStateInternal() 
        { 
        } 
    } 
}

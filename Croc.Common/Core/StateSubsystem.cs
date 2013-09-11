using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Core.Extensions; 

 

 

namespace Croc.Core 

{ 

    /// <summary> 

    /// Базовый класс для подсистем, которые имеют состояние 

    /// </summary> 

    public abstract class StateSubsystem : Subsystem, IStateSubsystem 

    { 

        /// <summary> 

        /// Событие "Состояние подсистемы изменилось" 

        /// </summary> 

        public event EventHandler<SubsystemStateEventArgs> StateChanged; 

 

 

        /// <summary> 

        /// Возбудить событие "Состояние подсистемы изменилось" 

        /// </summary> 

        public void RaiseStateChanged() 

        { 

            StateChanged.RaiseEvent(this, new SubsystemStateEventArgs(Name, GetState())); 

        } 

 

 

        /// <summary> 

        /// Получить состояние подсистемы 

        /// </summary> 

        /// <returns></returns> 

        public virtual object GetState() 

        { 

            return null; 

        } 

 

 

        /// <summary> 

        /// Восстановить состояние 

        /// </summary> 

        /// <param name="state"></param> 

        public virtual void RestoreState(object state) 

        { 

        } 

 

 

        /// <summary> 

        /// Принять новое состояние 


        /// </summary> 

        /// <param name="state">новое состояние</param> 

        /// <returns>результат принятия нового состояния подсистемой</returns> 

        public virtual SubsystemStateAcceptanceResult AcceptNewState(object newState) 

        { 

            return SubsystemStateAcceptanceResult.Accepted; 

        } 

 

 

        /// <summary> 

        /// Сбросить состояние (перевести его в начальное положение) 

        /// </summary> 

        /// <param name="raiseStateChangedEvent"> 

        /// нужно ли возбудить событие "Состояние подсистемы изменилось" 

        /// после того, как состояние будет сброшено</param> 

        public void ResetState(bool raiseStateChangedEvent) 

        { 

            ResetStateInternal(); 

 

 

            if (raiseStateChangedEvent) 

                RaiseStateChanged(); 

        } 

 

 

        /// <summary> 

        /// Сбросить состояние (перевести его в начальное положение) 

        /// </summary> 

        /// <remarks>реализация в классе-наследнике</remarks> 

        protected virtual void ResetStateInternal() 

        { 

        } 

    } 

}



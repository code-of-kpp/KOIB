using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Core 

{ 

    /// <summary> 

    /// Интерфейс подсистемы, которая имеет состояние 

    /// </summary> 

    /// <remarks>используется для того, чтобы пользователь подсистемы умел 

    /// сохранять и восстанавливать ее состояние</remarks> 

    public interface IStateSubsystem : ISubsystem 

    { 

        /// <summary> 

        /// Событие "Состояние подсистемы изменилось" 

        /// </summary> 

        event EventHandler<SubsystemStateEventArgs> StateChanged; 

 

 

        /// <summary> 

        /// Возбудить событие "Состояние подсистемы изменилось" 

        /// </summary> 

        void RaiseStateChanged(); 

 

 

        /// <summary> 

        /// Восстановить состояние 

        /// </summary> 

        /// <param name="state"></param> 

        void RestoreState(object state); 

 

 

        /// <summary> 

        /// Сбросить состояние (перевести его в начальное положение) 

        /// </summary> 

        /// <param name="raiseStateChangedEvent"> 

        /// нужно ли возбудить событие "Состояние подсистемы изменилось" 

        /// после того, как состояние будет сброшено</param> 

        void ResetState(bool raiseStateChangedEvent); 

 

 

        /// <summary> 

        /// Получить состояние подсистемы 

        /// </summary> 

        /// <returns></returns> 

        object GetState(); 

 

 


        /// <summary> 

        /// Принять новое состояние 

        /// </summary> 

        /// <param name="state">новое состояние</param> 

        /// <returns>результат принятия нового состояния подсистемой</returns> 

        SubsystemStateAcceptanceResult AcceptNewState(object newState); 

    } 

}



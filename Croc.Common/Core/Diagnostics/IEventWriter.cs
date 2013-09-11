namespace Croc.Core.Diagnostics 

{ 

    /// <summary> 

    /// Писатель сообщение куда-нибудь 

    /// </summary> 

    public interface IEventWriter : IInitializedType 

    { 

        /// <summary> 

        /// Выводит сообщение 

        /// </summary> 

        /// <param name="uniqueLogId">уникальный идентификатор журнала</param> 

        /// <param name="message">сообщение</param> 

        void Write(string uniqueLogId, string message); 

    } 

}



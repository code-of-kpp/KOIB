using System.Configuration; 

 

 

namespace Croc.Core.Diagnostics 

{ 

    /// <summary> 

    /// Интерфейс фильтрации сообщений 

    /// </summary> 

    public interface IEventFilter : IInitializedType 

    { 

        /// <summary> 

        /// Выполняет проверку возможности записи в протокол 

        /// </summary> 

        /// <param name="logEvent">Событие</param> 

        /// <returns>Признак возможности записи</returns> 

        bool Accepted(LoggerEvent logEvent); 

    } 

}



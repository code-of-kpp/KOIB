namespace Croc.Core.Diagnostics 

{ 

    /// <summary> 

    /// Фильтр, накладываемый непосредственно перед записью 

    /// </summary> 

    public interface IEventWriterFilter : IInitializedType 

    { 

        /// <summary> 

        /// Выполняет проверку возможности записи в протокол 

        /// </summary> 

        /// <param name="writerTriplet">триплет</param> 

        /// <param name="loggerEvent">Событие</param> 

        /// <param name="message">Отформатированное событие</param> 

        /// <returns>Признак возможности записи</returns> 

        bool Accepted(EventWriterTriplet writerTriplet, LoggerEvent loggerEvent, string message); 

    } 

}



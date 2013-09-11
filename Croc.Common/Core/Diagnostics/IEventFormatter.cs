namespace Croc.Core.Diagnostics 

{ 

    /// <summary> 

    /// Интерфейс форматера событий 

    /// </summary> 

    public interface IEventFormatter : IInitializedType 

    { 

        /// <summary> 

        /// Форматирует событие 

        /// </summary> 

        /// <param name="loggerEvent">Событие</param> 

        /// <returns>Текстовое представление события</returns> 

        string Format(LoggerEvent loggerEvent); 

    } 

}



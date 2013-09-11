namespace Croc.Core.Diagnostics 

{ 

    /// <summary> 

    /// Интерфейс для писателей на файловые системы 

    /// </summary> 

    public interface IEventFileSystemWriter : IEventWriter 

    { 

        /// <summary> 

        /// Получает точку файловой системы, в которую пишется протокол 

        /// </summary> 

        /// <param name="uniqueId">Уникальный идентификатор</param> 

        /// <returns>Полный путь</returns> 

        string GetPoint(string uniqueId); 

    } 

}



using System; 

using Croc.Core; 

 

 

namespace Croc.Bpc.Configuration 

{ 

    /// <summary> 

    /// Интерфейс менеджера конфигурации 

    /// </summary> 

    public interface IConfigurationManager : ISubsystem 

    { 

        /// <summary> 

        /// Загрузить рабочую конфигурацию 

        /// </summary> 

        /// <returns> 

        /// true - загрузили или рабочая конфигурация не найдена 

        /// false - ошибка загрузки 

        /// </returns> 

        bool LoadWorkingConfig(); 

 

 

        /// <summary> 

        /// Загрузить частную конфигурацию 

        /// </summary> 

        /// <param name="partialConfigXml">содержимое частного-конфиг файла. 

        /// Если null, то метод сам выполняет поиск и чтение частного-конфиг файла, 

        /// иначе - загрузка частной конфигурации выполняется из данного параметра</param> 

        /// <returns> 

        /// true - загрузили или частная конфигурация не найдена 

        /// false - ошибка загрузки 

        /// </returns> 

        bool LoadPartialConfig(ref string partialConfigXml); 

 

 

        /// <summary> 

        /// Сбросить рабочую конфигурацию в начальное состояние 

        /// </summary> 

        void ResetWorkingConfig(); 

 

 

        /// <summary> 

        /// Применить конфигурацию 

        /// </summary> 

        /// <returns>true - конфигурация успешно применена, false - ошибка применения конфигурации</returns> 

        bool ApplyConfig(); 

    } 

}



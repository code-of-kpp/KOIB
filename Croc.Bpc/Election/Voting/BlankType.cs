using System; 

 

 

namespace Croc.Bpc.Election.Voting 

{ 

    /// <summary> 

    /// Тип бланка 

    /// </summary> 

    public enum BlankType 

    { 

        /// <summary> 

        /// Неизвестно 

        /// </summary> 

        Unknown = -2, 

        /// <summary> 

        /// Тестовый бланк 

        /// </summary> 

        Test = -1, 

        /// <summary> 

        /// Бюллетень действительный 

        /// </summary> 

        Valid = 1, 

        /// <summary> 

        /// Бюллетень недействительный без указания причины 

        /// Может использоваться только в качестве маски агрегата 

        /// </summary> 

        NotValid = 2, 

        /// <summary> 

        /// Бюллетень неустановленной формы 

        /// </summary> 

        Bad = 3, 

        /// <summary> 

        /// Недействительный бюллетень - слишком много отметок 

        /// </summary> 

        TooManyMarks = 4, 

        /// <summary> 

        /// Недействительный бюллетень - нет отметок 

        /// </summary> 

        NoMarks = 5, 

        /// <summary> 

        /// Бланк опущен в сканер в непредусмотренном для него режиме 

        /// </summary> 

        BadMode = 6, 

        /// <summary> 

        /// Все, кроме НУФ 

        /// </summary> 

        AllButBad = 7, 

        /// <summary> 

        /// Все бюллетени (и валидные и невалидные) 

        /// </summary> 


        All = 8, 

        /// <summary> 

        /// Фиктивный тип, только для получения значений из конфигурации 

        /// </summary> 

        BadStamp, 

    } 

}



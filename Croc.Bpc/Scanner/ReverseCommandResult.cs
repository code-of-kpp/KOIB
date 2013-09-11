using System; 

 

 

namespace Croc.Bpc.Scanner 

{ 

    /// <summary> 

    /// Результат отправки команды "Реверсировать лист" сканеру 

    /// </summary> 

    public enum ReverseCommandResult 

    { 

        /// <summary> 

        /// Команда принята к исполнению 

        /// </summary> 

        Accepted, 

        /// <summary> 

        /// Выполнение реверса невозможно 

        /// </summary> 

        Impossible 

    } 

}



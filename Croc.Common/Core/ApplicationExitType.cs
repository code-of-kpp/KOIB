using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Core 

{ 

    /// <summary> 

    /// Тип выхода из приложения 

    /// </summary> 

    public enum ApplicationExitType : int 

    { 

        /// <summary> 

        /// Просто завершить работу приложения 

        /// </summary> 

        Exit = 0, 

        /// <summary> 

        /// Завершить работу приложения и потом запустить его заново 

        /// </summary> 

        RestartApplication = 1, 

        /// <summary> 

        /// Завершить работу приложения и потом перезагрузить ОС 

        /// </summary> 

        RebootOperationSystem = 2, 

        /// <summary> 

        /// Выключить сканер 

        /// </summary> 

        PowerOff = 3, 

		/// <summary> 

		/// Перезапуск драйвера, а затем приложения 

		/// </summary> 

		RestartDriverAndApplication = 4 

    } 

}



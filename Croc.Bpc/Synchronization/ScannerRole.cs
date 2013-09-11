using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Bpc.Synchronization 

{ 

    /// <summary> 

    /// Роль сканера 

    /// </summary> 

    public enum ScannerRole 

    { 

        /// <summary> 

        /// Не определена 

        /// </summary> 

        Undefined, 

        /// <summary> 

        /// Главный сканер 

        /// </summary> 

        Master, 

        /// <summary> 

        /// Подчиненный сканер 

        /// </summary> 

        Slave 

    } 

}



using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Core 

{ 

    /// <summary> 

    /// Результат принятия подсистемой нового состояния 

    /// </summary> 

    public enum SubsystemStateAcceptanceResult 

    { 

        /// <summary> 

        /// Принято 

        /// </summary> 

        Accepted = 0, 

 

 

        /// <summary> 

        /// Принято путем слияния нового состояния и текущего 

        /// </summary> 

        /// <remarks>Это означает, что в результате принятия нового состояния,  

        /// получилось еще более новое состояние, которое и приняла подсистема</remarks> 

        AcceptedByMerge, 

 

 

        /// <summary> 

        /// Отклонено 

        /// </summary> 

        /// <remarks>Это означает, что текущее состояние подсистемы признано более новым, 

        /// чем то, которое ей передали</remarks> 

        Rejected, 

    } 

}



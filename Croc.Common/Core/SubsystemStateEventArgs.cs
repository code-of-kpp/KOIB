using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Core 

{ 

    /// <summary> 

    /// Аргументы события состояния подсистемы 

    /// </summary> 

    public class SubsystemStateEventArgs : EventArgs 

    { 

        /// <summary> 

        /// Имя подсистемы, к состоянию которого относится событие 

        /// </summary> 

        public string SubsystemName 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Состояние 

        /// </summary> 

        public object State 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="subsystemName">Имя подсистемы, к состоянию которого относится событие</param> 

        public SubsystemStateEventArgs(string subsystemName, object state) 

        { 

            CodeContract.Requires(!string.IsNullOrEmpty(subsystemName)); 

 

 

            SubsystemName = subsystemName; 

            State = state; 

        } 

    } 

}



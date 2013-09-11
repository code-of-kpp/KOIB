using System.Collections.Generic; 

 

 

namespace Croc.Core.Diagnostics 

{ 

    /// <summary> 

    /// Тройка: фильтры-форматер-писатель 

    /// </summary> 

    public class EventWriterTriplet 

    { 

        /// <summary> 

        /// писатель 

        /// </summary> 

        public IEventWriter Writer = null; 

        /// <summary> 

        /// ассоциированный с ним форматтер 

        /// </summary> 

        public IEventFormatter Formatter = new Default.EventFormatter(); 

        /// <summary> 

        /// Признак вывода неформатированных сообщений 

        /// </summary> 

        public bool Raw; 

        /// <summary> 

        /// ассоциированные с ним фильтры 

        /// </summary> 

        public List<IEventWriterFilter> Filters = new List<IEventWriterFilter>(); 

    } 

}



using System; 

using System.Configuration; 

using System.Diagnostics; 

 

 

namespace Croc.Core.Diagnostics 

{ 

    /// <summary> 

    /// Фильтр событий по их уровню 

    /// </summary> 

    public class TraceLevelFilter : IEventFilter 

    { 

        /// <summary> 

        /// Маска типов событий, которые не будут отфильтровываться 

        /// </summary> 

        private int _allowedTraceEventTypeMask; 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="traceLevel">Маска уровней фильтрации</param> 

        public TraceLevelFilter(TraceLevel traceLevel) 

        { 

            for (int i = 0; i <= (int)traceLevel; ++i) 

                _allowedTraceEventTypeMask |= 1 << i; 

        } 

 

 

        public bool Accepted(LoggerEvent logEvent) 

        { 

            return ((int) logEvent.EventType & _allowedTraceEventTypeMask) > 0; 

        } 

 

 

        public void Init(NameValueConfigurationCollection props) 

        { 

            // ничего не делаем 

        } 

    } 

}



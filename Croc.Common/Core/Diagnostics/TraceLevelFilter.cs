using System.Configuration; 
using System.Diagnostics; 
namespace Croc.Core.Diagnostics 
{ 
    public class TraceLevelFilter : IEventFilter 
    { 
        private readonly int _allowedTraceEventTypeMask; 
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
        } 
    } 
}

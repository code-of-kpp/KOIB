namespace Croc.Core.Diagnostics 
{ 
    internal class NullLogger : ILogger 
    { 
        public void Log(LoggerEvent logEvent) 
        { 
        } 
        public bool IsAcceptedByEventType(LoggerEvent logEvent) 
        { 
            return false; 
        } 
    } 
}

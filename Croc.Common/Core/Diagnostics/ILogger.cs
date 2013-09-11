namespace Croc.Core.Diagnostics 
{ 
    public interface ILogger 
    { 
        void Log(LoggerEvent logEvent); 
        bool IsAcceptedByEventType(LoggerEvent logEvent); 
    } 
}

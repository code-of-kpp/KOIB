namespace Croc.Core.Diagnostics 
{ 
    public interface IEventFilter : IInitializedType 
    { 
        bool Accepted(LoggerEvent logEvent); 
    } 
}

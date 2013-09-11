namespace Croc.Core.Diagnostics 
{ 
    public interface IEventFormatter : IInitializedType 
    { 
        string Format(LoggerEvent loggerEvent); 
    } 
}

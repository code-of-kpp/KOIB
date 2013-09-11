namespace Croc.Core.Diagnostics 
{ 
    public interface IEventWriterFilter : IInitializedType 
    { 
        bool Accepted(EventWriterTriplet writerTriplet, LoggerEvent loggerEvent, string message); 
    } 
}

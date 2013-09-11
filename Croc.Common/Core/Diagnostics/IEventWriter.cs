namespace Croc.Core.Diagnostics 
{ 
    public interface IEventWriter : IInitializedType 
    { 
        void Write(string uniqueLogId, string message); 
    } 
}

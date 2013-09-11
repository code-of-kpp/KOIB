namespace Croc.Core.Diagnostics 
{ 
    public interface IEventFileSystemWriter : IEventWriter 
    { 
        string GetPoint(string uniqueId); 
    } 
}

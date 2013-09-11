using System; 
namespace Croc.Bpc.Sound 
{ 
    internal interface ISoundPlayer : IDisposable 
    { 
        string FileExt { get; } 
        void Play(string soundFilePath); 
        void Stop(); 
        event EventHandler PlayingStopped; 
    } 
}

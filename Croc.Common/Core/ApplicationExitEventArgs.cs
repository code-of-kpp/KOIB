using System; 
namespace Croc.Core 
{ 
    public class ApplicationExitEventArgs : EventArgs 
    { 
        public readonly ApplicationExitType ExitType; 
        public ApplicationExitEventArgs(ApplicationExitType type) 
        { 
            ExitType = type; 
        } 
    } 
}

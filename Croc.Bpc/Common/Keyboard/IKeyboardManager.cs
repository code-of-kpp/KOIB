using System; 
using Croc.Core; 
namespace Croc.Bpc.Keyboard 
{ 
    public interface IKeyboardManager : ISubsystem 
    { 
        event EventHandler<KeyEventArgs> KeyPressed; 
    } 
}

using System; 
using Croc.Bpc.Keyboard.Config; 
using Croc.Core.Diagnostics; 
namespace Croc.Bpc.Keyboard 
{ 
    public interface IKeyboardDriver : IDisposable 
    { 
        void Init(KeyboardDriverConfig config, ILogger logger); 
        event EventHandler<DriverKeyPressedEventArgs> KeyPressed; 
        void Start(); 
    } 
}

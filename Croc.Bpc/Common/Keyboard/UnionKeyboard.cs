using System; 
using Croc.Core; 
using Croc.Core.Configuration; 
using Croc.Core.Extensions; 
namespace Croc.Bpc.Keyboard 
{ 
    public sealed class UnionKeyboard : Subsystem, IKeyboardManager 
    { 
        public event EventHandler<KeyEventArgs> KeyPressed; 
        public override void Init(SubsystemConfig config) 
        { 
            var keyboards = Application.GetSubsystems<IKeyboardManager>(); 
            foreach (var item in keyboards) 
            { 
                var keyboard = item.Value; 
                if (keyboard is UnionKeyboard) 
                    continue; 
                keyboard.KeyPressed += KeyboardKeyPressed; 
            } 
        } 
        private void KeyboardKeyPressed(object sender, KeyEventArgs e) 
        { 
            KeyPressed.RaiseEvent(sender, e); 
        } 
    } 
}

using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Core; 

using Croc.Core.Extensions; 

using Croc.Core.Configuration; 

 

 

namespace Croc.Bpc.Keyboard 

{ 

    /// <summary> 

    /// Объединенная клавиатура - аккумулирует события со всех клавиатур 

    /// </summary> 

    public sealed class UnionKeyboard : Subsystem, IKeyboard 

    { 

        /// <summary> 

        /// Событие "Нажата клавиша" 

        /// </summary> 

        public event EventHandler<KeyEventArgs> KeyPressed; 

 

 

        /// <summary> 

        /// Инициализация 

        /// </summary> 

        /// <param name="config"></param> 

        public override void Init(SubsystemConfig config) 

        { 

            var keyboards = Application.GetSubsystems<IKeyboard>(); 

            foreach (var item in keyboards) 

            { 

                var keyboard = item.Value; 

 

 

                if (keyboard is UnionKeyboard) 

                    continue; 

 

 

                keyboard.KeyPressed += new EventHandler<KeyEventArgs>(KeyboardKeyPressed); 

            } 

        } 

 

 

        /// <summary> 

        /// Обработчик события нажатия клавиши 

        /// </summary> 

        /// <param name="sender"></param> 

        /// <param name="e"></param> 

        private void KeyboardKeyPressed(object sender, KeyEventArgs e) 

        { 


            KeyPressed.RaiseEvent(sender, e); 

        } 

    } 

}



using System; 

using Croc.Core; 

 

 

namespace Croc.Bpc.Keyboard 

{ 

    /// <summary> 

    /// Интерфейс подсистемы управления клавиатурой и др. устройствами, 

    /// который принимают информацию от пользователя через нажатия клавишь/кнопок 

    /// </summary> 

    public interface IKeyboard : ISubsystem 

    { 

        /// <summary> 

        /// Событие "Нажата клавиша" 

        /// </summary> 

        event EventHandler<KeyEventArgs> KeyPressed; 

    } 

}



using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Bpc.Keyboard 

{ 

    /// <summary> 

    /// Типы клавиш 

    /// </summary> 

    public enum KeyType 

    { 

        /// <summary> 

        /// Неизвестная клавиша 

        /// </summary> 

        Unknown, 

        /// <summary> 

        /// Да 

        /// </summary> 

        Yes, 

        /// <summary> 

        /// Нет 

        /// </summary> 

        No, 

        /// <summary> 

        /// Возврат 

        /// </summary> 

        GoBack, 

        /// <summary> 

        /// Помощь 

        /// </summary> 

        Help, 

        /// <summary> 

        /// Меню 

        /// </summary> 

        Menu, 

        /// <summary> 

        /// Цифра (от 0 до 9) 

        /// </summary> 

        Digit, 

        /// <summary> 

        /// Удаление 

        /// </summary> 

        Delete, 

        /// <summary> 

        /// Выход 

        /// </summary> 

        Quit, 

        /// <summary> 


        /// Выключение питания 

        /// </summary> 

        PowerOff, 

        /// <summary> 

        /// Вброс бюллетеня 

        /// </summary> 

        /// <remarks>используется в режиме эмуляции сканера</remarks> 

        Drop, 

    } 

}



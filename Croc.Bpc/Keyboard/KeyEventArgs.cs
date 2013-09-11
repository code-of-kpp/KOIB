using System; 

 

 

namespace Croc.Bpc.Keyboard 

{ 

    /// <summary> 

    /// Аргументы события, относящегося к действию с клавишей клавиатуры 

    /// </summary> 

    /// <remarks>действие - нажатие клавиши</remarks> 

    public class KeyEventArgs : EventArgs 

    { 

        /// <summary> 

        /// Код клавиши в аппаратной кодировке 

        /// </summary> 

        public readonly int ScanCode; 

        /// <summary> 

        /// Тип клавиши 

        /// </summary> 

        public readonly KeyType Type; 

        /// <summary> 

        /// Значение, поставленное в соответствие с клавишей 

        /// </summary> 

        public readonly int Value; 

        /// <summary> 

        /// Время действия, выполненного с клавишей (в тиках таймера) 

        /// </summary> 

        public readonly int TimeStamp; 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="scanCode"></param> 

        /// <param name="type"></param> 

        /// <param name="value"></param> 

        /// <param name="timeStamp"></param> 

        public KeyEventArgs(int scanCode, KeyType type, int value, int timeStamp) 

        { 

            ScanCode = scanCode; 

            Type = type; 

            Value = value; 

            TimeStamp = timeStamp; 

        } 

    } 

}



using System; 

using System.Runtime.InteropServices; 

using System.Threading; 

 

 

namespace Croc.Bpc.Keyboard 

{ 

    /// <summary> 

    /// Класс обработки кнопок КОИБ-2010 

    /// </summary> 

    public class ButtonsDevice : BaseKeyboard 

    { 

        /// <summary> 

        /// Флаги кнопок 

        /// </summary> 

        [Flags] 

        private enum Buttons : byte 

        { 

            /// <summary> 

            /// Выключение 

            /// </summary> 

            PowerOff = 0x1, 

            /// <summary> 

            /// Нет 

            /// </summary> 

            No = 0x2, 

            /// <summary> 

            /// Да 

            /// </summary> 

            Yes = 0x4, 

            /// <summary> 

            /// Помощь 

            /// </summary> 

            Help = 0x8, 

        } 

 

 

        /// <summary> 

        /// Есть данные, готовые для обработки 

        /// </summary> 

        private const int DATA_READY = 1; 

        /// <summary> 

        /// Нет данных, пригодных для обработки 

        /// </summary> 

        private const int DATA_NOT_READY = 0;        

 

 

		/// <summary> 

        /// Метод потока асинхронного чтения кнопок 

		/// </summary> 


        protected override void WatcherMethod() 

		{ 

            while (!_disposed) 

            { 

                byte mask = 0; 

                int timeStamp = 0; 

 

 

                if (GetButtons(ref mask, ref timeStamp) == DATA_READY && !_disposed) 

                { 

                    foreach (var button in Enum.GetValues(typeof(Buttons))) 

                    { 

                        if ((mask & (byte)button) != 0) 

                        { 

                            OnNewDataReady((byte)button, timeStamp); 

                        } 

                    } 

                } 

 

 

                // заснем на 200 мкс, чтобы не прочитать несколько раз один и тот же код 

                Thread.Sleep(200); 

            } 

		} 

 

 

        /// <summary> 

        /// Получаем события о нажатых кнопках (в общем случае могут быть нажаты все кнопки) 

        /// </summary> 

        /// <param name="mask">Битовая маска нажатых кнопок</param> 

        /// <param name="time">Время нажатия</param> 

        [DllImport("ButtonsProvider.dll")] 

        public static extern int GetButtons(ref byte mask, ref int time); 

    } 

}



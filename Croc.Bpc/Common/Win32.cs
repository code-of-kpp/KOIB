using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Runtime.InteropServices; 

 

 

namespace Croc.Bpc.Common 

{ 

    /// <summary> 

    /// Обертка для доступа к системным функциям Windows 

    /// </summary> 

    public static class Win32 

    { 

        /// <summary> 

        /// Структура, содержащая значение времени 

        /// </summary> 

        [StructLayout(LayoutKind.Sequential)] 

        public struct SystemTime 

        { 

            /// <summary> 

            /// Год 

            /// </summary> 

            public ushort Year; 

            /// <summary> 

            /// Месяц 

            /// </summary> 

            public ushort Month; 

            /// <summary> 

            /// День недели 

            /// </summary> 

            public ushort DayOfWeek; 

            /// <summary> 

            /// День 

            /// </summary> 

            public ushort Day; 

            /// <summary> 

            /// Час 

            /// </summary> 

            public ushort Hour; 

            /// <summary> 

            /// Минута 

            /// </summary> 

            public ushort Minute; 

            /// <summary> 

            /// Секунда 

            /// </summary> 

            public ushort Second; 

            /// <summary> 

            /// Миллисекунда 


            /// </summary> 

            public ushort Milliseconds; 

        }; 

 

 

        /// <summary> 

        /// Функция Windows API для установки системного времени 

        /// </summary> 

        /// <param name="st">Объект структуры времени</param> 

        /// <returns>true - если время установлено, false - в ином случае </returns> 

        [DllImport("Kernel32")] 

        public static extern bool SetSystemTime(ref SystemTime st); 

    } 

}



using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Core.Extensions 

{ 

    /// <summary> 

    /// Расширения для преобразования типов 

    /// </summary> 

    public static class ConvertExtensions 

    { 

        /// <summary> 

        /// Преобразование значения типа Bool к Int32 

        /// </summary> 

        /// <param name="value"></param> 

        /// <returns>== true => 1, == false => 0</returns> 

        public static int ToInt(this bool value) 

        { 

            return value ? 1 : 0; 

        } 

 

 

        /// <summary> 

        /// Преобразование значения типа Int32 к Bool 

        /// </summary> 

        /// <param name="value"></param> 

        /// <returns>== 0 => false, != 0 => true</returns> 

        public static bool ToBool(this int value) 

        { 

            return value == 0 ? false : true; 

        } 

    } 

}



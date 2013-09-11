using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Core.Extensions 

{ 

    /// <summary> 

    /// Вспомогательный класс для хранения тройки значений 

    /// </summary> 

    public class Triplet<T_First, T_Second, T_Third> 

    { 

        /// <summary> 

        /// Первое значение 

        /// </summary> 

        public T_First First 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Второе значение 

        /// </summary> 

        public T_Second Second 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Третье значение 

        /// </summary> 

        public T_Third Third 

        { 

            get; 

            private set; 

        } 

 

 

        public Triplet(T_First first, T_Second second, T_Third third) 

        { 

            First = first; 

            Second = second; 

            Third = third; 

        } 

    } 


}



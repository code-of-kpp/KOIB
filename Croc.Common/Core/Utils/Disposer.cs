using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Core.Utils 

{ 

    /// <summary> 

    /// Класс-помошник в удалении объектов 

    /// </summary> 

    public class Disposer 

    { 

        /// <summary> 

        /// Если объект реализует IDisposable, то вызывает для него метод Dispose 

        /// </summary> 

        /// <param name="obj"></param> 

        public static void DisposeObject(object obj) 

        { 

            if (obj != null && obj is IDisposable) 

                ((IDisposable)obj).Dispose(); 

        } 

    } 

}



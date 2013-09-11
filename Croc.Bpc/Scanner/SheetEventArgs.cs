using System; 

 

 

namespace Croc.Bpc.Scanner 

{ 

    /// <summary> 

    /// Аргументы событий о листе 

    /// </summary> 

    public class SheetEventArgs : EventArgs 

    { 

        /// <summary> 

        /// Сессия обработки листа 

        /// </summary> 

        public readonly SheetProcessingSession SheetProcessingSession; 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        public SheetEventArgs(SheetProcessingSession session) 

        { 

            SheetProcessingSession = session; 

        } 

    } 

}



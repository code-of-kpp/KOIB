using System; 

using System.Collections.Generic; 

using System.Text; 

 

 

namespace Croc.Bpc.Printing.Reports 

{ 

    /// <summary> 

    /// Сервисная строка, должна пропускаться при печати 

    /// Служит для хранения начального номера строки 

    /// </summary> 

    public class ServiceLine : IReportElement 

    { 

        /// <summary> 

        /// Номер текущей строки 

        /// </summary> 

        public int CurrentRow; 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="rowNumber">Номер строки</param> 

        public ServiceLine(int rowNumber) 

        { 

            CurrentRow = rowNumber; 

        } 

 

 

        /// <summary> 

        /// Признак того, что элемент выводится на печать 

        /// </summary> 

        public bool IsPrintable 

        { 

            get { return false; } 

        } 

    } 

}



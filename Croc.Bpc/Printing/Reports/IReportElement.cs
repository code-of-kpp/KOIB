using System; 

using System.Collections.Generic; 

using System.Text; 

 

 

namespace Croc.Bpc.Printing.Reports 

{ 

    /// <summary> 

    /// Интерфейс - элемент отчета 

    /// </summary> 

    public interface IReportElement 

    { 

        /// <summary> 

        /// Признак того, что элемент выводится на печать 

        /// </summary> 

        bool IsPrintable { get; } 

    } 

}



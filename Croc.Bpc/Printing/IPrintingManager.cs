using System; 

using System.Collections.Generic; 

using System.Collections.Specialized; 

using System.Linq; 

using System.Text; 

using Croc.Bpc.Printing.Config; 

using Croc.Bpc.Printing.Reports; 

using Croc.Core; 

 

 

namespace Croc.Bpc.Printing 

{ 

    /// <summary> 

    /// Интерфейс менеджера печати 

    /// </summary> 

    public interface IPrintingManager : ISubsystem 

    { 

        /// <summary> 

        /// Конфиг отчетов 

        /// </summary> 

        ReportConfig ReportConfig { get; } 

 

 

        /// <summary> 

        /// Найти принтер и проверить, что он готов к работе 

        /// </summary> 

        /// <returns>true - принтер найден, false - принтер не найден</returns> 

        bool FindPrinter(); 

 

 

        /// <summary> 

        /// Печать отчета 

        /// </summary> 

        /// <param name="reportType">тип отчета</param> 

        /// <returns>true - печать выполнена, false - ошибка печати</returns> 

        bool PrintReport(ReportType reportType, ListDictionary reportParameters); 

    } 

}



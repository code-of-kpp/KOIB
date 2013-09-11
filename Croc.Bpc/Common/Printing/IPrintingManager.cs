using System; 
using System.Collections.Specialized; 
using Croc.Core; 
namespace Croc.Bpc.Printing 
{ 
    public interface IPrintingManager : ISubsystem 
    { 
        bool FindPrinter(); 
        PrinterJob CreateReport(ReportType reportType, ListDictionary reportParameters, int copies); 
        event EventHandler<PrintReportEventArgs> PrintReportStarting; 
        event EventHandler<PrintReportEventArgs> PrintReportFinished; 
        bool PrintReport(PrinterJob printerJob); 
    } 
}

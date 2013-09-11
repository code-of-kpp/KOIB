using System; 
using Croc.Core; 
namespace Croc.Bpc.Printing 
{ 
    public class PrintReportEventArgs : EventArgs 
    { 
        public readonly PrinterJob Job; 
        public PrintReportEventArgs(PrinterJob printerJob) 
        { 
            CodeContract.Requires(printerJob != null); 
            Job = printerJob; 
        } 
    } 
}

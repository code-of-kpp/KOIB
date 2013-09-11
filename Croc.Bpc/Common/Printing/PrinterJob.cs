using System; 
using Croc.Core; 
namespace Croc.Bpc.Printing 
{ 
    [Serializable] 
    public class PrinterJob 
    { 
        public string FilePath { get; private set; } 
        public int PageCont { get; private set; } 
        public int Copies { get; set; } 
        public ReportType ReportType { get; private set; } 
        public PrinterJob(ReportType reportType, string filePath, int pageCount, int сopies) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(filePath)); 
            CodeContract.Requires(pageCount >= 0); 
            CodeContract.Requires(сopies >= 0); 
            ReportType = reportType; 
            FilePath = filePath; 
            PageCont = pageCount; 
            Copies = сopies; 
        } 
    } 
}

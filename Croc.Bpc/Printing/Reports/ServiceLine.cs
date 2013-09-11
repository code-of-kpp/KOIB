namespace Croc.Bpc.Printing.Reports 
{ 
    public class ServiceLine : IReportElement 
    { 
        public int CurrentRow; 
        public ServiceLine(int rowNumber) 
        { 
            CurrentRow = rowNumber; 
        } 
        public bool IsPrintable 
        { 
            get { return false; } 
        } 
    } 
}

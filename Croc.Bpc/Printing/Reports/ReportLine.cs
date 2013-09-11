using System; 
namespace Croc.Bpc.Printing.Reports 
{ 
    public class ReportLine : IReportElement 
    { 
        public bool IsPrintable 
        { 
            get { return true; } 
        } 
        private readonly string[] _lines; 
        private string _relativeFontSize; 
        public string[] Lines 
        { 
            get 
            { 
                return _lines; 
            } 
        } 
        public string FirstLine 
        { 
            get 
            { 
                if (_lines == null || _lines[0] == null) 
                    return " "; 
                return _lines[0]; 
            } 
        } 
        public bool Bold { get; set; } 
        public bool Italic { get; set; } 
        public int FontSize(int baseFontSize) 
        { 
            int fontSize = baseFontSize; 
            if (_relativeFontSize != null && _relativeFontSize.Trim().Length > 0) 
            { 
                int size = Convert.ToInt32(_relativeFontSize.Trim()); 
                if (size < 0 || _relativeFontSize.Trim()[0] == '+') 
                { 
                    fontSize += size; 
                } 
                else 
                { 
                    fontSize = size; 
                } 
            } 
            return fontSize; 
        } 
        public ServiceMode Mode { get; set; } 
        public LineAlign Align { get; set; } 
        public int IsLineDotted { get; set; } 
        public ReportLine(string[] lines, 
            LineAlign align, 
            string fontSize, 
            bool bold, 
            bool italic, 
            bool newPage, 
            bool resetPageNumber, 
            int isLineDotted) 
        { 
            _lines = lines; 
            Align = align; 
            _relativeFontSize = fontSize; 
            Bold = bold; 
            Italic = italic; 
            IsLineDotted = isLineDotted; 
            if (newPage) 
            { 
                Mode |= ServiceMode.PageBreak; 
            } 
            if (resetPageNumber) 
            { 
                Mode |= ServiceMode.ResetPageCounter; 
            } 
        } 
        public delegate string DoTransform(string str); 
        public void TransformLine(DoTransform transform) 
        { 
            for (var i = 0; i < _lines.Length; i++) 
            { 
                _lines[i] = transform(_lines[i]); 
            } 
        } 
    } 
}

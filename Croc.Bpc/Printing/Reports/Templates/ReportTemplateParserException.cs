using System; 
namespace Croc.Bpc.Printing.Reports.Templates 
{ 
    public class ReportTemplateParserException : Exception 
    { 
        private Type _type; 
        public Type Type 
        { 
            get { return _type; } 
        } 
        private ParseExceptionReason _reason; 
        public ParseExceptionReason Reason 
        { 
            get { return _reason; } 
        } 
        private string _name; 
        public string Name 
        { 
            get { return _name; } 
        } 
        #region Конструкторы 


        public ReportTemplateParserException() : base() { } 
        public ReportTemplateParserException(ParseExceptionReason reason, string name, Type type) 
            : base(reason.ToString()) 
        { 
            _reason = reason; 
            _name = name; 
            _type = type; 
        } 


        public ReportTemplateParserException( 
            ParseExceptionReason reason 
            , string name 
            , Type type 
            , System.Exception innerException)  
            : base(reason.ToString(), innerException) 
        { 
            _reason = reason; 
            _name = name; 
            _type = type; 
        } 
        #endregion 
    } 
}

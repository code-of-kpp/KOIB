using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Bpc.Printing.Reports.Templates 

{ 

    /// <summary> 

    /// Ошибка разбора переменной 

    /// </summary> 

    public class ReportTemplateParserException : Exception 

    { 

        /// <summary> 

        /// Причина ошибки 

        /// </summary> 

        public enum ExceptionReason 

        { 

            /// <summary> 

            /// Переменная не найдена 

            /// </summary> 

            NotFound, 

            /// <summary> 

            /// Произошло исключение при разборе  

            /// </summary> 

            Failure, 

            /// <summary> 

            /// Повторное определение переменной цикла 

            /// </summary> 

            AmbigiousFor, 

        } 

        private Type m_type; 

        public Type Type 

        { 

            get { return m_type; } 

        } 

        private ExceptionReason m_reason; 

        public ExceptionReason Reason 

        { 

            get { return m_reason; } 

        } 

        private string m_name; 

        public string Name 

        { 

            get { return m_name; } 

        } 

 

 

        public ReportTemplateParserException() : base() { } 

        public ReportTemplateParserException(ExceptionReason reason, string name, Type type) 


            : base(reason.ToString()) 

        { 

            m_reason = reason; 

            m_name = name; 

            m_type = type; 

        } 

        public ReportTemplateParserException(ExceptionReason reason, string name, Type type, System.Exception innerException) : 

            base(reason.ToString(), innerException) 

        { 

            m_reason = reason; 

            m_name = name; 

            m_type = type; 

        } 

    } 

}



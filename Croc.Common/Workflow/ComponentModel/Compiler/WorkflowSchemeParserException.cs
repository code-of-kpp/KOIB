using System; 

 

 

namespace Croc.Workflow.ComponentModel.Compiler 

{ 

    /// <summary> 

    /// Ошибка разбора схемы потока работ 

    /// </summary> 

    public class WorkflowSchemeParserException : Exception 

    { 

        public WorkflowSchemeParserException(string message) 

            : base(message) 

        { 

        } 

 

 

        public WorkflowSchemeParserException(string message, Exception innerEx) 

            : base(message, innerEx) 

        { 

        } 

 

 

        public WorkflowSchemeParserException(string message, WorkflowSchemeParser parser) 

            : base(FormatMessage(message, parser)) 

        { 

        } 

 

 

        public WorkflowSchemeParserException(string message, Exception innerEx, WorkflowSchemeParser parser) 

            : base(FormatMessage(message, parser), innerEx) 

        { 

        } 

 

 

        private static string FormatMessage(string message, WorkflowSchemeParser parser) 

        { 

            return parser.ReadDone 

                ? message 

                : string.Format("[{0}, стр {1}, поз {2}] {3}",  

                    parser.FileName, parser.LineNumber, parser.LinePosition, message); 

        } 

    } 

}



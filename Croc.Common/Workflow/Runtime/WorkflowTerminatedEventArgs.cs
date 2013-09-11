using System; 

 

 

namespace Croc.Workflow.Runtime 

{ 

    public class WorkflowTerminatedEventArgs : WorkflowEventArgs 

    { 

        /// <summary> 

        /// Исключение, которое послужило причиной для прерывания выполнения экземпляра потока работ 

        /// </summary> 

        public Exception Exception 

        { 

            get; 

            private set; 

        } 

 

 

        public WorkflowTerminatedEventArgs(WorkflowInstance wi, Exception ex) 

            : base(wi) 

        { 

            Exception = ex; 

        } 

    } 

}



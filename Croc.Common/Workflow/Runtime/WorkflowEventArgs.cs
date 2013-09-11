using System; 

 

 

namespace Croc.Workflow.Runtime 

{ 

    /// <summary> 

    /// Аргументы события потока работ 

    /// </summary> 

    public class WorkflowEventArgs : EventArgs 

    { 

        /// <summary> 

        /// Экземпляр потока работ 

        /// </summary> 

        public WorkflowInstance WorkflowInstance 

        { 

            get; 

            private set; 

        } 

 

 

        public WorkflowEventArgs(WorkflowInstance wi) 

        { 

            WorkflowInstance = wi; 

        } 

    } 

}



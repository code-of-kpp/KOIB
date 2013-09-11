using System; 

 

 

namespace Croc.Workflow.Runtime 

{ 

    public class WorkflowCompletedEventArgs : WorkflowEventArgs 

    { 

        public object Result 

        { 

            get; 

            private set; 

        } 

 

 

        public WorkflowCompletedEventArgs(WorkflowInstance wi, object result) 

            : base(wi) 

        { 

            Result = result; 

        } 

    } 

}



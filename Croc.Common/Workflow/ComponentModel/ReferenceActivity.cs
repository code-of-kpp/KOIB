using System; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    public class ReferenceActivity : Activity 
    { 
        public Activity ActivityForExecute 
        { 
            get; 
            set; 
        } 
        public ReferenceActivity() 
        { 
            ExecutionMethodCaller = new ActivityExecutionMethodCaller("ExecuteReferencedActivity", this); 
        } 
        internal NextActivityKey ExecuteReferencedActivity( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return ActivityForExecute.Execute(context, parameters); 
        } 
    } 
}

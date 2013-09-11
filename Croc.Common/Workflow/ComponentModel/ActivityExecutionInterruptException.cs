namespace Croc.Workflow.ComponentModel 
{ 
    public sealed class ActivityExecutionInterruptException : ActivityExecutionException 
    { 
        public ActivityExecutionInterruptException(Activity activity, WorkflowExecutionContext context) 
            : base("Прерывание выполнения действия", activity, context) 
        { 
        } 
    } 
}

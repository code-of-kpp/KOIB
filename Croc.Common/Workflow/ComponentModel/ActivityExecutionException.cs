using System; 
namespace Croc.Workflow.ComponentModel 
{ 
    public class ActivityExecutionException : Exception 
    { 
        public readonly Activity Activity; 
        public ActivityExecutionException(string message, Activity activity, WorkflowExecutionContext context) 
            : this(message, null, activity, context) 
        { 
        } 
        public ActivityExecutionException( 
            string message, Exception innerEx, Activity activity, WorkflowExecutionContext context) 
            : base(FormatMessage(message, activity, context), innerEx) 
        { 
            Activity = activity; 
        } 
        private static string FormatMessage(string message, Activity activity, WorkflowExecutionContext context) 
        { 
            return string.Format("[действие: {0}; контекст: {1}] {2}", activity, context, message); 
        } 
    } 
}

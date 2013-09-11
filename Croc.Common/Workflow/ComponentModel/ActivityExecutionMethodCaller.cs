using System; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    internal class ActivityExecutionMethodCaller : MethodCaller 
    { 
        public ActivityExecutionMethodCaller(string methodName, object methodOwner) 
            : base( 
                typeof(Func<WorkflowExecutionContext, ActivityParameterDictionary, NextActivityKey>), 
                methodName, 
                methodOwner) 
        { 
        } 
        public NextActivityKey Call(WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return (NextActivityKey)Call(new object[] { context, parameters }); 
        } 
    } 
}

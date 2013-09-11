using System; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    internal class ActivityUnInitializeMethodCaller : MethodCaller 
    { 
        public ActivityUnInitializeMethodCaller(string methodName, object methodOwner) 
            : base(typeof(Action<WorkflowExecutionContext>), methodName, methodOwner) 
        { 
        } 
        public void Call(WorkflowExecutionContext context) 
        { 
            Call(new object[] { context }); 
        } 
    } 
}

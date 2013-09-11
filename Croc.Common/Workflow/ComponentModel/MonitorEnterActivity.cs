using System; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Действие, которое получает эксклюзивную блокировку с заданным именем 

    /// </summary> 

    [Serializable] 

    public class MonitorEnterActivity : MonitorActivity 

    { 

        public MonitorEnterActivity() 

        { 

            base.ExecutionMethodCaller = new ActivityExecutionMethodCaller("MonitorEnter", this); 

        } 

 

 

        internal NextActivityKey MonitorEnter( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            context.MonitorEnter(LockName); 

            return context.DefaultNextActivityKey; 

        } 

    } 

}



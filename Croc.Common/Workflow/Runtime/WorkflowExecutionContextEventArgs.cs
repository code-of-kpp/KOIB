using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Workflow.ComponentModel; 

 

 

namespace Croc.Workflow.Runtime 

{ 

    /// <summary> 

    /// Аргументы события контекста выполнения экземпляра потока работ 

    /// </summary> 

    public class WorkflowExecutionContextEventArgs : EventArgs 

    { 

        /// <summary> 

        /// Контекст выполнения экземпляра потока работ 

        /// </summary> 

        public readonly WorkflowExecutionContext Context; 

        /// <summary> 

        /// Действие, к которому относится событие 

        /// </summary> 

        public readonly Activity Activity; 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="context"></param> 

        public WorkflowExecutionContextEventArgs(WorkflowExecutionContext context, Activity activity) 

        { 

            CodeContract.Requires(context != null); 

            CodeContract.Requires(activity != null); 

 

 

            Context = context; 

            Activity = activity; 

        } 

    } 

}



using System; 

 

 

namespace Croc.Workflow.Runtime.Hosting 

{ 

    /// <summary> 

    /// Базовый класс для всех сервисов исполняющей среды 

    /// </summary> 

    public abstract class WorkflowRuntimeService 

    { 

        /// <summary> 

        /// Исполняющая среда, в которую добавлен сервис 

        /// </summary> 

        public WorkflowRuntime Runtime 

        { 

            get; 

            internal set; 

        } 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        protected WorkflowRuntimeService() 

        { 

        } 

    } 

}



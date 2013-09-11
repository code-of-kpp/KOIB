using System; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Класс предназначен для вызова метода, который реализует логику действия 

    /// </summary> 

    [Serializable] 

    internal class ActivityExecutionMethodCaller : MethodCaller 

    { 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="methodName">имя метода</param> 

        /// <param name="methodOwner">объект-владелец метода</param> 

        public ActivityExecutionMethodCaller(string methodName, object methodOwner) 

            : base( 

                typeof(Func<WorkflowExecutionContext, ActivityParameterDictionary, NextActivityKey>), 

                methodName, 

                methodOwner) 

        { 

        } 

 

 

        /// <summary> 

        /// Вызвать метод 

        /// </summary> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey Call(WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return (NextActivityKey)base.Call(new object[] { context, parameters }); 

        } 

    } 

}



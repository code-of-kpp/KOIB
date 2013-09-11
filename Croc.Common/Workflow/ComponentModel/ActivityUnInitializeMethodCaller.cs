using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Класс предназначен для вызова метода инициализации и деинициализации действия 

    /// </summary> 

    [Serializable] 

    internal class ActivityUnInitializeMethodCaller : MethodCaller 

    { 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="methodName">имя метода</param> 

        /// <param name="methodOwner">объект-владелец метода</param> 

        public ActivityUnInitializeMethodCaller(string methodName, object methodOwner) 

            : base(typeof(Action<WorkflowExecutionContext>), methodName, methodOwner) 

        { 

        } 

 

 

        /// <summary> 

        /// Вызвать метод 

        /// </summary> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public void Call(WorkflowExecutionContext context) 

        { 

            base.Call(new object[] { context }); 

        } 

    } 

}



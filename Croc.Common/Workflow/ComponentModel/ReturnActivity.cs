using System; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Специальное действие, которое означает, что нужно выполнить выход из составного 

    /// действия с заданным результатом (ключом след. действия) 

    /// </summary> 

    [Serializable] 

    public class ReturnActivity : Activity 

    { 

        /// <summary> 

        /// Результат - ключ след. действия 

        /// </summary> 

        public NextActivityKey Result 

        { 

            get; 

            private set; 

        } 

 

 

        public ReturnActivity(NextActivityKey result) 

        { 

            Result = result; 

        } 

    } 

}



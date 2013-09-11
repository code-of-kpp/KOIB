using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Core; 

using Croc.Workflow.ComponentModel; 

 

 

namespace Croc.Bpc.Workflow 

{ 

    /// <summary> 

    /// Интерфейс подсистемы потока работ 

    /// </summary> 

    public interface IWorkflowManager : IStateSubsystem 

    { 

        /// <summary> 

        /// Поток работ завершил выполнение 

        /// </summary> 

        event EventHandler WorkflowStopped; 

 

 

        /// <summary> 

        /// Запустить поток работ 

        /// </summary> 

        void StartWorkflow(); 

 

 

        /// <summary> 

        /// Перейти к действию, которое определяет состояние потока работ 

        /// </summary> 

        void GoToStateActivity(bool sync); 

 

 

        /// <summary> 

        /// Перейти к действию с заданным именем 

        /// </summary> 

        void GoToActivity(string activityName, bool sync); 

 

 

        #region Счетчики ошибок 

 

 

        /// <summary> 

        /// Увеличить кол-во возникновения ошибки и возвращает увеличенное значение 

        /// </summary> 

        /// <param name="errorId">идентификатор ошибки</param> 

        int IncreaseErrorCounter(string errorId); 

 

 

        /// <summary> 


        /// Сбросить счетчики ошибок 

        /// </summary> 

        void ResetErrorCounters(); 

 

 

		/// <summary> 

		/// Сбросить определенный счетчик ошибок 

		/// </summary> 

		/// <param name="errorId">идентификатор ошибки</param> 

		void ResetErrorCounter(string errorId); 

 

 

        #endregion 

    } 

}



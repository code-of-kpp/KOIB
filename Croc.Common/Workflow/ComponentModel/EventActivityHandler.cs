using System; 

using System.Collections.Generic; 

using Croc.Workflow.Runtime; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Обработчик события, где в качестве обработчиков указан список действий, 

    /// которые должны начать выполняться при возникновении события 

    /// </summary> 

    /// <remarks> 

    /// Действия-обработчики делатся на 

    /// 1) Синхронный обработчик - это может быть только одно действие. При возникновении события, если 

    /// синхронный обработчик задан, то выполнение основного потока работ прерывается и управление передается  

    /// в данное действие-обработчик 

    /// 2) Асинхронные обработчики - это список действий действий. При возникновении события основной поток работ 

    /// продолжит выполняться, а для каждого действия-обработчика будет создан дополнительный поток поток работ, 

    /// которые будут работать параллельно с основным потоком работ.  

    /// При этом трекинг для этих потоков работ будет отключен. 

    /// </remarks> 

    [Serializable] 

    internal class EventActivityHandler 

    { 

        /// <summary> 

        /// Экземпляр потока работ 

        /// </summary> 

        [NonSerialized] 

        private WorkflowInstance _workflowInstance; 

 

 

        /// <summary> 

        /// Делегат метода-обработчика события 

        /// </summary> 

        public EventHandler Method 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Cинхронное действие-обработчик 

        /// </summary> 

        public Activity SyncActivity 

        { 

            get; 

            private set; 

        } 

 


 
        /// <summary> 

        /// Асинхронные действия-обработчики 

        /// </summary> 

        public List<Activity> AsyncActivities 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Содержатся ли действия-бработчики? 

        /// </summary> 

        public bool ContainsActivities 

        { 

            get 

            { 

                return SyncActivity != null || AsyncActivities.Count > 0; 

            } 

        } 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        public EventActivityHandler(WorkflowInstance workflowInstance) 

        { 

            CodeContract.Requires(workflowInstance != null); 

 

 

            _workflowInstance = workflowInstance; 

            Method = new EventHandler(OnEvent); 

            AsyncActivities = new List<Activity>(); 

        } 

 

 

        /// <summary> 

        /// Установить экземпляр потока работ 

        /// </summary> 

        /// <param name="workflowInstance"></param> 

        internal void SetWorkflowInstance(WorkflowInstance workflowInstance) 

        { 

            CodeContract.Requires(workflowInstance != null); 

            _workflowInstance = workflowInstance; 

        } 

 

 

        /// <summary> 

        /// Добавить действие-обработчик с заданным типом обработки 


        /// </summary> 

        /// <param name="handlerActivity"></param> 

        /// <param name="handlingType"></param> 

        public void AddActivity(Activity handlerActivity, EventHandlingType handlingType) 

        { 

            switch (handlingType) 

            { 

                case EventHandlingType.Sync: 

                    SyncActivity = handlerActivity; 

                    break; 

 

 

                case EventHandlingType.Async: 

                    if (!AsyncActivities.Contains(handlerActivity)) 

                        AsyncActivities.Add(handlerActivity); 

                    break; 

            } 

        } 

 

 

        /// <summary> 

        /// Удалить действие-обработчик 

        /// </summary> 

        /// <param name="handlerActivity"></param> 

        public void RemoveActivity(Activity handlerActivity) 

        { 

            if (SyncActivity == handlerActivity) 

            { 

                SyncActivity = null; 

                return; 

            } 

 

 

            if (AsyncActivities.Contains(handlerActivity)) 

                AsyncActivities.Remove(handlerActivity); 

        } 

 

 

        /// <summary> 

        /// Метод-обработчик события 

        /// </summary> 

        /// <param name="sender"></param> 

        /// <param name="e"></param> 

        private void OnEvent(object sender, EventArgs e) 

        { 

            if (SyncActivity != null) 

                _workflowInstance.GoToActivity(SyncActivity, false); 

 

 

            foreach (var activity in AsyncActivities) 


                _workflowInstance.StartChildWorkflow(activity); 

        } 

    } 

}



using System; 

using Croc.Workflow.ComponentModel; 

using System.Threading; 

 

 

namespace Croc.Workflow.Runtime 

{ 

    public class WorkflowInstance 

    { 

        /// <summary> 

        /// Идентификатор экземпляра потока работ 

        /// </summary> 

        public readonly Guid InstanceId; 

        /// <summary> 

        /// Исполняющая среда 

        /// </summary> 

        public WorkflowRuntime Runtime 

        { 

            get; 

            private set; 

        } 

        /// <summary> 

        /// Контекст выполнения экземпляра потока работ 

        /// </summary> 

        public WorkflowExecutionContext ExecutionContext 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="runtime"></param> 

        /// <param name="executionContext"></param> 

        internal WorkflowInstance(Guid instanceId, WorkflowRuntime runtime, WorkflowExecutionContext executionContext) 

        { 

            CodeContract.Requires(instanceId != Guid.Empty); 

            CodeContract.Requires(runtime != null); 

            CodeContract.Requires(executionContext != null); 

 

 

            InstanceId = instanceId; 

            Runtime = runtime; 

            ExecutionContext = executionContext; 

            ExecutionContext.SetWorkflowInstance(this); 

        } 

 

 


        #region Выполнение потока работ 

 

 

        /// <summary> 

        /// Нить выполнения рабочего потока 

        /// </summary> 

        private Thread _executionThread; 

 

 

        /// <summary> 

        /// Запуск выполнения данного экземпляра потока работ 

        /// </summary> 

        public void Start() 

        { 

            // запускаем поток выполнения 

            _executionThread = new Thread(new ThreadStart(ExecuteWorkflowMethod));             

            _executionThread.Start(); 

        } 

 

 

        /// <summary> 

        /// Прекращение работы экземпляра потока работ 

        /// </summary> 

        public void Stop() 

        { 

            ExecutionContext.InterruptExecution(); 

        } 

 

 

        /// <summary> 

        /// Аварийное прекращение работы экземпляра потока работ 

        /// </summary> 

        public void Abort() 

        { 

            if (_executionThread != null) 

                _executionThread.Abort(); 

        } 

 

 

        /// <summary> 

        /// Перейти к выполнению действия 

        /// </summary> 

        /// <remarks> 

        /// Текущее выполнение потока работ прерывается и продолжается с заданного действия 

        /// </remarks> 

        /// <param name="activity">действие, выполнять которое нужно начать</param> 

        /// <param name="sync">нужно ли выполнить переключение синхронно</param> 

        public void GoToActivity(string activityName, bool sync) 

        { 

            CodeContract.Requires(!string.IsNullOrEmpty(activityName)); 


 
 

            if (!ExecutionContext.Scheme.Activities.ContainsKey(activityName)) 

                throw new Exception("Действие не найдено: " + activityName); 

 

 

            var activity = ExecutionContext.Scheme.Activities[activityName]; 

            ExecutionContext.ToggleExecutionToActivity(activity, sync); 

        } 

 

 

        /// <summary> 

        /// Перейти к выполнению действия 

        /// </summary> 

        /// <remarks> 

        /// Текущее выполнение потока работ прерывается и продолжается с заданного действия 

        /// </remarks> 

        /// <param name="activity">действие, выполнять которое нужно начать</param> 

        /// <param name="sync">нужно ли выполнить переключение синхронно</param> 

        public void GoToActivity(Activity activity, bool sync) 

        { 

            ExecutionContext.ToggleExecutionToActivity(activity, sync); 

        } 

 

 

        /// <summary> 

        /// Создает и запускает дочерний экземпляр  

        /// </summary> 

        /// <param name="rootActivity"></param> 

        public WorkflowInstance StartChildWorkflow(Activity rootActivity) 

        { 

            //TODO: сделать 

            return null; 

        } 

 

 

        /// <summary> 

        /// Метод выполнения потока работ 

        /// </summary> 

        private void ExecuteWorkflowMethod() 

        { 

            Runtime.RaiseWorkflowStarted(this); 

            var activityToExecute = ExecutionContext.Scheme.RootActivity; 

 

 

            while (true) 

            { 

                try 

                { 

                    var res = activityToExecute.Execute(ExecutionContext); 


                    Runtime.RaiseWorkflowCompleted(this, res); 

                } 

                catch (ActivityExecutionInterruptException ex) 

                { 

                    try 

                    { 

                        activityToExecute = ExecutionContext.GetToggledActivity(ex); 

                        continue; 

                    } 

                    catch 

                    { 

                        // если поймали исключение, то значит это прерывание выполнения 

                        // было вызвано остановкой работы потока работ 

                        Runtime.RaiseWorkflowCompleted(this, null); 

                    } 

                } 

                catch (ThreadAbortException ex) 

                { 

                    Runtime.RaiseWorkflowTerminated(this, 

                        new Exception("Выполнение потока работ неожиданно прервано", ex)); 

                } 

                catch (Exception ex) 

                { 

                    Runtime.RaiseWorkflowTerminated(this, 

                        new Exception("При выполнении потока работ произошла ошибка", ex)); 

                } 

 

 

                return; 

            } 

        } 

 

 

        #endregion 

 

 

        #region Equals & GetHashCode 

 

 

        public override bool Equals(object obj) 

        { 

            if (obj == null) 

                return false; 

 

 

            var other = obj as WorkflowInstance; 

            if (other == null) 

                return false; 

 

 


            return other.InstanceId.Equals(this.InstanceId); 

        } 

 

 

        public override int GetHashCode() 

        { 

            return InstanceId.GetHashCode(); 

        } 

 

 

        #endregion 

    } 

}



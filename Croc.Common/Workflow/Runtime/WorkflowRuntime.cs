using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Xml; 

using System.Collections.ObjectModel; 

using Croc.Workflow.Runtime.Hosting; 

using Croc.Workflow.ComponentModel; 

using Croc.Core.Extensions; 

 

 

namespace Croc.Workflow.Runtime 

{ 

    /// <summary> 

    /// Исполняющая среда потока работ 

    /// </summary> 

    [Serializable] 

    public class WorkflowRuntime : IDisposable 

    { 

        /// <summary> 

        /// Все исполняющие среды 

        /// </summary> 

        private static List<WorkflowRuntime> s_runtimes = new List<WorkflowRuntime>(); 

 

 

        /// <summary> 

        /// Список сервисов 

        /// </summary> 

        private List<object> _services; 

        /// <summary> 

        /// Экземпляры потоков работ, выполняемые в данной исполняющей среде 

        /// </summary> 

        private Dictionary<Guid, WorkflowInstance> _instances; 

 

 

        /// <summary> 

        /// Запущена ли исполняющая среда 

        /// </summary> 

        public bool IsStarted 

        { 

            get; 

            private set; 

        } 

 

 

        #region События 

 

 

        /// <summary> 

        /// Событие "Экземпляр потока работ создан" 


        /// </summary> 

        public event EventHandler<WorkflowEventArgs> WorkflowCreated; 

 

 

        /// <summary> 

        /// Событие "Выполнение экземпляра потока работ запущено" 

        /// </summary> 

        public event EventHandler<WorkflowEventArgs> WorkflowStarted; 

 

 

        /// <summary> 

        /// Возбуждение события "Выполнение экземпляра потока работ запущено" 

        /// </summary> 

        /// <param name="wi">экземпляра потока работ, который был запущен</param> 

        internal void RaiseWorkflowStarted(WorkflowInstance wi) 

        { 

            WorkflowStarted.RaiseEvent(this, new WorkflowEventArgs(wi)); 

        } 

 

 

        /// <summary> 

        /// Событие "Выполнение экземпляра потока работ завершено" 

        /// </summary> 

        public event EventHandler<WorkflowCompletedEventArgs> WorkflowCompleted; 

 

 

        /// <summary> 

        /// Возбуждение события "Выполнение экземпляра потока работ завершено" 

        /// </summary> 

        /// <param name="wi">экземпляра потока работ, который завершил выполнение</param> 

        /// <param name="result">данные, полученные в результате выполнения потока работ</param> 

        internal void RaiseWorkflowCompleted(WorkflowInstance wi, object result) 

        { 

            WorkflowCompleted.RaiseEvent(this, new WorkflowCompletedEventArgs(wi, result)); 

        } 

 

 

        /// <summary> 

        /// Событие "Выполнение экземпляра потока работ прервано" 

        /// </summary> 

        public event EventHandler<WorkflowTerminatedEventArgs> WorkflowTerminated; 

 

 

        /// <summary> 

        /// Возбуждение события "Выполнение экземпляра потока работ прервано" 

        /// </summary> 

        /// <param name="wi">экземпляра потока работ, выполнение которого было прервано</param> 

        /// <param name="ex">исключение, которое послужило причиной для прерывания выполнения</param> 

        internal void RaiseWorkflowTerminated(WorkflowInstance wi, Exception ex) 

        { 


            WorkflowTerminated.RaiseEvent(this, new WorkflowTerminatedEventArgs(wi, ex)); 

        } 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        public WorkflowRuntime() 

        { 

            _services = new List<object>(); 

            _instances = new Dictionary<Guid, WorkflowInstance>(); 

            IsStarted = false; 

        } 

 

 

        #region Сервисы 

 

 

        /// <summary> 

        /// Добавление сервиса 

        /// </summary> 

        /// <param name="serviceToAdd"></param> 

        public void AddService(object serviceToAdd) 

        { 

            CodeContract.Requires(serviceToAdd != null); 

 

 

            // выполним проверки 

            foreach (object service in _services) 

            { 

                if (service == serviceToAdd) 

                    throw new InvalidOperationException("Нельзя добавить сервис, т.к. он был добавлен ранее"); 

 

 

                if (serviceToAdd is WorkflowSchemeLoaderService && 

                    service is WorkflowSchemeLoaderService) 

                { 

                    throw new InvalidOperationException("Только один сервис загрузки может быть добавлен"); 

                } 

 

 

                if (serviceToAdd is WorkflowPersistenceService && 

                    service is WorkflowPersistenceService) 

                { 

                    throw new InvalidOperationException("Только один сервис постоянства может быть добавлен"); 

                } 

            } 


 
 

            var runtime_service = serviceToAdd as WorkflowRuntimeService; 

            if (runtime_service != null) 

                runtime_service.Runtime = this; 

 

 

            _services.Add(serviceToAdd); 

        } 

 

 

        /// <summary> 

        /// Возвращает все сервисы заданного типа 

        /// </summary> 

        /// <typeparam name="T"></typeparam> 

        /// <returns></returns> 

        public ReadOnlyCollection<T> GetAllServices<T>() 

        { 

            List<T> services_req = new List<T>(); 

 

 

            foreach (object service in _services) 

            { 

                if (service is T) 

                    services_req.Add((T)service); 

            } 

 

 

            return new ReadOnlyCollection<T>(services_req); 

        } 

 

 

        /// <summary> 

        /// Возвращает сервис заданного типа 

        /// </summary> 

        /// <typeparam name="T"></typeparam> 

        /// <returns>сервис заданного типа,  

        /// null - если сервиса такого типа не было добавлено в исполняющую среду</returns> 

        public T GetService<T>() 

        { 

            ReadOnlyCollection<T> foundServices = GetAllServices<T>(); 

 

 

            if (foundServices.Count > 1) 

                throw new InvalidOperationException( 

                    string.Format("Найдено более одного сервиса типа {0}", typeof(T).Name)); 

 

 

            if (foundServices.Count == 0) 

                return default(T); 


 
 

            return foundServices[0]; 

        } 

 

 

        #endregion 

 

 

        #region Создание экземпляра потока работ 

 

 

        /// <summary> 

        /// Создание экземпляра потока работ по схеме потока работ из файла 

        /// </summary> 

        /// <param name="instanceId">идентификатор экземпляра потока работ</param> 

        /// <param name="workflowSchemeUri">xml-файл со схемой потока работ</param> 

        /// <param name="customXmlSchemas">список пользовательских xsd-схем</param> 

        /// <returns></returns> 

        public WorkflowInstance CreateWorkflow( 

            Guid instanceId,  

            string workflowSchemeUri, 

            IEnumerable<KeyValuePair<string, XmlReader>> customXmlSchemas) 

        { 

            CodeContract.Requires(!string.IsNullOrEmpty(workflowSchemeUri)); 

 

 

            // если исполняющая среда еще не запущена, то запустим ее 

            if (!IsStarted) 

                StartRuntime(); 

 

 

            // возьмем сервис загрузки схемы потока работ 

            var loader = GetService<WorkflowSchemeLoaderService>(); 

            // загрузим схему потока работ 

            var workflowScheme = loader.CreateInstance(workflowSchemeUri, customXmlSchemas); 

            // создадим контекст выполнения 

            var context = new WorkflowExecutionContext(workflowScheme); 

 

 

            return CreateWorkflow(instanceId, context); 

        } 

 

 

        /// <summary> 

        /// Восстановить экземпляра потока работ из хранилища 

        /// </summary> 

        /// <param name="instanceId"></param> 

        /// <returns></returns> 

        public WorkflowInstance RestoreWorkflow(Guid instanceId) 


        { 

            // если исполняющая среда еще не запущена, то запустим ее 

            if (!IsStarted) 

                StartRuntime(); 

 

 

            var persistenceService = this.GetService<WorkflowPersistenceService>(); 

            if (persistenceService == null) 

                throw new Exception("Сервис постоянства не найден"); 

 

 

            // загрузим контекст из хранилища 

            var context = persistenceService.LoadWorkflowInstanceState(instanceId); 

 

 

            return CreateWorkflow(instanceId, context); 

        } 

 

 

        /// <summary> 

        /// Восстанавливает экземпляр потока работ или создает новый, если восстановить не получается 

        /// </summary> 

        /// <param name="instanceId"></param> 

        /// <param name="workflowSchemeUri"></param> 

        /// <param name="customXmlSchemas"></param> 

        /// <returns></returns> 

        public WorkflowInstance RestoreOrCreateWorkflow( 

            Guid instanceId,  

            string workflowSchemeUri, 

            IEnumerable<KeyValuePair<string, XmlReader>> customXmlSchemas) 

        { 

            try 

            { 

                // попробуем восстановить экземпляр потока работ 

                return RestoreWorkflow(instanceId); 

            } 

            catch 

            { 

                // создадим новый экземпляр поток работ 

                return CreateWorkflow(instanceId, workflowSchemeUri, customXmlSchemas); 

            } 

        } 

 

 

        /// <summary> 

        /// Создает экземпляр потока работ с заданным контекстом выполнения 

        /// </summary> 

        /// <param name="context"></param> 

        /// <returns></returns> 

        private WorkflowInstance CreateWorkflow(Guid instanceId, WorkflowExecutionContext context) 


        { 

            CodeContract.Requires(context != null); 

 

 

            // подпишемся на событие изменения состояния контекста 

            context.ExecutionContextChanged += new EventHandler<WorkflowExecutionContextEventArgs>( 

                WorkflowExecutionContextChanged); 

 

 

            // создадим экземпляр потока работ 

            var instance = new WorkflowInstance(instanceId, this, context); 

 

 

            // добавим экземпляр потока работ в коллекцию экземпляров 

            _instances.Add(instance.InstanceId, instance); 

 

 

            // сообщим, что экземпляр потока работ создан 

            WorkflowCreated.RaiseEvent(this, new WorkflowEventArgs(instance)); 

 

 

            return instance; 

        } 

 

 

        /// <summary> 

        /// Контекст выполнения потока работ изменился 

        /// </summary> 

        /// <param name="sender"></param> 

        /// <param name="e"></param> 

        private void WorkflowExecutionContextChanged(object sender, WorkflowExecutionContextEventArgs e) 

        { 

            var persistenceService = this.GetService<WorkflowPersistenceService>(); 

            if (persistenceService != null) 

                persistenceService.SaveWorkflowInstanceState(e.Context); 

        } 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Запуск исполняющей среды 

        /// </summary> 

        public void StartRuntime() 

        { 

            // добавляем инфраструктурные дефолтовые сервисы, если пользователь их не добавил сам 

            if (GetAllServices<WorkflowSchemeLoaderService>().Count == 0) 

                AddService(new DefaultWorkflowSchemeLoaderService()); 

 


 
            IsStarted = true; 

            s_runtimes.Add(this); 

        } 

 

 

        /// <summary> 

        /// Остановка исполняющей среды 

        /// </summary> 

        public void StopRuntime() 

        { 

            IsStarted = false; 

 

 

            foreach (WorkflowInstance wi in _instances.Values) 

            { 

                wi.Stop(); 

            } 

 

 

            s_runtimes.Remove(this); 

        } 

 

 

        #region IDisposable Members 

 

 

        public void Dispose() 

        { 

            if (IsStarted) 

                StopRuntime(); 

        } 

 

 

        #endregion 

    } 

}



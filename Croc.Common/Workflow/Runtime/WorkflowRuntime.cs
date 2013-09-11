using System; 
using System.Collections.Generic; 
using System.Collections.ObjectModel; 
using System.Xml; 
using Croc.Core; 
using Croc.Core.Extensions; 
using Croc.Workflow.ComponentModel; 
using Croc.Workflow.Runtime.Hosting; 
namespace Croc.Workflow.Runtime 
{ 
    [Serializable] 
    public class WorkflowRuntime : IDisposable 
    { 
        private static readonly List<WorkflowRuntime> s_runtimes = new List<WorkflowRuntime>(); 
        private readonly List<object> _services; 
        private readonly Dictionary<Guid, WorkflowInstance> _instances; 
        public bool IsStarted 
        { 
            get; 
            private set; 
        } 
        #region События 
        public event EventHandler<WorkflowEventArgs> WorkflowCreated; 
        public event EventHandler<WorkflowEventArgs> WorkflowStarted; 
        internal void RaiseWorkflowStarted(WorkflowInstance wi) 
        { 
            WorkflowStarted.RaiseEvent(this, new WorkflowEventArgs(wi)); 
        } 
        public event EventHandler<WorkflowCompletedEventArgs> WorkflowCompleted; 
        internal void RaiseWorkflowCompleted(WorkflowInstance wi, object result) 
        { 
            WorkflowCompleted.RaiseEvent(this, new WorkflowCompletedEventArgs(wi, result)); 
        } 
        public event EventHandler<WorkflowTerminatedEventArgs> WorkflowTerminated; 
        internal void RaiseWorkflowTerminated(WorkflowInstance wi, string reason) 
        { 
            WorkflowTerminated.RaiseEvent(this, new WorkflowTerminatedEventArgs(wi, reason)); 
        } 
        #endregion 
        public WorkflowRuntime() 
        { 
            _services = new List<object>(); 
            _instances = new Dictionary<Guid, WorkflowInstance>(); 
            IsStarted = false; 
        } 
        #region Сервисы 
        public void AddService(object serviceToAdd) 
        { 
            CodeContract.Requires(serviceToAdd != null); 
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
            var runtimeService = serviceToAdd as WorkflowRuntimeService; 
            if (runtimeService != null) 
                runtimeService.Runtime = this; 
            _services.Add(serviceToAdd); 
        } 
        public ReadOnlyCollection<T> GetAllServices<T>() 
        { 
            var servicesReq = new List<T>(); 
            foreach (var service in _services) 
            { 
                if (service is T) 
                    servicesReq.Add((T)service); 
            } 
            return new ReadOnlyCollection<T>(servicesReq); 
        } 
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
        public WorkflowInstance CreateWorkflow( 
            Guid instanceId,  
            string workflowSchemeUri, 
            IEnumerable<KeyValuePair<string, XmlReader>> customXmlSchemas) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(workflowSchemeUri)); 
            if (!IsStarted) 
                StartRuntime(); 
            var loader = GetService<WorkflowSchemeLoaderService>(); 
            var workflowScheme = loader.CreateInstance(workflowSchemeUri, customXmlSchemas); 
            var context = new WorkflowExecutionContext(workflowScheme); 
            return CreateWorkflow(instanceId, context); 
        } 
        public WorkflowInstance RestoreWorkflow(Guid instanceId) 
        { 
            if (!IsStarted) 
                StartRuntime(); 
            var persistenceService = GetService<WorkflowPersistenceService>(); 
            if (persistenceService == null) 
                throw new Exception("Сервис постоянства не найден"); 
            var context = persistenceService.LoadWorkflowInstanceState(instanceId); 
            return CreateWorkflow(instanceId, context); 
        } 
        public WorkflowInstance RestoreOrCreateWorkflow( 
            Guid instanceId,  
            string workflowSchemeUri, 
            IEnumerable<KeyValuePair<string, XmlReader>> customXmlSchemas) 
        { 
            try 
            { 
                return RestoreWorkflow(instanceId); 
            } 
            catch 
            { 
                return CreateWorkflow(instanceId, workflowSchemeUri, customXmlSchemas); 
            } 
        } 
        private WorkflowInstance CreateWorkflow(Guid instanceId, WorkflowExecutionContext context) 
        { 
            CodeContract.Requires(context != null); 
            context.ExecutionContextChanged += WorkflowExecutionContextChanged; 
            var instance = new WorkflowInstance(instanceId, this, context); 
            _instances.Add(instance.InstanceId, instance); 
            WorkflowCreated.RaiseEvent(this, new WorkflowEventArgs(instance)); 
            return instance; 
        } 
        private void WorkflowExecutionContextChanged(object sender, WorkflowExecutionContextEventArgs e) 
        { 
            var persistenceService = GetService<WorkflowPersistenceService>(); 
            if (persistenceService != null) 
                persistenceService.SaveWorkflowInstanceState(e.Context); 
        } 
        #endregion 
        public void StartRuntime() 
        { 
            if (GetAllServices<WorkflowSchemeLoaderService>().Count == 0) 
                AddService(new DefaultWorkflowSchemeLoaderService()); 
            IsStarted = true; 
            s_runtimes.Add(this); 
        } 
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

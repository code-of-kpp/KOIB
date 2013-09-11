using System; 
using System.Threading; 
using Croc.Core; 
using Croc.Core.Extensions; 
using Croc.Core.Utils.Threading; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Workflow.Runtime 
{ 
    public class WorkflowInstance 
    { 
        public readonly Guid InstanceId; 
        public readonly WorkflowRuntime Runtime; 
        public readonly WorkflowExecutionContext ExecutionContext; 
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
        public void Start() 
        { 
            ThreadUtils.StartBackgroundThread(ExecuteWorkflowMethod); 
        } 
        public void Stop() 
        { 
            ExecutionContext.StopExecution(); 
        } 
        public void GoToActivity(string activityName) 
        { 
            CodeContract.Requires(!string.IsNullOrEmpty(activityName)); 
            if (!ExecutionContext.Scheme.Activities.ContainsKey(activityName)) 
                throw new Exception("Действие не найдено: " + activityName); 
            var activity = ExecutionContext.Scheme.Activities[activityName]; 
            GoToActivity(activity); 
        } 
        public void GoToActivity(Activity activity) 
        { 
            ExecutionContext.ToggleExecutionToActivity(activity); 
        } 
        private void ExecuteWorkflowMethod() 
        { 
            Runtime.RaiseWorkflowStarted(this); 
            var activityToExecute = ExecutionContext.Scheme.RootActivity; 
            ExecutionContext.StartExecution(); 
            while (true) 
            { 
                var returnActivity = activityToExecute as ReturnActivity; 
                if (returnActivity != null) 
                { 
                    Runtime.RaiseWorkflowCompleted(this, returnActivity.Result); 
                    return; 
                } 
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
                    catch (Exception ex2) 
                    { 
                        Runtime.RaiseWorkflowTerminated(this, "GetToggledActivity throw: " + ex2); 
                    } 
                } 
                catch (Exception ex) 
                { 
                    Runtime.RaiseWorkflowTerminated(this, ex.ToString()); 
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
            return other.InstanceId.Equals(InstanceId); 
        } 
        public override int GetHashCode() 
        { 
            return InstanceId.GetHashCode(); 
        } 
        #endregion 
    } 
}

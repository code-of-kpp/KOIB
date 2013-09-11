using System; 
using System.Collections.Generic; 
using Croc.Core.Utils.Collections; 
namespace Croc.Workflow.ComponentModel 
{ 
    [Serializable] 
    public class Activity : INamed 
    { 
        #region Свойства 
        [NonSerialized] 
        private bool _initialized; 
        public string Name 
        { 
            get; 
            internal set; 
        } 
        internal ActivityUnInitializeMethodCaller InitializeMethodCaller 
        { 
            get; 
            set; 
        } 
        internal ActivityUnInitializeMethodCaller UninitializeMethodCaller 
        { 
            get; 
            set; 
        } 
        internal ActivityExecutionMethodCaller ExecutionMethodCaller 
        { 
            get; 
            set; 
        } 
        public ActivityParameterDictionary Parameters 
        { 
            get; 
            private set; 
        } 
        public Dictionary<NextActivityKey, Activity> NextActivities 
        { 
            get; 
            private set; 
        } 
        public Activity FollowingActivity 
        { 
            get; 
            internal set; 
        } 
        public Activity Parent 
        { 
            get; 
            internal set; 
        } 
        public Activity Root 
        { 
            get 
            { 
                if (Parent == null) 
                    return null; 
                Activity root = Parent; 
                while (root.Parent != null) 
                    root = root.Parent; 
                return root; 
            } 
        } 
        internal bool Tracking 
        { 
            get; 
            set; 
        } 
        public ActivityPriority Priority 
        { 
            get; 
            internal set; 
        } 
        #endregion 
        public Activity() 
        { 
            Parameters = new ActivityParameterDictionary(); 
            NextActivities = new Dictionary<NextActivityKey, Activity>(); 
            Tracking = true; 
            Priority = ActivityPriority.Default; 
            _initialized = false; 
        } 
        #region Инициализация и деинициализация 
        private void _Initialize(WorkflowExecutionContext context) 
        { 
            if (!_initialized) 
            { 
                Initialize(context); 
                _initialized = true; 
            } 
        } 
        private void _Uninitialize(WorkflowExecutionContext context) 
        { 
            if (_initialized) 
            { 
                Uninitialize(context); 
                _initialized = false; 
            } 
        } 
        protected virtual void Initialize(WorkflowExecutionContext context) 
        { 
            if (InitializeMethodCaller != null) 
                InitializeMethodCaller.Call(context); 
        } 
        protected virtual void Uninitialize(WorkflowExecutionContext context) 
        { 
            if (UninitializeMethodCaller != null) 
                UninitializeMethodCaller.Call(context); 
        } 
        #endregion 
        #region Выполнение действия 
        internal NextActivityKey Execute(WorkflowExecutionContext context) 
        { 
            return _Execute(context, Parameters); 
        } 
        internal NextActivityKey Execute(WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var execParameters = new ActivityParameterDictionary(); 
            foreach (var param in parameters.Values) 
                execParameters.Add(param); 
            foreach (var param in Parameters) 
            { 
                if (!execParameters.ContainsKey(param.Name)) 
                    execParameters.Add(param); 
            } 
            return _Execute(context, execParameters); 
        } 
        private NextActivityKey _Execute(WorkflowExecutionContext context, ActivityParameterDictionary execParameters) 
        { 
            if (Parent != null && !Parent._initialized) 
            { 
                Parent._Initialize(context); 
            } 
            _Initialize(context); 
            var originalСontextTracking = context.Tracking; 
            if (!Tracking) 
                context.Tracking = false; 
            var originalСontextPriority = context.Priority; 
            if (Priority > originalСontextPriority) 
                context.Priority = Priority; 
            NextActivityKey res = null; 
            try 
            { 
                context.ActivityExecuting(this); 
                res = ExecutionMethodCaller.Call(context, execParameters); 
                context.ActivityExecuted(this); 
            } 
            catch (ActivityExecutionInterruptException ex) 
            { 
                context.ActivityExecutionInterrupted(ex); 
            } 
            catch (Exception ex) 
            { 
                var interruptException = ex.InnerException as ActivityExecutionInterruptException; 
                if (interruptException != null) 
                    context.ActivityExecutionInterrupted(interruptException); 
                else 
                    throw new ActivityExecutionException("Ошибка выполнения действия", ex, this, context); 
            } 
            finally 
            { 
                context.Tracking = originalСontextTracking; 
                context.Priority = originalСontextPriority; 
                _Uninitialize(context); 
            } 
            return res; 
        } 
        #endregion  
        public override string ToString() 
        { 
            return Name; 
        } 
    } 
}

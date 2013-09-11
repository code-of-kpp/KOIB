using System; 
using System.Collections.Generic; 
using System.IO; 
using System.Runtime.Serialization.Formatters.Binary; 
using System.Threading; 
using Croc.Core; 
using Croc.Core.Diagnostics.Default; 
using Croc.Core.Extensions; 
using Croc.Core.Utils.Collections; 
using Croc.Core.Utils.Threading; 
using Croc.Workflow.Runtime; 
namespace Croc.Workflow.ComponentModel 
{ 
    public class WorkflowExecutionContext : IWaitController 
    { 
        private static readonly BinaryFormatter s_formatter = new BinaryFormatter(); 
        private WorkflowInstance _workflowInstance; 
        public WorkflowScheme Scheme 
        { 
            get; 
            private set; 
        } 
        public Guid InstanceId 
        { 
            get 
            { 
                return _workflowInstance.InstanceId; 
            } 
        } 
        public bool Tracking 
        { 
            get; 
            internal set; 
        } 
        public ActivityPriority Priority 
        { 
            get; 
            internal set; 
        } 
        #region События 
        public event EventHandler<WorkflowExecutionContextEventArgs> ActivityExecutionStarting; 
        public event EventHandler<WorkflowExecutionContextEventArgs> ActivityExecutionFinished; 
        public event EventHandler<WorkflowExecutionContextEventArgs> ExecutionContextChanged; 
        #endregion 
        internal WorkflowExecutionContext(WorkflowScheme scheme) 
        { 
            CodeContract.Requires(scheme != null); 
            Scheme = scheme; 
            Tracking = true; 
            Priority = ActivityPriority.Default; 
        } 
        internal void SetWorkflowInstance(WorkflowInstance workflowInstance) 
        { 
            CodeContract.Requires(workflowInstance != null); 
            _workflowInstance = workflowInstance; 
            foreach (var handler in _eventHandlersDict.Values) 
                handler.SetWorkflowInstance(_workflowInstance); 
        } 
        public T GetService<T>() 
        { 
            return _workflowInstance.Runtime.GetService<T>(); 
        } 
        public NextActivityKey DefaultNextActivityKey 
        { 
            get 
            { 
                return Scheme.DefaultNextActivityKey; 
            } 
        } 
        #region Управление процессом выполнения потока работ 
        private static readonly object s_interruptExecutionSync = new object(); 
        private readonly ManualResetEvent _interruptExecutionEvent = new ManualResetEvent(false); 
        private readonly ManualResetEvent _interruptExecutionFinishedEvent = new ManualResetEvent(true); 
        private volatile Activity _toggleActivity; 
        private readonly BlockingQueue<Activity> _toggleActivityQueue = new BlockingQueue<Activity>(); 
        internal void StartExecution() 
        { 
            _toggleActivityQueue.Clear(); 
            _toggleActivityQueue.Open(); 
            ThreadUtils.StartBackgroundThread(InterruptExecutionThread); 
        } 
        internal void StopExecution() 
        { 
            _toggleActivityQueue.Close(); 
            InterruptExecution(Scheme.ExitActivity); 
        } 
        internal void ToggleExecutionToActivity(Activity toggleActivity) 
        { 
            _toggleActivityQueue.TryEnqueue(toggleActivity); 
        } 
        private void InterruptExecutionThread() 
        { 
            Activity toggleActivity; 
            while (_toggleActivityQueue.TryDequeue(out toggleActivity)) 
            { 
                InterruptExecution(toggleActivity); 
            } 
        } 
        private void InterruptExecution(Activity toggleActivity) 
        { 
            while (true) 
            { 
                _interruptExecutionFinishedEvent.WaitOne(); 
                lock (s_interruptExecutionSync) 
                { 
                    if (!_interruptExecutionFinishedEvent.WaitOne(0)) 
                    { 
                        continue; 
                    } 
                    _toggleActivity = toggleActivity; 
                    if (_toggleActivity.Priority >= Priority) 
                    { 
                        _interruptExecutionFinishedEvent.Reset(); 
                        _interruptExecutionEvent.Set(); 
                    } 
                    else 
                    { 
                    } 
                    return; 
                } 
            } 
        } 
        private void CheckNeedInterrupt() 
        { 
            lock (s_interruptExecutionSync) 
            { 
                if (_toggleActivity == null || _toggleActivity.Priority < Priority) 
                    return; 
                _interruptExecutionFinishedEvent.Reset(); 
                throw new ActivityExecutionInterruptException(CurrentExecutingActivity, this); 
            } 
        } 
        public void ResetInterrupt() 
        { 
            lock (s_interruptExecutionSync) 
            { 
                _toggleActivity = null; 
                _interruptExecutionEvent.Reset(); 
                _interruptExecutionFinishedEvent.Set(); 
            } 
        } 
        internal void ActivityExecutionInterrupted(ActivityExecutionInterruptException ex) 
        { 
            if (_toggleActivity == null) 
            { 
                throw ex; 
            } 
            if (!IsStackCorrectForExecuteActivity(_toggleActivity)) 
                ExecutingActivitiesStackPop(); 
            throw ex; 
        } 
        internal Activity GetToggledActivity(ActivityExecutionInterruptException ex) 
        { 
            if (_toggleActivity == null || 
                !IsStackCorrectForExecuteActivity(_toggleActivity)) 
            { 
                throw ex; 
            } 
            var res = _toggleActivity; 
            ResetInterrupt(); 
            Tracking = GetTrackingBeforeExecuteActivity(res); 
            return res; 
        } 
        private static bool GetTrackingBeforeExecuteActivity(Activity activity) 
        { 
            var tmp = activity; 
            while (tmp.Parent != null) 
            { 
                if (!tmp.Parent.Tracking) 
                    return false; 
                tmp = tmp.Parent; 
            } 
            return tmp.Equals(activity) ? true : tmp.Tracking; 
        } 
        #endregion 
        #region Отслеживание стека выполнения действий 
        private static readonly object s_activitiesStackSync = new object(); 
        private readonly ListStack<string> _executingActivitiesStack = new ListStack<string>(); 
        private ListStack<string> _trackingActivitiesStack = new ListStack<string>(); 
        private int _restoredActivitiesCount; 


        [NonSerialized] 
        private bool _restoring; 
        public bool Restoring 
        { 
            get 
            { 
                return _restoring; 
            } 
        } 
        public Activity CurrentExecutingActivity 
        { 
            get 
            { 
                lock (s_activitiesStackSync) 
                { 
                    return _executingActivitiesStack.Count == 0 
                               ? null 
                               : Scheme.Activities[_executingActivitiesStack.Peek()]; 
                } 
            } 
        } 
        public IEnumerable<string> CurrentExecutingActivities() 
        { 
            lock (s_activitiesStackSync) 
            { 
                return new List<string>(_executingActivitiesStack); 
            } 
        } 
        internal void ActivityExecuting(Activity activity) 
        { 
            CodeContract.Requires(activity != null); 
            CheckNeedInterrupt(); 
            StartActivityExecutionTracking(activity); 
        } 
        internal void ActivityExecuted(Activity activity) 
        { 
            CodeContract.Requires(activity != null); 
            EndActivityExecutionTracking(activity); 
        } 
        private void ExecutingActivitiesStackPop() 
        { 
            lock (s_activitiesStackSync) 
            { 
                var popName = _executingActivitiesStack.Pop(); 
                if (_trackingActivitiesStack.Count > 0 && 
                    string.CompareOrdinal(_trackingActivitiesStack.Peek(), popName) == 0) 
                    _trackingActivitiesStack.Pop(); 
            } 
        } 
        private void StartActivityExecutionTracking(Activity activity) 
        { 
            lock (s_activitiesStackSync) 
            { 
                _executingActivitiesStack.Push(activity.Name); 
            } 
            var eventArgs = new WorkflowExecutionContextEventArgs(this, activity); 
            ActivityExecutionStarting.RaiseEvent(this, eventArgs); 
            if (!Tracking) 
                return; 
            if (_restoring) 
            { 
                if (_trackingActivitiesStack[_restoredActivitiesCount] != activity.Name) 
                { 
                    var msg = string.Format( 
                        "Ошибка при восстановлении выполнения потока работ: " + 
                        "действие {0}, которое начинает выполнение, отличается от действия {1}, " + 
                        "которое, согласно информации в контексте, должно начать выполнение", 
                        activity.Name, _trackingActivitiesStack[_restoredActivitiesCount]); 
                    throw new ActivityExecutionException(msg, activity, this); 
                } 
                _restoredActivitiesCount++; 
                if (_restoredActivitiesCount == _trackingActivitiesStack.Count) 
                    _restoring = false; 
                return; 
            } 
            _trackingActivitiesStack.Push(activity.Name); 
            ExecutionContextChanged.RaiseEvent(this, eventArgs); 
        } 
        private void EndActivityExecutionTracking(Activity activity) 
        { 
            lock (s_activitiesStackSync) 
            { 
                SafePopFromStack(_executingActivitiesStack, activity); 
            } 
            var eventArgs = new WorkflowExecutionContextEventArgs(this, activity); 
            ActivityExecutionFinished.RaiseEvent(this, eventArgs); 
            if (!Tracking) 
                return; 
            if (_restoring) 
                return; 
            SafePopFromStack(_trackingActivitiesStack, activity); 
            ExecutionContextChanged.RaiseEvent(this, eventArgs); 
        } 
        private static void SafePopFromStack(ListStack<string> stack, Activity activity) 
        { 
            if (stack.Count == 0) 
                throw new InvalidOperationException("Стек пуст"); 
            var top = stack.Peek(); 
            if (string.CompareOrdinal(top, activity.Name) != 0) 
                throw new InvalidOperationException("На вершине стека другое действие: " + top); 
            stack.Pop(); 
        } 
        internal string GetActivityNameToRestore() 
        { 
            if (!_restoring) 
                throw new InvalidOperationException("Контекст не находится в режиме восстановления работы"); 
            return _trackingActivitiesStack[_restoredActivitiesCount]; 
        } 
        private bool IsStackCorrectForExecuteActivity(Activity nextExecutingActivity) 
        { 
            lock (s_activitiesStackSync) 
            { 
                return 
                    _executingActivitiesStack.Count == 0 || 
                    (nextExecutingActivity.Parent != null && 
                     _executingActivitiesStack.Peek() == nextExecutingActivity.Parent.Name); 
            } 
        } 
        #endregion 
        #region IWaitController Members 
        public void Sleep(TimeSpan timeout) 
        { 
            Sleep(Convert.ToInt32(timeout.TotalMilliseconds)); 
        } 
        public void Sleep(int millisecondsTimeout) 
        { 
            if (_interruptExecutionEvent.WaitOne(millisecondsTimeout, false)) 
                throw new ActivityExecutionInterruptException(CurrentExecutingActivity, this); 
        } 
        public void WaitOne(WaitHandle waitHandle) 
        { 
            WaitAny(new[] { waitHandle }); 
        } 
        public bool WaitOne(WaitHandle waitHandle, TimeSpan timeout) 
        { 
            return WaitAny(new[] { waitHandle }, timeout) != WaitHandle.WaitTimeout; 
        } 
        public int WaitAny(WaitHandle[] waitHandles) 
        { 
            var waitHandlesEx = new List<WaitHandle>(waitHandles) { _interruptExecutionEvent }; 
            var index = WaitHandle.WaitAny(waitHandlesEx.ToArray()); 
            if (index == waitHandles.Length) 
                throw new ActivityExecutionInterruptException(CurrentExecutingActivity, this); 
            return index; 
        } 
        public int WaitAny(WaitHandle[] waitHandles, TimeSpan timeout) 
        { 
            return WaitAny(waitHandles, Convert.ToInt32(timeout.TotalMilliseconds)); 
        } 
        public int WaitAny(WaitHandle[] waitHandles, int timeout) 
        { 
            var waitHandlesEx = new List<WaitHandle>(waitHandles) { _interruptExecutionEvent }; 
            var index = WaitHandle.WaitAny(waitHandlesEx.ToArray(), timeout, false); 
            if (index == waitHandles.Length) 
                throw new ActivityExecutionInterruptException(CurrentExecutingActivity, this); 
            return index; 
        } 
        public int WaitOneOrAllOthers(WaitHandle one, WaitHandle[] others) 
        { 
            var index = WaitHandleUtils.WaitOneOrTwoOrAllOthers(_interruptExecutionEvent, one, others); 
            if (index == 0) 
                throw new ActivityExecutionInterruptException(CurrentExecutingActivity, this); 
            return index == WaitHandle.WaitTimeout 
                       ? index 
                       : (index - 1); 
        } 
        #endregion 
        #region Подписка на события 
        private static readonly object s_eventHandlersDictSync = new object(); 
        private Dictionary<string, EventActivityHandler> _eventHandlersDict = 
            new Dictionary<string, EventActivityHandler>(); 
        internal void SubscribeToEvent( 
            EventHolder eventHolder, Activity handlerActivity, EventHandlingType handlingType) 
        { 
            lock (s_eventHandlersDictSync) 
            { 
                EventActivityHandler handler; 
                if (!_eventHandlersDict.ContainsKey(eventHolder.EventName)) 
                { 
                    handler = new EventActivityHandler(_workflowInstance); 
                    _eventHandlersDict[eventHolder.EventName] = handler; 
                    eventHolder.AddEventHandler(handler.Method); 
                } 
                else 
                    handler = _eventHandlersDict[eventHolder.EventName]; 
                handler.AddActivity(handlerActivity, handlingType); 
            } 
        } 
        internal void UnsubscribeFromEvent(EventHolder eventHolder, Activity handlerActivity) 
        { 
            lock (s_eventHandlersDictSync) 
            { 
                if (!_eventHandlersDict.ContainsKey(eventHolder.EventName)) 
                    return; 
                var handler = _eventHandlersDict[eventHolder.EventName]; 
                handler.RemoveActivity(handlerActivity); 
                if (!handler.ContainsActivities) 
                { 
                    eventHolder.RemoveEventHandler(handler.Method); 
                    _eventHandlersDict.Remove(eventHolder.EventName); 
                } 
            } 
        } 
        #endregion 
        #region Мониторинг 
        internal void MonitorEnter(string lockName) 
        { 
        } 
        internal void MonitorExit(string lockName) 
        { 
        } 
        #endregion 
        #region Сериализация 
        public object GetState() 
        { 
            return new object[] {  
                Scheme,  
                _trackingActivitiesStack, 
                _eventHandlersDict 
            }; 
        } 
        public static void Save(WorkflowExecutionContext context, Stream stream) 
        { 
            s_formatter.Serialize(stream, context.GetState()); 
        } 
        public static WorkflowExecutionContext Load(Stream stream) 
        { 
            var arr = (object[])s_formatter.Deserialize(stream); 
            var context = new WorkflowExecutionContext((WorkflowScheme)arr[0]) 
            { 
                _trackingActivitiesStack = (ListStack<string>)arr[1], 
                _eventHandlersDict = (Dictionary<string, EventActivityHandler>)arr[2] 
            }; 
            context._restoring = (context._trackingActivitiesStack.Count > 0); 
            return context; 
        } 


        #endregion 
        public override string ToString() 
        { 
            lock (s_activitiesStackSync) 
            { 
                return _executingActivitiesStack.ToString(); 
            } 
        } 
    } 
}

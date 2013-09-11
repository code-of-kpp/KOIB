using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Runtime.Serialization; 

using System.IO; 

using System.Runtime.Serialization.Formatters.Binary; 

using System.Threading; 

 

 

using Croc.Workflow.Runtime; 

using Croc.Core.Utils.Collections; 

using Croc.Core.Extensions; 

using Croc.Core; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Контекст выполнения экземпляра потока работ 

    /// </summary> 

    public class WorkflowExecutionContext : IWaitController 

    { 

        /// <summary> 

        /// Форматтер для сериализации 

        /// </summary> 

        private static BinaryFormatter s_formatter = new BinaryFormatter(); 

 

 

        /// <summary> 

        /// Экземпляр потока работ 

        /// </summary> 

        private WorkflowInstance _workflowInstance; 

 

 

        /// <summary> 

        /// Схема потока работ 

        /// </summary> 

        public WorkflowScheme Scheme 

        { 

            get; 

            private set; 

        } 

        /// <summary> 

        /// Идентификатор контекста 

        /// </summary> 

        public Guid InstanceId 

        { 

            get 

            { 


                return _workflowInstance.InstanceId; 

            } 

        } 

        /// <summary> 

        /// Значение режима отслеживания по умолчанию 

        /// </summary> 

        private const bool DEFAULT_TRACKING = true; 

        /// <summary> 

        /// Включен ли режим отслеживания состояния 

        /// </summary> 

        public bool Tracking 

        { 

            get; 

            internal set; 

        } 

        /// <summary> 

        /// Приоритет текущего выполняемого действия 

        /// </summary> 

        public ActivityPriority Priority 

        { 

            get; 

            internal set; 

        } 

 

 

        #region События 

 

 

        /// <summary> 

        /// Событие "Действие начинает выполняться" 

        /// </summary> 

        public event EventHandler<WorkflowExecutionContextEventArgs> ActivityExecutionStarting; 

        /// <summary> 

        /// Событие "Выполнение действия завершено" 

        /// </summary> 

        public event EventHandler<WorkflowExecutionContextEventArgs> ActivityExecutionFinished; 

        /// <summary> 

        /// Событие "Контекст выполнения изменился" 

        /// </summary> 

        public event EventHandler<WorkflowExecutionContextEventArgs> ExecutionContextChanged; 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="scheme"></param> 

        internal WorkflowExecutionContext(WorkflowScheme scheme) 


        { 

            CodeContract.Requires(scheme != null); 

            Scheme = scheme; 

            Tracking = DEFAULT_TRACKING; 

            Priority = ActivityPriority.Default; 

        } 

 

 

        /// <summary> 

        /// Установить экземпляр потока работ 

        /// </summary> 

        /// <param name="workflowInstance"></param> 

        internal void SetWorkflowInstance(WorkflowInstance workflowInstance) 

        { 

            CodeContract.Requires(workflowInstance != null); 

            _workflowInstance = workflowInstance; 

 

 

            // установим ссылку на экземпляр потока работ для обработчиков событий 

            foreach (var handler in _eventHandlersDict.Values) 

                handler.SetWorkflowInstance(_workflowInstance); 

        } 

 

 

        /// <summary> 

        /// Получить сервис 

        /// </summary> 

        /// <typeparam name="T"></typeparam> 

        /// <returns></returns> 

        public T GetService<T>() 

        { 

            return _workflowInstance.Runtime.GetService<T>(); 

        } 

 

 

        /// <summary> 

        /// Ключ следующего действия по умолчанию 

        /// </summary> 

        public NextActivityKey DefaultNextActivityKey 

        { 

            get 

            { 

                return Scheme.DefaultNextActivityKey; 

            } 

        } 

 

 

        #region Управление процессом выполнения 

 

 


        /// <summary> 

        /// Событие прерывания выполнения рабочего потока 

        /// </summary> 

        private ManualResetEvent _interruptExecutionEvent = new ManualResetEvent(false); 

        /// <summary> 

        /// Событие завершения прерывания выполнения рабочего потока 

        /// </summary> 

        /// <remarks>изначально, считаем, что прерывание уже завершено</remarks> 

        private ManualResetEvent _interruptExecutionFinishedEvent = new ManualResetEvent(true); 

        /// <summary> 

        /// Действие, на которое нужно переключить выполнение потока работ 

        /// </summary> 

        private Activity _toggleActivity; 

 

 

 

 

        /// <summary> 

        /// Вызывается, чтобы сообщить контексту о том, что действие начало выполняться 

        /// </summary> 

        /// <param name="activity"></param> 

        internal void ActivityExecuting(Activity activity) 

        { 

            CodeContract.Requires(activity != null); 

 

 

            // если нужно прервать выполнение 

            if (NeedInterrupt()) 

                // сгереним исключение-прерывание 

                throw new ActivityExecutionInterruptException(CurrentExecutingActivity); 

 

 

            // начинаем отслеживание выполнения действия 

            StartActivityExecutionTracking(activity); 

        } 

 

 

        /// <summary> 

        /// Вызывается, чтобы сообщить контексту о том, что действие завершило выполнение 

        /// </summary> 

        /// <param name="activity"></param> 

        internal void ActivityExecuted(Activity activity) 

        { 

            CodeContract.Requires(activity != null); 

 

 

            // завершаем отслеживание выполнения действия 

            EndActivityExecutionTracking(activity); 

        } 

 


 
        /// <summary> 

        /// Переключить выполнение на действие 

        /// </summary> 

        /// <remarks>выполнение текущего действия прерывается и продолжается с заданного действия</remarks> 

        /// <param name="toggleActivity">Действие, на которое нужно переключить выполнение потока работ</param> 

        /// <param name="sync">нужно ли выполнить переключение синхронно.  

        /// Если true, то выполнение метода завершиться только, когда переключение выполнения  

        /// к заданному действию будет завершено</param> 

        internal void ToggleExecutionToActivity(Activity toggleActivity, bool sync) 

        { 

            _toggleActivity = toggleActivity; 

            _interruptExecutionFinishedEvent.Reset(); 

 

 

            // если нужно немедленно прервать выполнение 

            if (NeedInterrupt()) 

                // то выставим событие прерывания 

                _interruptExecutionEvent.Set(); 

 

 

            if (!sync) 

                return; 

 

 

            _interruptExecutionFinishedEvent.WaitOne(); 

        } 

 

 

        /// <summary> 

        /// Нужно ли прервать выполнение потока работ 

        /// </summary> 

        internal bool NeedInterrupt() 

        { 

            // нужно, если действие-прерывание определено и его приоритет не ниже приоритета контекста 

            return _toggleActivity != null && _toggleActivity.Priority >= Priority; 

        } 

 

 

        /// <summary> 

        /// Прерывание выполнения потока работ 

        /// </summary> 

        internal void InterruptExecution() 

        { 

            _interruptExecutionEvent.Set(); 

        } 

 

 

        /// <summary> 

        /// Отменить прерывание выполнения 


        /// </summary> 

        /// <remarks>Используется для того, чтобы действие могло отловить исключение о прерывании 

        /// и отменить прерывание выполнения</remarks> 

        public void ResetInterrupt() 

        { 

            _toggleActivity = null; 

            _interruptExecutionEvent.Reset(); 

            _interruptExecutionFinishedEvent.Set(); 

        } 

 

 

        /// <summary> 

        /// Происходит ли в данный момент прерывание выполнения потока работ 

        /// </summary> 

        /// <returns></returns> 

        public bool IsExecutionInterrupting() 

        { 

            return _interruptExecutionEvent.WaitOne(0); 

        } 

 

 

        /// <summary> 

        /// Выполнение действия было прервано 

        /// </summary> 

        /// <param name="ex">исключение о прерывании выполнения действия</param> 

        internal void ActivityExecutionInterrupted(ActivityExecutionInterruptException ex) 

        { 

            // если действие, на которое нужно переключить выполнение не задано 

            if (_toggleActivity == null) 

                // то значит это просто прерывание выполнения => пропускаем исключение 

                throw ex; 

 

 

            // иначе - это переключение выполнения 

 

 

            // снимаем с вершины стека выполняемых действий имена действий до тех пор, 

            // пока на вершине не останется действие, которое является родительским для _toggleActivity, 

            // или пока стек не опустеет, если родительского действия нет 

 

 

            // если стек еще не готов 

            if (!IsStackCorrectForExecuteActivity(_toggleActivity)) 

                // то удаляем вершину со стека 

                ExecutingActivitiesStackPop(); 

 

 

            // в любом случае пропускаем исключение, т.к. в итоге нам нужно вывалиться внутри составного действия 

            throw ex; 

        } 


 
 

        /// <summary> 

        /// Проверяет - находится ли стек в состоянии, когда можно перейти к выполнению заданного действия 

        /// </summary> 

        /// <returns></returns> 

        private bool IsStackCorrectForExecuteActivity(Activity nextExecutingActivity) 

        { 

            // состояние стека годится, если 

            return 

                // он пуст 

                _executingActivitiesStack.Count == 0 || 

                // или на вершине - имя родительского состояния 

                (nextExecutingActivity.Parent != null && 

                _executingActivitiesStack.Peek() == nextExecutingActivity.Parent.Name); 

        } 

 

 

        /// <summary> 

        /// Возвращает действие, к выполнению которого должно перейти составное действие после того, как 

        /// произошло прерывание выполнения текущего действия. 

        /// Если прерывание произошло в результате останова выполнения или еще рано переходить к 

        /// выполнению другого действия, то метод пропускает исключение дальше 

        /// </summary> 

        /// <returns></returns> 

        internal Activity GetToggledActivity(ActivityExecutionInterruptException ex) 

        { 

            // если действие, на которое нужно переключить выполнение не задано 

            // или задано, но стек еще не готов 

            if (_toggleActivity == null || 

                !IsStackCorrectForExecuteActivity(_toggleActivity)) 

                // то пропускаем исключение 

                throw ex; 

 

 

            var res = _toggleActivity; 

            // отменим прерывание выполнения, чтобы выполнение продолжилось далее 

            ResetInterrupt(); 

 

 

            // выставим режим отслеживания в положение, в котором он мог бы оказаться, 

            // если бы выполнение потока работ дошло до действия, к выполнению которого переходим, 

            // "естесственным" путем, т.е. без переключения выполнения. 

            Tracking = GetTrackingBeforeExecuteActivity(res); 

 

 

            return res; 

        } 

 

 


        /// <summary> 

        /// Вычисляет значение включенности режима отслеживания, которое  

        /// должно быть перед началом выполнения заданного действия 

        /// </summary> 

        /// <param name="activity"></param> 

        /// <returns></returns> 

        private bool GetTrackingBeforeExecuteActivity(Activity activity) 

        { 

            // если у действия есть родительские действия,  

            // то вычислим значения режима на основании его включенности у родительских действиях 

            var tmp = activity; 

            while (tmp.Parent != null) 

            { 

                if (!tmp.Parent.Tracking) 

                    return false; 

 

 

                tmp = tmp.Parent; 

            } 

 

 

            // если родительских действий нет, то вернем режим по умолчанию,  

            // иначе - режим корневого родителя 

            return tmp.Equals(activity) ? DEFAULT_TRACKING : tmp.Tracking; 

        } 

 

 

        #endregion 

 

 

        #region IWaitController Members 

 

 

        /// <summary> 

        /// Приостановить выполнение действия на заданный тайм-аут 

        /// </summary> 

        /// <param name="timeout"></param> 

        public void Sleep(TimeSpan timeout) 

        { 

            Sleep(Convert.ToInt32(timeout.TotalMilliseconds)); 

        } 

 

 

        /// <summary> 

        /// Приостановить выполнение действия на заданный тайм-аут 

        /// </summary> 

        /// <param name="millisecondsTimeout"></param> 

        public void Sleep(int millisecondsTimeout) 

        { 

            if (_interruptExecutionEvent.WaitOne(millisecondsTimeout, false)) 


                // произошло прерывание выполнения 

                throw new ActivityExecutionInterruptException(CurrentExecutingActivity); 

        } 

 

 

        /// <summary> 

        /// Ожидать бесконечно заданное событие 

        /// </summary> 

        /// <param name="waitHandle"></param> 

        public void WaitOne(WaitHandle waitHandle) 

        { 

            WaitAny(new[] { waitHandle }); 

        } 

 

 

        /// <summary> 

        /// Ожидать заданное событие в течение таймаута 

        /// </summary> 

        /// <param name="waitHandle"></param> 

        /// <param name="timeout"></param> 

        /// <returns>true - произошло событие, false - время таймаута истекло</returns> 

        public bool WaitOne(WaitHandle waitHandle, TimeSpan timeout) 

        { 

            return WaitAny(new[] { waitHandle }, timeout) != WaitHandle.WaitTimeout; 

        } 

 

 

        /// <summary> 

        /// Ожидать любое из заданных событий 

        /// </summary> 

        /// <param name="waitHandles"></param> 

        /// <returns></returns> 

        public int WaitAny(WaitHandle[] waitHandles) 

        { 

            var waitHandlesEx = new List<WaitHandle>(waitHandles); 

            waitHandlesEx.Add(_interruptExecutionEvent); 

 

 

            var index = WaitHandle.WaitAny(waitHandlesEx.ToArray()); 

 

 

            // произошло прерывание выполнения 

            if (index == waitHandles.Length) 

                throw new ActivityExecutionInterruptException(CurrentExecutingActivity); 

 

 

            return index; 

        } 

 

 


        /// <summary> 

        /// Ожидать любое из заданных событий в течение заданного тайм-аута 

        /// </summary> 

        /// <param name="waitHandles"></param> 

        /// <param name="timeout"></param> 

        /// <returns></returns> 

        public int WaitAny(WaitHandle[] waitHandles, TimeSpan timeout) 

        { 

            return WaitAny(waitHandles, Convert.ToInt32(timeout.TotalMilliseconds)); 

        } 

 

 

        /// <summary> 

        /// Ожидать любое из заданных событий в течение заданного тайм-аута 

        /// </summary> 

        /// <param name="waitHandles"></param> 

        /// <param name="timeout"></param> 

        /// <returns></returns> 

        public int WaitAny(WaitHandle[] waitHandles, int timeout) 

        { 

            var waitHandlesEx = new List<WaitHandle>(waitHandles); 

            waitHandlesEx.Add(_interruptExecutionEvent); 

 

 

            var index = WaitHandle.WaitAny(waitHandlesEx.ToArray(), timeout, false); 

 

 

            // произошло прерывание выполнения 

            if (index == waitHandles.Length) 

                throw new ActivityExecutionInterruptException(CurrentExecutingActivity); 

 

 

            return index; 

        } 

 

 

        #endregion 

 

 

        #region Отслеживание стека выполнения действий 

 

 

        /// <summary> 

        /// Стек имен действий, которые выполняются 

        /// </summary> 

        private ListStack<string> _executingActivitiesStack = new ListStack<string>(); 

        /// <summary> 

        /// Стек имен действий, выполнение которых отслеживается 

        /// </summary> 

        private ListStack<string> _trackingActivitiesStack = new ListStack<string>(); 


        /// <summary> 

        /// Кол-во действий, выполнение которых уже восстановилось 

        /// </summary> 

        /// <remarks> 

        /// Суть этого счетчика - кол-во элементов в _executingActivities от начала, начало выполнения 

        /// которых мы снова зафиксировали 

        /// </remarks> 

        private int _restoredActivitiesCount = 0; 

 

 

        [NonSerialized] 

        private bool _restoring = false; 

        /// <summary> 

        /// Признак того, что контекст находится в режиме восстановления работы 

        /// </summary> 

        public bool Restoring 

        { 

            get 

            { 

                return _restoring; 

            } 

        } 

 

 

        /// <summary> 

        /// Действие, которое в данный момент выполняется 

        /// </summary> 

        public Activity CurrentExecutingActivity 

        { 

            get 

            { 

                return _executingActivitiesStack.Count == 0 

                    ? null : Scheme.Activities[_executingActivitiesStack.Peek()]; 

            } 

        } 

 

 

        /// <summary> 

        /// Адаптирует стек выполняемых действий так, чтобы он был корректным для действия, 

        /// которое будет выполняться следующим 

        /// </summary> 

        /// <param name="nextExecutingActivity">действие, которое будет выполняться следующим</param> 

        private void AdaptExecutingActivitiesStackForNextExecutingActivity(Activity nextExecutingActivity) 

        { 

            // снимаем с вершины стека имена действий до тех пор, пока на вершине 

            // не отстанется действие, которое является родительским для nextExecutingActivity 

 

 

            // если для действия нет родительского 

            if (nextExecutingActivity.Parent == null) 


            { 

                // то просто чистим стек 

                _executingActivitiesStack.Clear(); 

                _trackingActivitiesStack.Clear(); 

                return; 

            } 

 

 

            var parentActivityName = nextExecutingActivity.Parent.Name; 

 

 

            while (_executingActivitiesStack.Count > 0 && 

                string.CompareOrdinal(_executingActivitiesStack.Peek(), parentActivityName) != 0) 

            { 

                ExecutingActivitiesStackPop();     

            } 

        } 

 

 

        /// <summary> 

        /// Удаляет элемент с вершины стека выполняемых действий и при этом, 

        /// если нужно, то удаляет и соотв. элемент с вершины стека отслеж. действий 

        /// </summary> 

        private void ExecutingActivitiesStackPop() 

        { 

            var popName = _executingActivitiesStack.Pop(); 

 

 

            // если на вершине стека отслеж. действий тоже действие,  

            // которое удалили с вершины стека выполняемых действий 

            if (string.CompareOrdinal(_trackingActivitiesStack.Peek(), popName) == 0) 

                // то и с вершины стека отслеж. действий тоже удалим его 

                _trackingActivitiesStack.Pop(); 

        } 

 

 

        /// <summary> 

        /// Начать отслеживать выполнение действия 

        /// </summary> 

        private void StartActivityExecutionTracking(Activity activity) 

        { 

            _executingActivitiesStack.Push(activity.Name); 

 

 

            var eventArgs = new WorkflowExecutionContextEventArgs(this, activity); 

            ActivityExecutionStarting.RaiseEvent(this, eventArgs); 

 

 

            // если режим отслеживания состояния выключен 

            if (!Tracking) 


                // то выходим 

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

 

 

                // увеличим счетчик _restoredActivitiesCount 

                _restoredActivitiesCount++; 

 

 

                // если счетчик сравнялся с кол-вом элементов в стеке, значит восстановление завершилось 

                if (_restoredActivitiesCount == _trackingActivitiesStack.Count) 

                    _restoring = false; 

 

 

                return; 

            } 

 

 

            _trackingActivitiesStack.Push(activity.Name); 

            ExecutionContextChanged.RaiseEvent(this, eventArgs); 

        } 

 

 

        /// <summary> 

        /// Завершить отслеживать выполнение действия 

        /// </summary> 

        private void EndActivityExecutionTracking(Activity activity) 

        { 

            SafePopFromStack(_executingActivitiesStack, activity); 

 

 

            var eventArgs = new WorkflowExecutionContextEventArgs(this, activity); 

            ActivityExecutionFinished.RaiseEvent(this, eventArgs); 

 

 

            // если режим отслеживания состояния выключен 


            if (!Tracking) 

                // то ничего не делаем 

                return; 

 

 

            if (_restoring) 

                return; 

 

 

            SafePopFromStack(_trackingActivitiesStack, activity); 

            ExecutionContextChanged.RaiseEvent(this, eventArgs); 

        } 

 

 

        /// <summary> 

        /// Удаляет информацию о действии с вершины стека, при этом проверяет,  

        /// что на вершине именно это заданное действие 

        /// </summary> 

        /// <param name="activity"></param> 

        /// <returns></returns> 

        private string SafePopFromStack(ListStack<string> stack, Activity activity) 

        { 

            if (stack.Count == 0) 

                throw new ActivityExecutionException("Стек пуст", activity, this); 

 

 

            if (stack.Peek() != activity.Name) 

                throw new ActivityExecutionException( 

                    "На вершине стека другое действие: " + stack.Peek(), activity, this); 

 

 

            return stack.Pop(); 

        } 

 

 

        /// <summary> 

        /// Возвращает имя действия, к выполнению которого нужно перейти  

        /// для восстановления выполнения потока работ 

        /// </summary> 

        /// <returns></returns> 

        internal string GetActivityNameToRestore() 

        { 

            if (!_restoring) 

                throw new InvalidOperationException("Контекст не находится в режиме восстановления работы"); 

 

 

            return _trackingActivitiesStack[_restoredActivitiesCount]; 

        } 

 

 


        #endregion 

 

 

        #region Подписка на события 

 

 

        /// <summary> 

        /// Объект синхронизации доступа к Словарю обработчиков событий 

        /// </summary> 

        private static object s_eventHandlersDictSync = new object(); 

        /// <summary> 

        /// Словарь обработчиков событий: [уникальное имя события, обработчик] 

        /// </summary> 

        private Dictionary<string, EventActivityHandler> _eventHandlersDict = 

            new Dictionary<string, EventActivityHandler>(); 

 

 

        /// <summary> 

        /// Подписаться на событие 

        /// </summary> 

        /// <param name="eventHolder">держатель события</param> 

        /// <param name="handlerActivity">действие-обработчик</param> 

        /// <param name="handlingType">тип обработки события</param> 

        internal void SubscribeToEvent( 

            EventHolder eventHolder, Activity handlerActivity, EventHandlingType handlingType) 

        { 

            lock (s_eventHandlersDictSync) 

            { 

                EventActivityHandler handler; 

 

 

                // если обработчик еще не зарегистрирован 

                if (!_eventHandlersDict.ContainsKey(eventHolder.EventName)) 

                { 

                    // создадим его 

                    handler = new EventActivityHandler(_workflowInstance); 

                    _eventHandlersDict[eventHolder.EventName] = handler; 

                    eventHolder.AddEventHandler(handler.Method); 

                } 

                else 

                    // получим обработчик 

                    handler = _eventHandlersDict[eventHolder.EventName]; 

 

 

                // добавим действие-обработчик 

                handler.AddActivity(handlerActivity, handlingType); 

            } 

        } 

 

 


        /// <summary> 

        /// Отписаться от события 

        /// </summary> 

        /// <param name="eventHolder">держатель события</param> 

        /// <param name="handlerActivity">действие-обработчик</param> 

        internal void UnsubscribeFromEvent(EventHolder eventHolder, Activity handlerActivity) 

        { 

            lock (s_eventHandlersDictSync) 

            { 

                // если обработчик не зарегистрирован 

                if (!_eventHandlersDict.ContainsKey(eventHolder.EventName)) 

                    return; 

 

 

                // получим обработчик 

                var handler = _eventHandlersDict[eventHolder.EventName]; 

 

 

                // удалим действие из обработчика 

                handler.RemoveActivity(handlerActivity); 

 

 

                // если действий в обработчике не осталось 

                if (!handler.ContainsActivities) 

                { 

                    eventHolder.RemoveEventHandler(handler.Method); 

                    _eventHandlersDict.Remove(eventHolder.EventName); 

                } 

            } 

        } 

 

 

        #endregion 

 

 

        #region Мониторинг 

 

 

        /// <summary> 

        /// Включить монитор 

        /// </summary> 

        /// <param name="lockName">имя блокировки</param> 

        internal void MonitorEnter(string lockName) 

        { 

            // TODO: сделать 

        } 

 

 

        /// <summary> 

        /// Выключить монитор 


        /// </summary> 

        /// <param name="lockName">имя блокировки</param> 

        internal void MonitorExit(string lockName) 

        { 

            // TODO: сделать 

        } 

 

 

        #endregion 

 

 

        #region Сериализация 

 

 

        /// <summary> 

        /// Возвращает состояние контекста 

        /// </summary> 

        /// <returns></returns> 

        public object GetState() 

        { 

            return new object[] {  

                Scheme,  

                _trackingActivitiesStack, 

                _eventHandlersDict 

            }; 

        } 

 

 

        /// <summary> 

        /// Сохраняет состояние контекста в поток 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="stream"></param> 

        public static void Save(WorkflowExecutionContext context, Stream stream) 

        { 

            s_formatter.Serialize(stream, context.GetState()); 

        } 

 

 

        /// <summary> 

        /// Восстанавливает состояние контекста из потока 

        /// </summary> 

        /// <param name="stream"></param> 

        /// <returns></returns> 

        public static WorkflowExecutionContext Load(Stream stream) 

        { 

            var arr = (object[])s_formatter.Deserialize(stream); 

 

 

            // восстановим контекст 


            var context = new WorkflowExecutionContext((WorkflowScheme)arr[0]) 

            { 

                _trackingActivitiesStack = (ListStack<string>)arr[1], 

                _eventHandlersDict = (Dictionary<string, EventActivityHandler>)arr[2] 

            }; 

            // установим признак того, что выполнение восстанавливается 

            context._restoring = (context._trackingActivitiesStack.Count > 0); 

 

 

            return context; 

        } 

 

 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Первое действие с конца стека не являющееся Common 

        /// </summary> 

        /// <returns>Имя действия</returns> 

        public string GetFirstNotCommonActivityFromStack() 

        { 

            return _executingActivitiesStack.Last(s => !s.StartsWith("Common")); 

        } 

 

 

        public override string ToString() 

        { 

            return _executingActivitiesStack.ToString(); 

        } 

    } 

}



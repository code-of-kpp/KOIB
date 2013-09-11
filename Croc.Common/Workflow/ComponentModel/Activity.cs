using System; 

using System.Collections.Generic; 

using Croc.Core.Utils.Collections; 

 

 

namespace Croc.Workflow.ComponentModel 

{ 

    /// <summary> 

    /// Действие 

    /// </summary> 

    [Serializable] 

    public class Activity : INamed 

    { 

        #region Свойства 

        /// <summary> 

        /// Проинициализировано ли данное действие, т.е. вызывался ли для него метод Initialize 

        /// </summary> 

        [NonSerialized] 

        private bool _initialized; 

 

 

        /// <summary> 

        /// Имя действия 

        /// </summary> 

        public string Name 

        { 

            get; 

            set; 

        } 

 

 

        /// <summary> 

        /// Вызыватель метода, который реализует инициализацию действия 

        /// </summary> 

        internal ActivityUnInitializeMethodCaller InitializeMethodCaller 

        { 

            get; 

            set; 

        } 

 

 

        /// <summary> 

        /// Вызыватель метода, который реализует деинициализацию действия 

        /// </summary> 

        internal ActivityUnInitializeMethodCaller UninitializeMethodCaller 

        { 

            get; 

            set; 

        } 

 


 
        /// <summary> 

        /// Вызыватель метода, который реализует логику действия 

        /// </summary> 

        internal ActivityExecutionMethodCaller ExecutionMethodCaller 

        { 

            get; 

            set; 

        } 

 

 

        /// <summary> 

        /// Параметры действия 

        /// </summary> 

        public ActivityParameterDictionary Parameters 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Следующие действия, к выполнению которых может быть выполнен переход 

        /// </summary> 

        public Dictionary<NextActivityKey, Activity> NextActivities 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Действие, которое идет следом за текущим в схеме потока работ 

        /// </summary> 

        public Activity FollowingActivity 

        { 

            get; 

            set; 

        } 

 

 

        /// <summary> 

        /// Родительское действие 

        /// </summary> 

        public Activity Parent 

        { 

            get; 

            set; 

        } 

 


 
        /// <summary> 

        /// Родительское действие, которое находится в корне иерархии 

        /// </summary> 

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

 

 

        /// <summary> 

        /// Включен ли режим отслеживания состояния 

        /// </summary> 

        internal bool Tracking 

        { 

            get; 

            set; 

        } 

 

 

        /// <summary> 

        /// Приоритет 

        /// </summary> 

        public ActivityPriority Priority 

        { 

            get; 

            set; 

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

 

 

        /// <summary> 

        /// Инициализация действия 

        /// </summary> 

        /// <remarks>Метод вызывается исполняющей средой перед началом выполнения действия или при первом обращении к  

        /// вложенному действию для данного составного действия</remarks> 

        /// <param name="context"></param> 

        private void _Initialize(WorkflowExecutionContext context) 

        { 

            if (!_initialized) 

            { 

                Initialize(context); 

                _initialized = true; 

            } 

        } 

 

 

        /// <summary> 

        /// Деинициализация действия 

        /// </summary> 

        /// <remarks>Метод вызывается исполняющей средой после окончания выполнения действия.  

        /// В случае, если инициализация была вызвана при первом обращении к вложенному действию данного составного 

        /// действия, то деинициализация не будет вызываться</remarks> 

        /// <param name="context"></param> 

        private void _Uninitialize(WorkflowExecutionContext context) 

        { 

            if (_initialized) 

            { 

                Uninitialize(context); 

                _initialized = false; 

            } 

        } 

 

 

        /// <summary> 

        /// Инициализация действия 

        /// </summary> 

        /// <remarks>Метод вызывается исполняющей средой перед началом выполнения действия</remarks> 

        /// <param name="context"></param> 

        protected virtual void Initialize(WorkflowExecutionContext context) 

        { 

            if (InitializeMethodCaller != null) 

                InitializeMethodCaller.Call(context); 


        } 

 

 

        /// <summary> 

        /// Деинициализация действия 

        /// </summary> 

        /// <remarks>Метод вызывается исполняющей средой после окончания выполнения действия</remarks> 

        /// <param name="context"></param> 

        protected virtual void Uninitialize(WorkflowExecutionContext context) 

        { 

            if (UninitializeMethodCaller != null) 

                UninitializeMethodCaller.Call(context); 

        } 

 

 

        #endregion 

 

 

        #region Выполнение действия 

 

 

        /// <summary> 

        /// Выполнить действие с параметрами, которые хранятся в самом действие (т.е. с параметрами, 

        /// которые были записаны в действие при разборе схемы потока работ) 

        /// </summary> 

        /// <param name="context"></param> 

        /// <returns></returns> 

        internal NextActivityKey Execute(WorkflowExecutionContext context) 

        { 

            return _Execute(context, this.Parameters); 

        } 

 

 

        /// <summary> 

        /// Выполнить действие с заданными параметрами 

        /// </summary> 

        /// <remarks> 

        /// Дело в том, что действие уже может содержать параметры, но, скажем так, статические,  

        /// значения которых были получены в момент загрузки схемы. Но при вызове действия, например, через  

        /// действие-ссылку, могут быть переданы другие параметры с другими значениями. 

        /// Этот метод сначала формирует список параметров, который будет передан в метод выполнения действия. 

        /// Список формируется так: берутся execParameters-ы и дополняются теми статическими параметрами, 

        /// которых нет среди execParameters-ов. 

        /// </remarks> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        internal NextActivityKey Execute(WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            var execParameters = new ActivityParameterDictionary(); 


 
 

            // добавим переданные параметры 

            foreach (var param in parameters.Values) 

                execParameters.Add(param); 

 

 

            // добавим внутренние параметры, которых нет среди переданных 

            foreach (var param in this.Parameters) 

            { 

                if (!execParameters.ContainsKey(param.Name)) 

                    execParameters.Add(param); 

            } 

 

 

            return _Execute(context, execParameters); 

        } 

 

 

        private NextActivityKey _Execute(WorkflowExecutionContext context, ActivityParameterDictionary execParameters) 

        { 

            // если данное действие имеет родительское (т.е. данное действие входит в составное действие) 

            // и это родительское действие еще не проинициализировано 

            if (Parent != null && !Parent._initialized) 

            { 

                // то инициализируем его 

                Parent._Initialize(context); 

            } 

 

 

            // инициализируем действие 

            _Initialize(context); 

 

 

            // запомним, каким был режим у контекста 

            var originalСontextTracking = context.Tracking; 

            // если для данного действия режим отслеживания выключен 

            if (!Tracking) 

                // то выключим его для контекста на время выполнения данного действия 

                context.Tracking = false; 

 

 

            // запомним, каким был приоритет у контекста 

            var originalСontextPriority = context.Priority; 

            // если приоритет действия выше текущего приоритета контекста 

            if (Priority > originalСontextPriority) 

                // то повысим приоритет всего контекста 

                context.Priority = Priority; 

 

 


            // выполняем действие 

            NextActivityKey res = null; 

            try 

            { 

                // говорим контексту, что начинаем выполнять действие 

                context.ActivityExecuting(this); 

 

 

                // вызываем метод выполнения действия 

                res = ExecutionMethodCaller.Call(context, execParameters); 

 

 

                // говорим контексту, что закончили выполнять действие 

                context.ActivityExecuted(this); 

            } 

            // выполнение было прервано 

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

                // восстановим режим отслеживания у контекста 

                context.Tracking = originalСontextTracking; 

                // восстановим приориет контекста 

                context.Priority = originalСontextPriority; 

 

 

                // деинициализируем действие 

                _Uninitialize(context); 

            } 

 

 

            return res; 

        } 

 

 

        #endregion  

 

 

        public override string ToString() 

        { 


            return this.Name; 

        } 

    } 

}



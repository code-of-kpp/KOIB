using System; 

using System.Collections.Generic; 

using System.IO; 

using System.Runtime.Serialization; 

using System.Runtime.Serialization.Formatters.Binary; 

using System.Text; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Bpc.FileSystem; 

using Croc.Bpc.Keyboard; 

using Croc.Bpc.Synchronization; 

using Croc.Bpc.Workflow.Config; 

using Croc.Core; 

using Croc.Core.Configuration; 

using Croc.Core.Diagnostics; 

using Croc.Core.Extensions; 

using Croc.Workflow.Runtime; 

 

 

namespace Croc.Bpc.Workflow 

{ 

    /// <summary> 

    /// Менеджер рабочих потоков 

    /// </summary> 

    [SubsystemConfigurationElementTypeAttribute(typeof(WorkflowManagerConfig))] 

    public sealed class WorkflowManager : StateSubsystem, IWorkflowManager 

    { 

        private static Guid WorkflowInstanceId = new Guid("33444D0A-2D7D-4ad9-BE75-DEEF50E4C9A0"); 

 

 

        /// <summary> 

        /// Конфиг менеджера потока работ 

        /// </summary> 

        private WorkflowManagerConfig _config; 

        /// <summary> 

        /// Исполняющая среда потока работ 

        /// </summary> 

        private WorkflowRuntime _runtime; 

        /// <summary> 

        /// Основной поток работ 

        /// </summary> 

        private WorkflowInstance _mainWorkflowInstance; 

        /// <summary> 

        /// Менеджер синхронизации 

        /// </summary> 

        private ISynchronizationManager _syncManager; 

		/// <summary> 

		/// Менеджер файловой системы 

		/// </summary> 

		private IFileSystemManager _fileSystemManager; 

 


 
        /// <summary> 

        /// Логгер действий пользователя 

        /// </summary> 

        private ILogger _userActionLogger; 

 

 

        #region Инициализация 

 

 

        /// <summary> 

        /// Инициализация подсистемы 

        /// </summary> 

        /// <param name="config"></param> 

        public override void Init(SubsystemConfig config) 

        { 

            _config = (WorkflowManagerConfig)config; 

 

 

            // получим ссылки на другие подсистемы 

            _syncManager = Application.GetSubsystemOrThrow<ISynchronizationManager>(); 

			_fileSystemManager = Application.GetSubsystemOrThrow<IFileSystemManager>(); 

 

 

            // создаем логгер действий пользователя 

            _userActionLogger = Application.CreateLogger("UserAction", System.Diagnostics.TraceLevel.Info); 

            SubscribeToUserActions(); 

 

 

			// загрузим счетчик ошибок 

			LoadErrorState(); 

            // инициализируем поток работ 

            InitWorkflow(); 

        } 

 

 

        #region Логирование действий пользователя 

 

 

        /// <summary> 

        /// Подпишемся на события действий пользователя  

        /// </summary> 

        private void SubscribeToUserActions() 

        { 

            // событие нажатия клавиш 

            var keyboard = (IKeyboard)Application.GetSubsystemOrThrow<UnionKeyboard>(); 

            keyboard.KeyPressed += new EventHandler<KeyEventArgs>(LogUserKeyPressed); 

 

 

            // событие изменения настроек сканера 


			Application.ConfigUpdated +=new EventHandler<ConfigUpdatedEventArgs>(LogApplicationConfigUpdated); 

 

 

            // смена потока работ 

            StateChanged += new EventHandler<SubsystemStateEventArgs>(LogStateChanged); 

        } 

 

 

        /// <summary> 

        /// Кнопки нажатые во время выполнения одной активности 

        /// </summary> 

        private SortedList<long, KeyEventArgs> _pressedKeys = new SortedList<long, KeyEventArgs>(); 

 

 

        /// <summary> 

        /// Объект синхронизации занесения нажатых клавиш в  список 

        /// </summary> 

        private static object s_pressedKeysSync = new object(); 

 

 

        /// <summary> 

        /// Логируем нажатие на клавиши 

        /// </summary> 

        /// <param name="sender"></param> 

        /// <param name="e"></param> 

        private void LogUserKeyPressed(object sender, KeyEventArgs e) 

        { 

            // добавим в словарь 

            var timeKey = DateTime.Now.ToBinary(); 

            lock (s_pressedKeysSync) 

            { 

                if (_pressedKeys.ContainsKey(timeKey)) 

                { 

                    // теоретически клавиши могли нажать в одно время 

                    timeKey = DateTime.Now.AddMilliseconds(1).ToBinary(); 

                } 

                _pressedKeys.Add(timeKey, e); 

            } 

 

 

            // если это выход залогируем все, что осталось 

            if (e.Type == KeyType.Quit || e.Type == KeyType.PowerOff) 

            { 

                LogActivityKeys(); 

            } 

        } 

 

 

        /// <summary> 

        /// Логируем все ключи нажатые в рамках одного действия 


        /// </summary> 

        private void LogActivityKeys() 

        { 

            // текущий контекст выполнения 

            var currentContext = _mainWorkflowInstance.ExecutionContext.GetFirstNotCommonActivityFromStack(); 

 

 

            StringBuilder keysStr = new StringBuilder(); 

            // время нажатия клавиши 

            var latestTime = DateTime.MinValue.TimeOfDay; 

 

 

            lock (s_pressedKeysSync) 

            { 

                // сформируем строку с нажатыми клавишами 

                foreach (var pair in _pressedKeys) 

                { 

                    TimeSpan time = DateTime.FromBinary(pair.Key).TimeOfDay; 

                    // если время нажатия между клавишами более минуты 

                    if (time - latestTime >= new TimeSpan(0, 1, 0)) 

                    { 

                        keysStr.Append(DateTime.FromBinary(pair.Key).ToShortTimeString() + " "); 

                        latestTime = time; 

                    } 

                    // если это число, то выведем его значение 

                    if (pair.Value.Type == KeyType.Digit) 

                    { 

                        keysStr.AppendFormat("{0}={1} ", pair.Value.Type, pair.Value.Value); 

                        continue; 

                    } 

                    keysStr.Append(pair.Value.Type + " "); 

                } 

 

 

                // очистим словарь 

                _pressedKeys.Clear(); 

            } 

 

 

            // логируем сообщение 

            _userActionLogger.LogInfo(Message.UserKeyPressed 

                , _stateActivityName != null ? _stateActivityName : "<начальное состояние>"  

                , currentContext 

                , keysStr.ToString().Trim()); 

        } 

 

 

        /// <summary> 

        /// Логируем изменение состояния потока работ 

        /// </summary> 


        /// <param name="sender"></param> 

        /// <param name="e"></param> 

        private void LogStateChanged(object sender, SubsystemStateEventArgs e) 

        { 

            _userActionLogger.LogInfo(Message.WorkflowStateChanged, e.State); 

        } 

 

 

		/// <summary> 

		/// Логируем изменение параметров конфигурации 

		/// </summary> 

		/// <param name="sender"></param> 

		/// <param name="e"></param> 

		private void LogApplicationConfigUpdated(object sender, ConfigUpdatedEventArgs e) 

		{ 

			_userActionLogger.LogInfo 

				(Message.ParameterUpdated, e.UpdatedParameterName, e.SubsystemName, e.OldValue, e.NewValue); 

		} 

 

 

        #endregion 

 

 

        /// <summary> 

        /// Инициализация потока работ 

        /// </summary> 

        private void InitWorkflow() 

        { 

            // инициализация исполняющей среды 

            _runtime = new WorkflowRuntime(); 

 

 

            // подписываемся на события 

            _runtime.WorkflowCompleted += 

                delegate(object sender, WorkflowCompletedEventArgs e) 

                { 

                    Logger.LogInfo(Message.WorkflowThreadStopped, e.Result != null ? ". Результат: " + e.Result : ""); 

                    WorkflowStopped.RaiseEvent(this); 

                }; 

            _runtime.WorkflowTerminated += 

                delegate(object sender, WorkflowTerminatedEventArgs e) 

                { 

                    Logger.LogException(Message.WorkflowThreadException, e.Exception); 

                    WorkflowStopped.RaiseEvent(this); 

                }; 

 

 

            // создаем экземпляр потока работ 

            _mainWorkflowInstance = _runtime.CreateWorkflow( 

                WorkflowInstanceId, 


                _config.WorkflowScheme.Uri, 

                _config.WorkflowScheme.XmlSchemas.ToList()); 

 

 

            // подписываемся на события 

            _mainWorkflowInstance.ExecutionContext.ActivityExecutionStarting +=  

                new EventHandler<WorkflowExecutionContextEventArgs>(ExecutionContext_ActivityExecutionStarting); 

            _mainWorkflowInstance.ExecutionContext.ActivityExecutionFinished +=  

                new EventHandler<WorkflowExecutionContextEventArgs>(ExecutionContext_ActivityExecutionFinished); 

        } 

 

 

        #endregion 

 

 

        #region Отслеживание состояние потока работ 

 

 

        /// <summary> 

        /// Имя действия, которое определяет текущее состояние менеджера потока работ 

        /// </summary> 

        private string _stateActivityName; 

 

 

        /// <summary> 

        /// Обработчик события "Действие начинает выполняться" 

        /// </summary> 

        /// <param name="sender"></param> 

        /// <param name="e"></param> 

        private void ExecutionContext_ActivityExecutionStarting(object sender, WorkflowExecutionContextEventArgs e) 

        { 

            Logger.LogVerbose(Message.WorkflowActivityExecutionStarting, e.Activity.Name); 

 

 

            // если 

            if (// это главный сканер 

                _syncManager.ScannerRole == ScannerRole.Master && 

                // и это начинает выполняться контрольное действие  

                // (только для контрольных действий режим отслеживания будет включен) 

                e.Context.Tracking) 

            { 

                // изменим имя действия-состояния 

                _stateActivityName = e.Activity.Name; 

                // сообщим, что состояние изменилось 

                RaiseStateChanged(); 

            } 

        } 

 

 

        /// <summary> 


        /// Обработчик события "Выполнение действия завершено" 

        /// </summary> 

        /// <param name="sender"></param> 

        /// <param name="e"></param> 

        private void ExecutionContext_ActivityExecutionFinished(object sender, WorkflowExecutionContextEventArgs e) 

        { 

            // сбросим в лог все, что нажал пользователь 

            if (_pressedKeys.Count > 0) 

                LogActivityKeys(); 

 

 

            Logger.LogVerbose(Message.WorkflowActivityExecutionFinished, e.Activity.Name); 

        } 

 

 

        #endregion 

 

 

        #region IWorkflowManager 

 

 

        /// <summary> 

        /// Поток работ завершил выполнение 

        /// </summary> 

        public event EventHandler WorkflowStopped; 

 

 

        /// <summary> 

        /// Запустить поток работ 

        /// </summary> 

        public void StartWorkflow() 

        { 

            _mainWorkflowInstance.Start(); 

        } 

 

 

        /// <summary> 

        /// Перейти к действию, которое определяет состояние потока работ 

        /// </summary> 

        public void GoToStateActivity(bool sync) 

        { 

            var context = _mainWorkflowInstance.ExecutionContext; 

 

 

            // если действие состояния не определено или определено,  

            // но текущее выполняемое действие и так равно действию состояния 

            if (_stateActivityName == null || 

                (context.CurrentExecutingActivity != null && 

                context.CurrentExecutingActivity.Name.Equals(_stateActivityName))) 

                // то ничего не делаем 


                return; 

 

 

            // переходим к другому действию 

            GoToActivity(_stateActivityName, sync); 

        } 

 

 

        /// <summary> 

        /// Перейти к действию с заданным именем 

        /// </summary> 

        public void GoToActivity(string activityName, bool sync) 

        { 

            _mainWorkflowInstance.GoToActivity(activityName, sync); 

        } 

 

 

        #region Счетчики ошибок 

 

 

		/// <summary> 

		/// Имя файла с ошибками 

		/// </summary> 

		private const string ERROR_FILE_NAME = "wf_errors.bin"; 

 

 

		/// <summary> 

		/// Форматер для сериализации ошибок 

		/// </summary> 

		private IFormatter _errorCountersSerializationFormatter = new BinaryFormatter(); 

 

 

        /// <summary> 

        /// Объект синхронизации доступа к счетчикам ошибок 

        /// </summary> 

        private static object s_errorCountersSync = new object(); 

 

 

		/// <summary> 

        /// Счетчики кол-ва возникновения ошибок:  

        /// [код ошибки String, кол-во раз возникновения ошибки Int32] 

        /// </summary> 

		private Dictionary<string, Int32> _errorCounters = new Dictionary<string, Int32>(); 

 

 

		/// <summary> 

		/// Путь к сериализуемому файлу ошибок  

		/// </summary> 

		private string _errorCounterFileName; 

 


 
		/// <summary> 

		/// Путь к сериализуемому файлу ошибок  

		/// </summary> 

		private string ErrorCounterFileName 

		{ 

			get  

			{ 

				if (!String.IsNullOrEmpty(_errorCounterFileName)) 

				{ 

					return _errorCounterFileName; 

				} 

 

 

				// получим путь к файлу ошибок 

				var runtimeDirPath = _fileSystemManager.GetDataDirectoryPath(FileType.RuntimeData); 

				_errorCounterFileName = Path.Combine(runtimeDirPath, ERROR_FILE_NAME); 

 

 

				return _errorCounterFileName; 

			} 

		} 

 

 

        /// <summary> 

        /// Увеличить кол-во возникновения ошибки и возвращает увеличенное значение 

        /// </summary> 

        /// <param name="errorId">идентификатор ошибки</param> 

        public int IncreaseErrorCounter(string errorId) 

        { 

            lock (s_errorCountersSync) 

            { 

                if (!_errorCounters.ContainsKey(errorId)) 

                    _errorCounters[errorId] = 1; 

                else 

                    _errorCounters[errorId] = _errorCounters[errorId] + 1; 

 

 

                // дергаем сохранение счетчика ошибок 

				SaveErrorState(); 

 

 

                return _errorCounters[errorId]; 

            } 

        } 

 

 

        /// <summary> 

        /// Сбросить счетчики ошибок 

        /// </summary> 


        public void ResetErrorCounters() 

        { 

            lock (s_errorCountersSync) 

            { 

                _errorCounters.Clear(); 

 

 

                // дергаем сохранение счетчика ошибок 

                SaveErrorState(); 

            } 

		} 

 

 

		/// <summary> 

		/// Сбросить определенный счетчик ошибок 

		/// </summary> 

		/// <param name="errorid">идентефикатор сбрасываемой ошибки</param> 

		public void ResetErrorCounter(string errorid) 

		{ 

			lock (s_errorCountersSync) 

			{ 

				_errorCounters.Remove(errorid); 

 

 

                // дергаем сохранение счетчика ошибок 

                SaveErrorState(); 

			} 

		} 

 

 

		/// <summary> 

		/// Сохраняет счетчик ошибок в файл 

		/// </summary> 

		private void SaveErrorState() 

		{ 

			try 

			{ 

                if(!_fileSystemManager.SafeSerialization(FileType.RuntimeData, _errorCounters, 

                    _errorCountersSerializationFormatter, ErrorCounterFileName)) 

                { 

                    Logger.LogWarning(Message.WorkflowErrorCounterSerializationFailed); 

                } 

            } 

			catch (Exception ex) 

			{ 

                Logger.LogException(Message.WorkflowErrorCounterSerializationException, ex); 

			} 

		} 

 

 


		/// <summary> 

		/// Загрузить счетчик ошибок с диска из файла ошибок 

		/// </summary> 

		public void LoadErrorState() 

		{ 

			lock (s_errorCountersSync) 

			{ 

			    FileInfo errorCounterFile = new FileInfo(ErrorCounterFileName); 

 

 

				// если файл с состоянием не найден 

				if (!errorCounterFile.Exists) 

				{ 

					// считаем, что ошибок нет 

					return; 

				} 

 

 

                if (errorCounterFile.Length <= 0) 

                { 

                    // протоколируем факт того, что файл нулевого размера 

                    Logger.LogWarning(Message.WorkflowErrorCounterHasZeroSize); 

                    try 

                    { 

                        errorCounterFile.Delete(); 

                    } 

                    catch(Exception ex) 

                    { 

                        // не смогли удалить, протоколируем 

                        Logger.LogException(Message.WorkflowErrorCounterDeleteError, ex); 

                    } 

                    return; 

                } 

 

 

				// читаем данные из файла и десериализуем счетчик ошибок 

                try 

                { 

                    using (var stream = File.Open(ErrorCounterFileName, FileMode.Open, FileAccess.Read, FileShare.Read)) 

                    { 

                        _errorCounters = 

                            (Dictionary<string, Int32>) _errorCountersSerializationFormatter.Deserialize(stream); 

                    } 

                } 

                catch(Exception ex) 

                { 

                    // повреждение этого файла не является аварийным событием 

                    Logger.LogException(Message.WorkflowErrorCounterDeserializationError, ex); 

                } 

			} 


		} 

 

 

        #endregion 

 

 

        #endregion 

 

 

        #region StateSubsystem overrides 

 

 

        /// <summary> 

        /// Получить состояние подсистемы 

        /// </summary> 

        /// <returns></returns> 

        public override object GetState() 

        { 

            return _stateActivityName; 

        } 

 

 

        /// <summary> 

        /// Восстановить состояние 

        /// </summary> 

        /// <param name="state"></param> 

        public override void RestoreState(object state) 

        { 

            // если состояние не задано 

            if (state == null) 

                // то просто ничего не делаем 

                return; 

 

 

            // имя действия-состояния 

            _stateActivityName = (string)state; 

        } 

 

 

        /// <summary> 

        /// Принять новое состояние 

        /// </summary> 

        /// <param name="newState">новое состояние</param> 

        /// <returns>результат принятия нового состояния подсистемой</returns> 

        public override SubsystemStateAcceptanceResult AcceptNewState(object newState) 

        { 

            var newStateActivityName = (string)newState; 

 

 

            // вычислим порядковые номера текущего действия-состояния и нового 


            var currentOrderNum = GetActivityOrderNumber(_stateActivityName); 

            var newOrderNum = GetActivityOrderNumber(newStateActivityName); 

 

 

            // если текущий порядковый номер больше, чем новый 

            if (currentOrderNum > newOrderNum) 

                // то отклоняем новое состояние  

                return SubsystemStateAcceptanceResult.Rejected; 

 

 

            // иначе - принимаем 

            _stateActivityName = newStateActivityName; 

            GoToStateActivity(false); 

 

 

            return SubsystemStateAcceptanceResult.Accepted; 

        } 

 

 

        /// <summary> 

        /// Возвращает порядковый номер действия по его имени 

        /// </summary> 

        /// <remarks>предполагается, что имя действия имеет формат: 

        /// ИмяСоставногоДействия.N_ИмяДействия, где N - порядковый номер в 16-ричном формате</remarks> 

        /// <param name="stateActivityName"></param> 

        /// <returns>порядковый номер, начиная с 1, или  

        /// 0, если порядковый номер определить не удалось</returns> 

        private int GetActivityOrderNumber(string activityName) 

        { 

            if (string.IsNullOrEmpty(activityName)) 

                return 0; 

 

 

            var startIndex = activityName.IndexOf('.'); 

            var endIndex = activityName.IndexOf('_'); 

            if (startIndex < 0 || endIndex < 0) 

                return 0; 

 

 

            var orderNumStr = activityName.Substring(startIndex + 1, endIndex - startIndex - 1); 

            int orderNum = 0; 

            int.TryParse(orderNumStr, System.Globalization.NumberStyles.AllowHexSpecifier, null, out orderNum); 

 

 

            return orderNum; 

        } 

 

 

        /// <summary> 

        /// Сбросить состояние (перевести его в начальное положение) 


        /// </summary> 

        protected override void ResetStateInternal() 

        { 

            _stateActivityName = null; 

        } 

 

 

        #endregion 

 

 

        #region IDisposable Members 

 

 

        public override void Dispose() 

        { 

            base.Dispose(); 

 

 

            if (_runtime != null) 

                _runtime.Dispose(); 

        } 

 

 

        #endregion 

    } 

}



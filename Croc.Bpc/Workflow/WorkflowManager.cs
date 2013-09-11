using System; 
using System.Collections.Generic; 
using System.Diagnostics; 
using System.IO; 
using System.Linq; 
using System.Runtime.Serialization; 
using System.Runtime.Serialization.Formatters.Binary; 
using System.Text; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.FileSystem; 
using Croc.Bpc.Keyboard; 
using Croc.Bpc.Scanner; 
using Croc.Bpc.Synchronization; 
using Croc.Bpc.Workflow.Activities; 
using Croc.Bpc.Workflow.Config; 
using Croc.Core; 
using Croc.Core.Configuration; 
using Croc.Core.Diagnostics; 
using Croc.Core.Extensions; 
using Croc.Core.Utils.Threading; 
using Croc.Workflow.Runtime; 
namespace Croc.Bpc.Workflow 
{ 
    [SubsystemConfigurationElementTypeAttribute(typeof(WorkflowManagerConfig))] 
    public sealed class WorkflowManager : StateSubsystem, IWorkflowManager 
    { 
        private static readonly Guid s_workflowInstanceId = new Guid("33444D0A-2D7D-4ad9-BE75-DEEF50E4C9A0"); 
        private WorkflowManagerConfig _config; 
        private WorkflowRuntime _runtime; 
        private WorkflowInstance _mainWorkflowInstance; 
        private ISynchronizationManager _syncManager; 
        private IFileSystemManager _fileSystemManager; 
        private IScannerManager _scannerManager; 
        #region Инициализация 
        public override void Init(SubsystemConfig config) 
        { 
            _config = (WorkflowManagerConfig)config; 
            _syncManager = Application.GetSubsystemOrThrow<ISynchronizationManager>(); 
            _fileSystemManager = Application.GetSubsystemOrThrow<IFileSystemManager>(); 
            _scannerManager = Application.GetSubsystemOrThrow<IScannerManager>(); 
            StateChanged += (sender, e) => Logger.LogInfo(Message.WorkflowStateChanged, e.State); 
            var keyboard = (IKeyboardManager)Application.GetSubsystemOrThrow<UnionKeyboard>(); 
            keyboard.KeyPressed += LogUserKeyPressed; 
            LoadErrorState(); 
            InitWorkflow(); 
        } 
        #region Логирование нажатие клавиш пользователем 
        private readonly SortedList<long, KeyEventArgs> _pressedKeys = new SortedList<long, KeyEventArgs>(); 
        private static readonly object s_pressedKeysSync = new object(); 
        private void LogUserKeyPressed(object sender, KeyEventArgs e) 
        { 
            lock (s_pressedKeysSync) 
            { 
                var timeKey = DateTime.Now.Ticks; 
                if (_pressedKeys.ContainsKey(timeKey)) 
                { 
                    timeKey++; 
                } 
                _pressedKeys.Add(timeKey, e); 
            } 
            if (e.Type == KeyType.Quit || e.Type == KeyType.PowerOff) 
            { 
                LogActivityKeys(); 
            } 
        } 
        private void LogActivityKeys() 
        { 
            var executingActivities = _mainWorkflowInstance.ExecutionContext.CurrentExecutingActivities(); 
            var currentContext = executingActivities.LastOrDefault(s => !s.StartsWith("Common")); 
            string keysStr; 
            lock (s_pressedKeysSync) 
            { 
                var keysSb = new StringBuilder((_pressedKeys.Count + 1)*8); 
                var latestTime = DateTime.MinValue.TimeOfDay; 
                foreach (var pair in _pressedKeys) 
                { 
                    var time = new DateTime(pair.Key).TimeOfDay; 
                    if (time - latestTime >= new TimeSpan(0, 1, 0)) 
                    { 
                        keysSb.Append(DateTime.FromBinary(pair.Key).ToShortTimeString()); 
                        keysSb.Append(": "); 
                        latestTime = time; 
                    } 
                    if (pair.Value.Type == KeyType.Digit) 
                        keysSb.Append(pair.Value.Value); 
                    else 
                        keysSb.Append(pair.Value.Type); 
                    keysSb.Append(','); 
                } 
                _pressedKeys.Clear(); 
                if (keysSb.Length > 0) 
                    keysSb.Length -= 1; 
                keysStr = keysSb.ToString(); 
            } 
            Logger.LogInfo(Message.WorkflowUserKeyPressed, 
                           _stateActivityName ?? "<начальное состояние>", 
                           currentContext ?? "<не определено>", 
                           keysStr); 
        } 
        #endregion 
        private void InitWorkflow() 
        { 
            _runtime = new WorkflowRuntime(); 
            _runtime.WorkflowCompleted += 
                (sender, e) => 
                { 
                    if (e.Result != null) 
                        Logger.LogInfo(Message.WorkflowThreadStoppedWithResult, e.Result); 
                    else 
                        Logger.LogInfo(Message.WorkflowThreadStopped); 
                }; 
            _runtime.WorkflowTerminated += 
                (sender, e) => 
                { 
                    Logger.LogError(Message.WorkflowThreadTerminated, e.Reason); 
                    WorkflowTerminated.RaiseEvent(this); 
                }; 
            _mainWorkflowInstance = _runtime.CreateWorkflow( 
                s_workflowInstanceId, 
                _config.WorkflowScheme.Uri, 
                _config.WorkflowScheme.XmlSchemas.ToList()); 
            _mainWorkflowInstance.ExecutionContext.ActivityExecutionStarting += 
                ExecutionContext_ActivityExecutionStarting; 
            _mainWorkflowInstance.ExecutionContext.ActivityExecutionFinished += 
                ExecutionContext_ActivityExecutionFinished; 
            var commonActivity = (CommonActivity)_mainWorkflowInstance.ExecutionContext.Scheme.Activities 
                .First(i => i.Value is CommonActivity).Value; 
            commonActivity.InfoOutputStarting += CommonActivity_InfoOutputStarting; 
        } 
        private void CommonActivity_InfoOutputStarting(object sender, InfoOutputEventArgs e) 
        { 
            if (QuietMode && 
                (e.InfoType == InfoType.Question || e.InfoType == InfoType.Warning)) 
            { 
                QuietMode = false; 
            } 
        } 
        #endregion 
        #region Отслеживание состояние потока работ 
        private volatile string _stateActivityName; 
        private void ExecutionContext_ActivityExecutionStarting(object sender, WorkflowExecutionContextEventArgs e) 
        { 
            Logger.LogVerbose(Message.WorkflowActivityExecutionStarting, e.Activity.Name); 
            if (// это главный сканер 
                _syncManager.ScannerRole == ScannerRole.Master && 
                e.Context.Tracking) 
            { 
                _stateActivityName = e.Activity.Name; 
                RaiseStateChanged(); 
            } 
        } 
        private void ExecutionContext_ActivityExecutionFinished(object sender, WorkflowExecutionContextEventArgs e) 
        { 
            if (_pressedKeys.Count > 0) 
                LogActivityKeys(); 
            Logger.LogVerbose(Message.WorkflowActivityExecutionFinished, e.Activity.Name); 
        } 
        #endregion 
        #region IQuietMode 
        private static readonly object s_quietModeSync = new object(); 
        private bool _quietMode; 
        public bool QuietMode 
        { 
            get 
            { 
                return _quietMode; 
            } 
            set  
            { 
                lock (s_quietModeSync) 
                { 
                    _quietMode = value; 
                    var subsystems = Application.FindAllSubsystemsImplementsInterface<IQuietMode>() 
                        .Where(subsystem => !subsystem.Equals(this)); 
                    foreach (var subsystem in subsystems) 
                        subsystem.QuietMode = _quietMode; 
                } 
            } 
        } 
        #endregion 
        #region IWorkflowManager 
        #region Запуск потока работ 
        public event EventHandler WorkflowTerminated; 
        public void StartWorkflow(bool quietStart) 
        { 
            QuietMode = quietStart; 
            _mainWorkflowInstance.Start(); 
        } 
        #endregion 
        #region Управление выполнением 
        public bool GoToStateActivity() 
        { 
            if (QuietMode) 
                QuietMode = false; 
            var context = _mainWorkflowInstance.ExecutionContext; 
            if ( // действие состояния не определено 
                _stateActivityName == null || 
                (context.CurrentExecutingActivity != null && 
                 string.CompareOrdinal(_stateActivityName, context.CurrentExecutingActivity.Name) == 0)) 
            { 
                return false; 
            } 
            ThreadUtils.StartBackgroundThread( 
                () => 
                    { 
                        _scannerManager.StopScanning(); 
                        GoToActivity(_stateActivityName); 
                    }); 
            return true; 
        } 
        public void GoToActivity(string activityName) 
        { 
            _mainWorkflowInstance.GoToActivity(activityName); 
        } 
        public void SyncState() 
        { 
            RaiseStateChanged(); 
        } 
        #endregion 
        #region Счетчики ошибок 
        private const string ERROR_FILE_NAME = "wf_errors.bin"; 
        private readonly IFormatter _errorCountersSerializationFormatter = new BinaryFormatter(); 
        private static readonly object s_errorCountersSync = new object(); 
        private Dictionary<string, Int32> _errorCounters = new Dictionary<string, Int32>(); 
        private string _errorCounterFileName; 
        private string ErrorCounterFileName 
        { 
            get 
            { 
                if (!String.IsNullOrEmpty(_errorCounterFileName)) 
                { 
                    return _errorCounterFileName; 
                } 
                var runtimeDirPath = _fileSystemManager.GetDataDirectoryPath(FileType.RuntimeData); 
                _errorCounterFileName = Path.Combine(runtimeDirPath, ERROR_FILE_NAME); 
                return _errorCounterFileName; 
            } 
        } 
        public int IncreaseErrorCounter(string errorId) 
        { 
            lock (s_errorCountersSync) 
            { 
                if (!_errorCounters.ContainsKey(errorId)) 
                    _errorCounters[errorId] = 1; 
                else 
                    _errorCounters[errorId] = _errorCounters[errorId] + 1; 
                SaveErrorState(); 
                return _errorCounters[errorId]; 
            } 
        } 
        public void ResetErrorCounters() 
        { 
            lock (s_errorCountersSync) 
            { 
                _errorCounters.Clear(); 
                SaveErrorState(); 
            } 
        } 
        public void ResetErrorCounter(string errorid) 
        { 
            lock (s_errorCountersSync) 
            { 
                _errorCounters.Remove(errorid); 
                SaveErrorState(); 
            } 
        } 
        private void SaveErrorState() 
        { 
            try 
            { 
                if (!_fileSystemManager.SafeSerialization(_errorCounters, 
                    _errorCountersSerializationFormatter, ErrorCounterFileName, true, true)) 
                { 
                    Logger.LogWarning(Message.WorkflowErrorCounterSaveFailed); 
                } 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.WorkflowErrorCounterSaveError, ex); 
            } 
        } 
        public void LoadErrorState() 
        { 
            try 
            { 
                Dictionary<string, Int32> errorCounters; 
                if (!_fileSystemManager.SafeDeserialization( 
                    out errorCounters, _errorCountersSerializationFormatter, ErrorCounterFileName)) 
                { 
                    Logger.LogError(Message.WorkflowErrorCounterLoadFailed); 
                } 
                lock (s_errorCountersSync) 
                { 
                    _errorCounters = errorCounters ?? new Dictionary<string, int>(); 
                } 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.WorkflowErrorCounterLoadError, ex); 
            } 
        } 
        #endregion 
        #endregion 
        #region StateSubsystem overrides 
        public override object GetState() 
        { 
            return _stateActivityName; 
        } 
        public override void RestoreState(object state) 
        { 
            if (state == null) 
                return; 
            _stateActivityName = (string)state; 
        } 
        public override SubsystemStateAcceptanceResult AcceptNewState(object newState) 
        { 
            var newStateActivityName = (string)newState; 
            var currentOrderNum = GetActivityOrderNumber(_stateActivityName); 
            var newOrderNum = GetActivityOrderNumber(newStateActivityName); 
            if (currentOrderNum > newOrderNum) 
            { 
                Logger.LogVerbose( 
                    Message.WorkflowNewStateRejectedBecauseOfOrderLess, 
                    newStateActivityName, 
                    _stateActivityName); 
                return SubsystemStateAcceptanceResult.Rejected; 
            } 
            var newDiffersFromCurrent = (string.CompareOrdinal(_stateActivityName, newStateActivityName) != 0); 
            if (// порядковые номера совпадают 
                currentOrderNum == newOrderNum && 
                newDiffersFromCurrent && 
                _syncManager.ScannerRole == ScannerRole.Master) 
            { 
                Logger.LogVerbose( 
                    Message.WorkflowNewStateRejectedBecauseOfMasterStateHasPriority, 
                    newStateActivityName, 
                    _stateActivityName); 
                return SubsystemStateAcceptanceResult.Rejected; 
            } 
            _stateActivityName = newStateActivityName; 
            Logger.LogVerbose(Message.WorkflowNewStateAccepted, newStateActivityName, _stateActivityName); 
            if (newDiffersFromCurrent) 
                GoToStateActivity(); 
            return SubsystemStateAcceptanceResult.Accepted; 
        } 
        private int GetActivityOrderNumber(string activityName) 
        { 
            var context = _mainWorkflowInstance.ExecutionContext; 
            if (string.IsNullOrEmpty(activityName) || !context.Scheme.Activities.Contains(activityName)) 
                return 0; 
            var activity = context.Scheme.Activities[activityName]; 
            return activity.Parameters.GetParamValue("Order", 0); 
        } 
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

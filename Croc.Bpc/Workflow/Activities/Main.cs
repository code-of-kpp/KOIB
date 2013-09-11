using System; 
using System.Security.Permissions; 
using System.Threading; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Keyboard; 
using Croc.Bpc.Synchronization; 
using Croc.Bpc.Voting; 
using Croc.Core; 
using Croc.Core.Diagnostics; 
using Croc.Core.Extensions; 
using Croc.Core.Utils.Threading; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities 
{ 
    [Serializable] 
    public class MainActivity : BpcCompositeActivity 
    { 
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)] 
        protected override void Initialize(WorkflowExecutionContext context) 
        { 
            base.Initialize(context); 
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException; 
            _syncManager.RemoteScannerConnected += SyncManager_RemoteScannerConnected; 
            _syncManager.RemoteScannerDisconnected += SyncManager_RemoteScannerDisconnected; 
            _syncManager.RemoteScannerWaitForInitialization += SyncManager_RemoteScannerWaitForInitialization; 
            _syncManager.RemoteScannerExitFromMenu += SyncManager_RemoteScannerExitFromMenu; 
            StartWaitForMenuEntering(); 
        } 
        #region Обработка неотловленных исключений 
        public event EventHandler UnexpectedErrorOccurred; 
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) 
        { 
            try 
            { 
                CoreApplication.Instance.Logger.LogError(Message.Common_CriticalException, (Exception)e.ExceptionObject); 
                UnexpectedErrorOccurred.RaiseEvent(this); 
                CoreApplication.Instance.WaitForExit(); 
            } 
            catch (Exception ex) 
            { 
                try 
                { 
                    CoreApplication.Instance.Logger.LogError(Message.Common_CriticalException, ex); 
                } 
                catch 
                { 
                    LoggingUtils.LogToConsole("OnUnhandledException: " + ex); 
                } 
            } 
        } 
        #endregion 
        #region Обработка событий от удаленного сканера 
        private enum RemoteScannerEventType 
        { 
            Connected, 
            Disconnected, 
            WaitForInitialization, 
            ExitFromMenu 
        } 
        private RemoteScannerEventType _lastRemoteScannerEvent = RemoteScannerEventType.Disconnected; 
        private static readonly object s_remoteScannerEventSync = new object(); 
        private static readonly object s_handleRemoteScannerEventSync = new object(); 
        public event EventHandler RemoteScannerConnected; 
        public event EventHandler RemoteScannerDisconnected; 
        public event EventHandler RemoteScannerWaitForInitialization; 
        public event EventHandler RemoteScannerExitFromMenu; 
        private void SyncManager_RemoteScannerConnected(object sender, EventArgs e) 
        { 
            _logger.LogVerbose(Message.SyncRemoteScannerConnected); 
            HandleRemoteScannerEvent(RemoteScannerEventType.Connected); 
        } 
        private void SyncManager_RemoteScannerDisconnected(object sender, EventArgs e) 
        { 
            _logger.LogVerbose(Message.SyncRemoteScannerDisconnected); 
            HandleRemoteScannerEvent(RemoteScannerEventType.Disconnected); 
        } 
        private void SyncManager_RemoteScannerWaitForInitialization(object sender, EventArgs e) 
        { 
            _logger.LogVerbose(Message.SyncRemoteScannerWaitForInitialization); 
            HandleRemoteScannerEvent(RemoteScannerEventType.WaitForInitialization); 
        } 
        private void SyncManager_RemoteScannerExitFromMenu(object sender, EventArgs e) 
        { 
            _logger.LogVerbose(Message.SyncRemoteScannerExitFromMenu); 
            HandleRemoteScannerEvent(RemoteScannerEventType.ExitFromMenu); 
        } 
        private void HandleRemoteScannerEvent(RemoteScannerEventType eventType) 
        { 
            lock (s_remoteScannerEventSync) 
            { 
                _lastRemoteScannerEvent = eventType; 
            } 
            if (!Monitor.TryEnter(s_handleRemoteScannerEventSync, 100)) 
                return; 
            try 
            { 
                _scannerManager.StopScanning(); 
                lock (s_remoteScannerEventSync) 
                { 
                    switch (_lastRemoteScannerEvent) 
                    { 
                        case RemoteScannerEventType.Connected: 
                            _logger.LogVerbose(Message.SyncRemoteScannerConnectedEventRaise); 
                            RemoteScannerConnected.RaiseEvent(this); 
                            break; 
                        case RemoteScannerEventType.Disconnected: 
                            _logger.LogVerbose(Message.SyncRemoteScannerDisconnectedEventRaise); 
                            RemoteScannerDisconnected.RaiseEvent(this); 
                            break; 
                        case RemoteScannerEventType.WaitForInitialization: 
                            _logger.LogVerbose(Message.SyncRemoteScannerWaitForInitializationEventRaise); 
                            RemoteScannerWaitForInitialization.RaiseEvent(this); 
                            break; 
                        case RemoteScannerEventType.ExitFromMenu: 
                            _logger.LogVerbose(Message.SyncRemoteScannerExitFromMenuEventRaise); 
                            RemoteScannerExitFromMenu.RaiseEvent(this); 
                            break; 
                    } 
                    Thread.Sleep(TimeSpan.FromSeconds(1)); 
                } 
            } 
            finally 
            { 
                Monitor.Exit(s_handleRemoteScannerEventSync); 
            } 
        } 
        #endregion 
        #region Вход в меню 
        public event EventHandler SystemMenuEntering; 
        public event EventHandler OperatorMenuEntering; 
        private void StartWaitForMenuEntering() 
        { 
            ThreadUtils.StartBackgroundThread(WaitForMenuEntering); 
        } 
        private void WaitForMenuEntering() 
        { 
            var waitedEvents = new WaitHandle[] 
            { 
                new RepetableKeyPressWaitHandle(new KeyPressingWaitDescriptor(KeyType.Menu), 1), 
                new RepetableKeyPressWaitHandle(new KeyPressingWaitDescriptor(KeyType.Menu), 2) 
            }; 
            while (true) 
            { 
                var occurredEventIndex = WaitHandle.WaitAny(waitedEvents, Timeout.Infinite); 
                if (occurredEventIndex == 0) 
                    OperatorMenuEntering.RaiseEvent(this); 
                else if (occurredEventIndex == 1) 
                    SystemMenuEntering.RaiseEvent(this); 
                Thread.Sleep(500); 
            } 
        } 
        #endregion 
        #region Реализация действий 
        public NextActivityKey CanGoToMainVotingMode( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return 
                _electionManager.IsElectionDay() == ElectionDayСomming.ItsElectionDay && 
                _electionManager.SourceData.ElectionMode == ElectionMode.Real && 
                _electionManager.SourceData.GetTimeToModeStart(VotingMode.Main) > TimeSpan.Zero 
                ? BpcNextActivityKeys.No : BpcNextActivityKeys.Yes; 
        } 
        public TimeSpan MainVotingModeStartTime 
        { 
            get 
            { 
                return _electionManager.SourceData.GetVotingModeStartTime(VotingMode.Main); 
            } 
        } 
        public NextActivityKey ResetState( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (_syncManager.ScannerRole == ScannerRole.Slave) 
                return context.DefaultNextActivityKey; 
            if (_syncManager.IsRemoteScannerConnected) 
                Thread.Sleep(500); 
            _syncManager.ResetState("возврат к инициализации"); 
            if (_syncManager.IsRemoteScannerConnected) 
                _syncManager.RemoteScanner.ResetState("возврат к инициализации [инициатор главный]"); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey ResetUik( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _syncManager.ResetUik(ResetSoftReason.ElectionFinished); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey SyncWorkflowWithMasterScanner( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _scannerManager.SetIndicator(CommonActivity.SYNCHRONIZATION_INDICATOR_TEXT); 
            _logger.LogVerbose(Message.WorkflowWaitForSynchronizationWithMaster); 
            context.Sleep(Timeout.Infinite); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey NoticeRemoteScannerAboutExitFromMenu( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (_syncManager.IsRemoteScannerConnected) 
                _syncManager.RemoteScanner.NoticeAboutExitFromMenu(); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey DoNothing(WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey IsElectionDayAndRealElectionMode( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return 
                IsIsElectionDayOrExtra(_electionManager.IsElectionDay()) && 
                _electionManager.SourceData.IsReal 
                    ? BpcNextActivityKeys.Yes 
                    : BpcNextActivityKeys.No; 
        } 
        #endregion 
    } 
}

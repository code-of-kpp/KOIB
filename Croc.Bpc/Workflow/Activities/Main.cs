using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Core.Extensions; 

using Croc.Workflow.ComponentModel; 

using Croc.Bpc.Election; 

using Croc.Bpc.Election.Voting; 

using Croc.Bpc.Configuration; 

using Croc.Bpc.Common.Diagnostics; 

using System.Security.Permissions; 

using Croc.Core; 

using System.Threading; 

using Croc.Bpc.Scanner; 

using Croc.Bpc.Synchronization; 

using Croc.Bpc.Keyboard; 

 

 

namespace Croc.Bpc.Workflow.Activities 

{ 

    [Serializable] 

    public class MainActivity : BpcCompositeActivity 

    { 

        /// <summary> 

        /// Инициализация 

        /// </summary> 

        /// <param name="context"></param> 

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)] 

        protected override void Initialize(WorkflowExecutionContext context) 

        { 

            base.Initialize(context); 

 

 

            // обработка неотловленных исключений 

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException); 

 

 

            // подпишемся на события подключения/потери связи с удаленным сканером 

            _syncManager.RemoteScannerConnected += new EventHandler(SyncManager_RemoteScannerConnected); 

            _syncManager.RemoteScannerDisconnected += new EventHandler(SyncManager_RemoteScannerDisconnected); 

 

 

            // запустим поток ожидающий нажатия меню 1 или 2 раза 

			Thread sysMenuThread = new Thread(WaitMenuPress); 

			sysMenuThread.IsBackground = true; 

			sysMenuThread.Start(); 

        } 

 

 

        #region Обработка неотловленных исключений 


 
 

        /// <summary> 

        /// Событие "Произошла неожиданная ошибка" 

        /// </summary> 

        public event EventHandler UnexpectedErrorOccurred; 

 

 

        /// <summary> 

        /// Обработка всех неотловленных исключений 

        /// </summary> 

        /// <param name="sender"></param> 

        /// <param name="e"></param> 

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) 

        { 

            try 

            { 

                CoreApplication.Instance.Logger.LogException(Message.CriticalException, (Exception)e.ExceptionObject); 

                UnexpectedErrorOccurred.RaiseEvent(this); 

 

 

                // ждем, когда приложение завершит работу, чтобы не выпускать исключение 

                CoreApplication.Instance.WaitForExit(); 

            } 

            catch (Exception ex2) 

            { 

                try 

                { 

                    CoreApplication.Instance.Logger.LogException(Message.CriticalException, ex2); 

                } 

                catch { } 

            } 

        } 

 

 

        #endregion 

 

 

        #region Обработка событий по подключению/отключению удаленного сканера 

 

 

        /// <summary> 

        /// Событие "Соединение с удаленным сканером установлено" 

        /// </summary> 

        public event EventHandler RemoteScannerConnected; 

        /// <summary> 

        /// Событие "Связь с удаленным сканером потеряна" 

        /// </summary> 

        public event EventHandler RemoteScannerDisconnected; 

 


 
        /// <summary> 

        /// Обработчик события "Соединение с удаленным сканером установлено" 

        /// </summary> 

        /// <param name="sender"></param> 

        /// <param name="e"></param> 

        private void SyncManager_RemoteScannerConnected(object sender, EventArgs e) 

        { 

            _logger.LogVerbose(Message.DebugVerbose, "Удаленный сканер подключился"); 

 

 

            StopScanningAfterSheetProcessingFinished(); 

 

 

            _logger.LogVerbose(Message.DebugVerbose, "Извещаем о подключении удаленного сканера"); 

            RemoteScannerConnected.RaiseEvent(this); 

        } 

 

 

        /// <summary> 

        /// Обработка события "Связь с удаленным сканером потеряна" 

        /// </summary> 

        /// <param name="sender"></param> 

        /// <param name="e"></param> 

        private void SyncManager_RemoteScannerDisconnected(object sender, EventArgs e) 

        { 

            _logger.LogVerbose(Message.DebugVerbose, "Удаленный сканер отключился"); 

 

 

            StopScanningAfterSheetProcessingFinished(); 

 

 

            _logger.LogVerbose(Message.DebugVerbose, "Извещаем об отключении удаленного сканера"); 

            RemoteScannerDisconnected.RaiseEvent(this); 

        } 

 

 

        /// <summary> 

        /// Останавливает сканирование, но только после того, как обработка текущего листа 

        /// завершится (если конечно лист обрабатывается, если нет, то останавливает сканирование сразу же) 

        /// </summary> 

        private void StopScanningAfterSheetProcessingFinished() 

        { 

            // ждем, когда завершится обработка листа 

            _scannerManager.SheetProcessingSession.WaitForClose(); 

            // выключаем сканирование, чтобы не допустить вброс еще одного бюллетеня, потому что если  

            // придет событие "Поступил новый лист", то оно "затрет" событие о нахождении/потери второго сканера 

            _scannerManager.StopScanning(); 

            // выключим лампы 

            _scannerManager.SetLampsRegime(ScannerLampsRegime.BothOff); 


        } 

 

 

        #endregion 

 

 

        #region Вход в системное меню 

 

 

		/// <summary> 

		/// Ожидание нажатия меню или двойного меню 

		/// </summary> 

		private void WaitMenuPress() 

		{ 

			var waitedEvents = new List<WaitHandle>(2); 

 

 

			// нужно отлавливать одинарное или двойное нажатие Меню 

			waitedEvents.Add(new RepetableKeyPressWaitHandle(new KeyPressingWaitDescriptor(KeyType.Menu), 1)); 

			waitedEvents.Add(new RepetableKeyPressWaitHandle(new KeyPressingWaitDescriptor(KeyType.Menu), 2)); 

 

 

			// ждем события 

			while (true) 

			{ 

				var occurredEventIndex = WaitHandle.WaitAny(waitedEvents.ToArray(), Timeout.Infinite); 

 

 

				if (occurredEventIndex == 0) 

					OperatorMenuEntering.RaiseEvent(this); 

				else if (occurredEventIndex == 1) 

					SystemMenuEntering.RaiseEvent(this); 

 

 

				Thread.Sleep(500); 

			} 

		} 

 

 

        /// <summary> 

        /// Событие "Поступила команда входа в системное меню" 

        /// </summary> 

        public event EventHandler SystemMenuEntering; 

 

 

		/// <summary> 

		/// Событие "Поступила команда входа в меню оператора" 

		/// </summary> 

		public event EventHandler OperatorMenuEntering; 

 


 
        #endregion 

 

 

        #region Реализация действий 

 

 

        /// <summary> 

        /// Открывает канал для подключения удаленных сканеров 

        /// </summary> 

        public NextActivityKey OpenIncomingInteractionChannel( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // открываем канал 

            _syncManager.OpenIncomingInteractionChannel(_scannerManager.SerialNumber, _scannerManager.IPAddress); 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Проверка версий данного приложения и удаленного (то, что работает на втором сканере) 

        /// </summary> 

        public NextActivityKey CheckApplicationVersions( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _syncManager.ApplicationVersion == _syncManager.RemoteScanner.ApplicationVersion 

                ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// Сбрасывание состояния в начальное положение 

        /// </summary> 

        public NextActivityKey ResetState( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _syncManager.ResetState(); 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Сбрасывание выборов в начальное состояние и перезапуск приложения 

        /// </summary> 

        public NextActivityKey ResetElectionAndExit( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // подождем, пока синхронизация завершится 

            _syncManager.WaitForSynchronizationFinished(context); 

 


 
            // сбросим ПО 

            _syncManager.ResetSoft(); 

 

 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Синхронизация потока работ подчиненного сканера с главным 

        /// </summary> 

        public NextActivityKey SyncWorkflowWithMasterScanner( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _scannerManager.SetIndicator(CommonActivity.SYNCHRONIZATION_INDICATOR_TEXT); 

 

 

            // ждем, пока не случится синхронизация или пока связь с удаленным сканером не пропадет 

            // (и то и другое приведет к переключению выполнения потока работ на другое действие) 

            context.Sleep(Timeout.Infinite); 

 

 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Можно ли перейти в стац. режим голосования 

        /// </summary> 

        public NextActivityKey CanGoToMainVotingMode( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // переходить нельзя, если: 

            return 

                // сейчас День выборов И 

                _electionManager.SourceData.IsElectionDay && 

                // режим выборов == Боевой И 

                _electionManager.SourceData.ElectionMode == ElectionMode.Real && 

                // время начала Стационарного режима голосования еще не наступило 

                !_electionManager.SourceData.IsVotingModeTime(VotingMode.Main) 

 

 

                ? BpcNextActivityKeys.No : BpcNextActivityKeys.Yes; 

        } 

 

 

        /// <summary> 

        /// Время начала Стационарного режима голосования 

        /// </summary> 


        public TimeSpan MainVotingModeStartTime 

        { 

            get 

            { 

                return _electionManager.SourceData.GetVotingModeStartTime(VotingMode.Main); 

            } 

        } 

 

 

        #endregion 

    } 

}



using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Threading; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Workflow.ComponentModel; 

using Croc.Core.Diagnostics; 

using Croc.Bpc.Election; 

using Croc.Bpc.Scanner; 

using Croc.Bpc.Synchronization; 

using System.IO; 

using Croc.Bpc.Election.Voting; 

using Croc.Core.Utils.Threading; 

using Croc.Bpc.Keyboard; 

 

 

namespace Croc.Bpc.Workflow.Activities.Initialization 

{ 

    /// <summary> 

    /// Инициализация 

    /// </summary> 

    [Serializable] 

    public class InitializationActivity : BpcCompositeActivity 

    { 

        /// <summary> 

        /// Путь к файлу с ИД 

        /// </summary> 

        [NonSerialized] 

        private string _sourceDataFilePath; 

        /// <summary> 

        /// Номер УИК, полученный из имени файла с ИД 

        /// </summary> 

        [NonSerialized] 

        private int _uikFromFile; 

        public int UikFromFile 

        { 

            get 

            { 

                return _uikFromFile; 

            } 

        } 

        /// <summary> 

        /// ИД, загруженные из файла 

        /// </summary> 

        [NonSerialized] 

        private SourceData _sourceDataFromFile; 

        public SourceData SourceDataFromFile 

        { 

            get 


            { 

                return _sourceDataFromFile; 

            } 

        } 

 

 

        /// <summary> 

        /// Событие "Нажали кнопку ДА" 

        /// </summary> 

        public WaitHandle ButtonYesPressedEvent 

        { 

            get 

            { 

                return KeyPressedWaitHandle.YesPressed; 

            } 

        } 

        /// <summary> 

        /// Событие "Удаленный сканер стал Главным" 

        /// </summary> 

        /// <remarks>Используем защищенный дескриптор ожидания с ручным сбросом</remarks> 

        [NonSerialized] 

        private EventWaitHandleEx _remoteScannerBecameMaster; 

        public WaitHandle RemoteScannerBecameMaster 

        { 

            get 

            { 

                return _remoteScannerBecameMaster; 

            } 

        } 

        /// <summary> 

        /// Дата выборов 

        /// </summary> 

        public DateTime ElectionDate 

        { 

            get 

            { 

                return _electionManager.SourceData.ElectionDate; 

            } 

        } 

 

 

        /// <summary> 

        /// Инициализация 

        /// </summary> 

        /// <param name="context"></param> 

        protected override void Initialize(WorkflowExecutionContext context) 

        { 

            base.Initialize(context); 

 

 


            _remoteScannerBecameMaster = new EventWaitHandleEx(false, true, this); 

            _syncManager.RemoteScanner.ScannerRoleChanged += RemoteScanner_ScannerRoleChanged; 

        } 

 

 

        /// <summary> 

        /// Деинициализация 

        /// </summary> 

        /// <param name="context"></param> 

        protected override void Uninitialize(WorkflowExecutionContext context) 

        { 

            _syncManager.RemoteScanner.ScannerRoleChanged -= RemoteScanner_ScannerRoleChanged; 

            _remoteScannerBecameMaster.Dispose(); 

 

 

            base.Uninitialize(context); 

        } 

 

 

        /// <summary> 

        /// Обработчик события "Роль удаленного сканера изменилась" 

        /// </summary> 

        /// <param name="sender"></param> 

        /// <param name="e"></param> 

        void RemoteScanner_ScannerRoleChanged(object sender, EventArgs e) 

        { 

            _remoteScannerBecameMaster.GetAccess(this); 

 

 

            if (_syncManager.RemoteScanner.ScannerRole == ScannerRole.Master) 

                _remoteScannerBecameMaster.Set(); 

            else 

                _remoteScannerBecameMaster.Reset(); 

        } 

 

 

        #region Реализация действий 

 

 

        /// <summary> 

        /// Сброс роли сканера в "Не определена" 

        /// </summary> 

        /// <param name="context"></param> 

        public void ResetScannerRoleToUndefined(WorkflowExecutionContext context) 

        { 

            _syncManager.ScannerRole = ScannerRole.Undefined; 

        } 

 

 

        /// <summary> 


        /// Загрузка состояния 

        /// </summary> 

        public NextActivityKey LoadState( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            try 

            { 

                _syncManager.LoadState(); 

                _logger.LogInfo(Message.SyncStateLoaded); 

                return BpcNextActivityKeys.Yes; 

            } 

            catch (Exception ex) 

            { 

                _logger.LogException(Message.SyncStateLoadException, ex); 

                return BpcNextActivityKeys.No; 

            } 

        } 

 

 

        /// <summary> 

        /// Архивирование состояния и сброс состояния в начальное 

        /// </summary> 

        public NextActivityKey ResetState( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            try 

            { 

                _syncManager.ResetState(); 

                _logger.LogInfo(Message.SyncStateReset); 

            } 

            catch (Exception ex) 

            { 

                _logger.LogException(Message.SyncStateResetException, ex); 

            } 

 

 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Установка роли сканера в "Неопределена", отмена синхронизации и поиск файла с ИД 

        /// </summary> 

        public NextActivityKey SearchSourceDataFile( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        {             

            // выключим синхронизацию состояния 

            _syncManager.SynchronizationEnabled = false; 

 

 


            // попробуем найти файл с ИД 

            if (!_electionManager.FindSourceDataFile(out _sourceDataFilePath, out _uikFromFile)) 

                return BpcNextActivityKeys.No; 

 

 

            _logger.LogInfo(Message.ElectionSourceDataFound, _sourceDataFilePath, _uikFromFile); 

            return BpcNextActivityKeys.Yes; 

        } 

 

 

        /// <summary> 

        /// Установление роли сканера = "Главный" 

        /// </summary> 

        public NextActivityKey SetRoleToMaster( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _syncManager.ScannerRole = ScannerRole.Master; 

            var remoteScannerRole = _syncManager.RemoteScanner.WaitForScannerRoleDefined(); 

 

 

            // если 2-ой сканер тоже Главный 

            return remoteScannerRole == ScannerRole.Master 

                ? BpcNextActivityKeys.No    // то это ошибка 

                : BpcNextActivityKeys.Yes;  // ОК: 2-ой Подчиненный или его нет 

        } 

 

 

        /// <summary> 

        /// Установка даты и времени на подчиненном сканера 

        /// </summary> 

        public NextActivityKey SetDateTimeOnSlave( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // если удаленный сканер подключен 

            if (_syncManager.IsRemoteScannerConnected) 

                // то установим на нем время равное текущему UTC-времени 

                _syncManager.RemoteScanner.SetSystemTime(DateTime.UtcNow); 

 

 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Установление роли сканера = "Подчиненный" 

        /// </summary> 

        public NextActivityKey SetRoleToSlave( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _syncManager.ScannerRole = ScannerRole.Slave; 


            var remoteScannerRole = _syncManager.RemoteScanner.WaitForScannerRoleDefined(); 

 

 

            // если 2-ой сканер не Главный или его нет 

            return remoteScannerRole != ScannerRole.Master 

                ? BpcNextActivityKeys.No    // то это ошибка 

                : BpcNextActivityKeys.Yes;  // ОК: 2-ой сканер Главный 

        } 

 

 

        /// <summary> 

        /// Загрузка исходных данных 

        /// </summary> 

        public NextActivityKey LoadSourceData( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            try 

            { 

                // ИД грузить нужно только на главном сканере 

                if (_syncManager.ScannerRole == ScannerRole.Master) 

                { 

                    _sourceDataFromFile = SourceDataLoader.LoadDataFromFile(_sourceDataFilePath); 

                    _logger.LogInfo(Message.ElectionSourceDataLoaded); 

                } 

 

 

                return BpcNextActivityKeys.Yes; 

            } 

            catch (Exception ex) 

            { 

                _logger.LogException(Message.ElectionSourceDataLoadException, ex); 

                return BpcNextActivityKeys.No; 

            } 

        } 

 

 

        /// <summary> 

        /// Состояние сканера было восстановлено? 

        /// </summary> 

        public NextActivityKey IsStateRestored( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _syncManager.IsStateInitial ? BpcNextActivityKeys.No : BpcNextActivityKeys.Yes; 

        } 

 

 

        /// <summary> 

        /// Режим выборов в ИД - Тренировка? 

        /// </summary> 

        public NextActivityKey IsSDElectionModeTraining( 


            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _electionManager.SourceData.ElectionMode == ElectionMode.Training 

                ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// Проверка даты выборов 

        /// </summary> 

        public NextActivityKey CheckElectionDate( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            var delta = (_electionManager.SourceData.LocalTimeNow.Date - _electionManager.SourceData.ElectionDate.Date) 

                .TotalDays; 

 

 

            if (delta == 0) 

                return BpcNextActivityKeys_VotingTime.ElectionDayNow; 

 

 

            return delta < 0 

                ? BpcNextActivityKeys_VotingTime.ElectionDayHasNotCome 

                : BpcNextActivityKeys_VotingTime.ElectionDayPassed; 

        } 

 

 

        /// <summary> 

        /// Установить режим выборов = Тренировка 

        /// </summary> 

        public NextActivityKey SetElectionModeToTraining( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _electionManager.SourceData.ElectionMode = ElectionMode.Training; 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Установить режим выборов = Боевой (настоящие выборы) 

        /// </summary> 

        public NextActivityKey SetElectionModeToReal( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _electionManager.SourceData.ElectionMode = ElectionMode.Real; 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 


        /// Синхронизация с удаленным сканером 

        /// </summary> 

        public NextActivityKey Synchronize( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // запускаем синхронизацию 

            _scannerManager.SetIndicator(CommonActivity.SYNCHRONIZATION_INDICATOR_TEXT); 

            _syncManager.StartStateSynchronization(true); 

 

 

            // ожидаем, когда синхронизация завершится 

            var synchronizationSucceeded = _syncManager.WaitForSynchronizationFinished(context); 

 

 

            if (!synchronizationSucceeded) 

                return BpcNextActivityKeys.No; 

 

 

            // если это главный сканер 

            if (_syncManager.ScannerRole == ScannerRole.Master) 

                // то перейдем к действию, которое соотв. восстановленному состоянию 

                _workflowManager.GoToStateActivity(false); 

 

 

            return BpcNextActivityKeys.Yes; 

        } 

 

 

        #endregion 

    } 

}



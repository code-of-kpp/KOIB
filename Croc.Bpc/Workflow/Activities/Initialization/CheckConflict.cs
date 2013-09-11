using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Workflow.ComponentModel; 

using Croc.Core.Diagnostics; 

using Croc.Bpc.Election; 

using Croc.Bpc.Election.Voting; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Bpc.Synchronization; 

 

 

namespace Croc.Bpc.Workflow.Activities.Initialization 

{ 

    [Serializable] 

    public class CheckConflictActivity : BpcCompositeActivity 

    { 

        /// <summary> 

        /// Имя данных "Обнаружен ли конфликт" 

        /// </summary> 

        private const string HASCONFLICT_DATANAME = "HasConflict"; 

 

 

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

            set 

            { 

                _uikFromFile = value; 

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


            set 

            { 

                _sourceDataFromFile = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Ждет решение главного сканера о наличии конфликта 

        /// </summary> 

        public NextActivityKey WaitForMasterDecision( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // включаем синхронизацию на время ожидания 

            _scannerManager.SetIndicator(CommonActivity.SYNCHRONIZATION_INDICATOR_TEXT); 

            _syncManager.SynchronizationEnabled = true; 

 

 

            NextActivityKey result; 

            var hasConflict = _syncManager.GetDataTransmittedFromRemoteScanner(HASCONFLICT_DATANAME, context); 

 

 

            // если получили null (приложение начало выключаться или потеряна связь со 2-м сканером) 

            if (hasConflict == null) 

            { 

                _logger.LogInfo(Message.WorkflowCannotDetectConflict); 

                result = BpcNextActivityKeys.No; 

            } 

            else if ((bool)hasConflict) 

            { 

                // есть конфликт 

                _logger.LogInfo(Message.WorkflowMasterDetectConflict); 

                result = BpcNextActivityKeys.No; 

            } 

            else 

            { 

                // нет конфликта 

                _logger.LogInfo(Message.WorkflowMasterDetectNoConflict); 

                result = BpcNextActivityKeys.Yes; 

            } 

 

 

            // выключаем синхронизацию 

            _syncManager.SynchronizationEnabled = false; 

 

 

            return result; 

        } 

 

 


        /// <summary> 

        /// Передает решение о наличии конфликта на подчиненный сканер 

        /// </summary> 

        public NextActivityKey TransmitDecisionToSlaveScanner( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // если это главный сканер и удаленный сканер подключен 

            if (_syncManager.ScannerRole == ScannerRole.Master && _syncManager.IsRemoteScannerConnected) 

            { 

                var hasConflict = parameters.GetParamValue<bool>("HasConflict"); 

                _syncManager.RemoteScanner.PutData(HASCONFLICT_DATANAME, hasConflict); 

            } 

 

 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// ИД_с совпадают с ИД_ф? 

        /// </summary> 

        public NextActivityKey IsStateSDEqualsFileSD( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return  

                _sourceDataFromFile != null &&  

                _electionManager.SourceData != null && 

                _sourceDataFromFile.Equals(_electionManager.SourceData) 

                ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// На Главном и Подчиненном сканерах НЕ совпадают ИД_с? 

        /// </summary> 

        public NextActivityKey StateSDNotEqualsOnMasterAndSlaveScanners( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return  

                // условие - что ИД совпадают 

                _electionManager.SourceData != null && 

                _electionManager.SourceData.Id.Equals(_syncManager.RemoteScanner.SourceDataId) 

                ? BpcNextActivityKeys.No : BpcNextActivityKeys.Yes; 

        } 

 

 

        /// <summary> 

        /// в ИД_с сейчас день выборов? 

        /// </summary> 

        public NextActivityKey IsElectionDayNowInStateSD( 


            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _syncManager.IsElectionDayNow ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// в ИД_с на подчиненном сканере сейчас день выборов? 

        /// </summary> 

        public NextActivityKey IsElectionDayNowInStateSDOnSlaveScanner( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _syncManager.RemoteScanner.IsElectionDayNow ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// заменяем ИД_с на ИД_ф 

        /// </summary> 

        public NextActivityKey ReplaseStateSDToFileSD( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _electionManager.SetSourceData(_sourceDataFromFile, _uikFromFile); 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// подчиненный сканер есть и у него состояние восстановлено? 

        /// </summary> 

        public NextActivityKey SlaveScannerExistsAndHasRestoredState( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _syncManager.IsRemoteScannerConnected && !_syncManager.RemoteScanner.IsStateInitial 

                ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// сбрасываем состояние в начальное на подчиненном сканере 

        /// </summary> 

        public NextActivityKey ResetStateOnSlaveScanner( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _syncManager.RemoteScanner.ResetState(); 

            return context.DefaultNextActivityKey; 

        } 

    } 

}



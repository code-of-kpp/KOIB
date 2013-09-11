using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Workflow.ComponentModel; 

using Croc.Bpc.FileSystem; 

using Croc.Bpc.Common.Diagnostics; 

using System.IO; 

using System.Collections.Specialized; 

 

 

namespace Croc.Bpc.Workflow.Activities.Summarizing 

{ 

    /// <summary> 

    /// Подведение итогов голосования 

    /// </summary> 

    [Serializable] 

    public class SummarizingActivity : ElectionEnumeratorActivity 

    { 

        /// <summary> 

        /// Имя данных "Доп. сведения введены" 

        /// </summary> 

        private const string ADDINFOENTERED_DATANAME = "AddInfoEntered"; 

 

 

        /// <summary> 

        /// Параметры для отчета "Предварительный протокол голосования" 

        /// </summary> 

        public ListDictionary PreliminaryElectionProtocolParameters 

        { 

            get 

            { 

                var protocolParams = new ListDictionary(); 

                protocolParams.Add("Election", _currentElection); 

                return protocolParams; 

            } 

        } 

 

 

        /// <summary> 

        /// Формирование предварительного протокола с результатами голосования по всем выборам 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey GeneratePreliminaryVotingResultProtocol( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _electionManager.GeneratePreliminaryVotingResultProtocol(); 

            return context.DefaultNextActivityKey; 


        } 

 

 

        /// <summary> 

        /// Извещает подчиненный сканер о завершении ввода доп. сведений и  

        /// формирует протокол с результатами голосования 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey NoticeSlaveAboutAddInfoEnteredAndGenerateVotingResultProtocol( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // извещаем подчиненный сканер, что ввод доп. сведений завершен 

            _syncManager.RemoteScanner.PutData(ADDINFOENTERED_DATANAME, true); 

            // формируем протокол 

            _electionManager.GenerateVotingResultProtocol(_currentElection); 

 

 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Ожидает завершения ввода доп. сведений по текущим выборам на главном сканере 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey WaitForAddInfoEnteredOnMaster( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _scannerManager.SetIndicator(CommonActivity.SYNCHRONIZATION_INDICATOR_TEXT); 

 

 

            // ожидаем, когда на главном сканере введут доп. сведения 

            var addInfoEntered = _syncManager.GetDataTransmittedFromRemoteScanner(ADDINFOENTERED_DATANAME, context); 

 

 

            // если получили null (приложение начало выключаться или потеряна связь со 2-м сканером) 

            // замечание:  

            //  при потере связи со 2-м сканером метод GetDataTransmittedFromRemoteScanner вернет null, 

            //  т.к. исключение ActivityExecutionInterruptException, которое он также должен был бы сгенерировать 

            //  при потере связи, не будет возбуждено, потому что данное действие WaitForAddInfoEnteredOnMaster 

            //  выполняется внутри действия с более высоким приоритетом, чем действие-обработчик события о потере связи 

            if (addInfoEntered == null) 

            { 

                _logger.LogInfo(Message.WorkflowCannotDetectAddInfoEnteredOnMaster); 

                return BpcNextActivityKeys.No; 

            } 


            else if (!(bool)addInfoEntered) 

            { 

                _logger.LogInfo(Message.WorkflowAddInfoNotEnteredOnMaster); 

                return BpcNextActivityKeys.No; 

            } 

 

 

            // доп. сведения введены 

            _logger.LogInfo(Message.WorkflowAddInfoEnteredOnMaster); 

            return BpcNextActivityKeys.Yes; 

        } 

 

 

        /// <summary> 

        /// Формирование протокола с результатами голосования по текущим выборам 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey GenerateVotingResultProtocol( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _electionManager.GenerateVotingResultProtocol(_currentElection); 

            return context.DefaultNextActivityKey; 

        } 

    } 

}



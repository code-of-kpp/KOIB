using System; 

using System.Collections.Generic; 

using Croc.Bpc.Scanner; 

using Croc.Workflow.ComponentModel; 

 

 

namespace Croc.Bpc.Workflow.Activities.Initialization 

{ 

    [Serializable] 

    public class ConnectToScannerActivity : BpcCompositeActivity 

    { 

        /// <summary> 

        /// Ошибки, обнаруженные в результате диагностики сканера 

        /// </summary> 

        [NonSerialized] 

        private List<ScannerDiagnosticsError> _diagnosticsErrors; 

        /// <summary> 

        /// Перечислитель ошибок диагностики 

        /// </summary> 

        [NonSerialized] 

        private IEnumerator<ScannerDiagnosticsError> _diagnosticsErrorsEnumerator; 

        /// <summary> 

        /// Признак того, что обнаружена критичная ошибка 

        /// </summary> 

        [NonSerialized] 

        private bool _criticalErrorFound; 

 

 

        /// <summary> 

        /// Установка соединения со сканером 

        /// </summary> 

        /// <remarks> 

        /// Параметры: 

        ///     MaxTryCount - максимальное кол-во попыток установить подключение (по умолчанию = 3) 

        ///     Delay       - задержка между попытками (по умолчанию = 5 сек) 

        ///     ErrorId     - ИД ошибки подключения к сканеру 

        /// </remarks> 

        public NextActivityKey EstablishConnectionToScanner( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            var maxTryCount = parameters.GetParamValue("MaxTryCount", 3); 

            var delay = TimeSpan.Parse(parameters.GetParamValue("Delay", "0:0:5")); 

 

 

            if (!_scannerManager.EstablishConnectionToScanner(maxTryCount, delay)) 

                return BpcNextActivityKeys.No; 

 

 

            // если подключится удалось, сбросим старые ошибки 

            var errorId = parameters.GetParamValueOrThrow<string>("ErrorId"); 


            _workflowManager.ResetErrorCounter(errorId); 

            return BpcNextActivityKeys.Yes; 

        } 

 

 

        /// <summary> 

        /// Диагностика сканера 

        /// </summary> 

        public NextActivityKey PerformDiagnostics( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _criticalErrorFound = false; 

            _diagnosticsErrors = _scannerManager.PerformDiagnostics(); 

 

 

            return _diagnosticsErrors.Count == 0 

                ? BpcNextActivityKeys.Yes // т.е. все ОК 

                : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// Инициализация перебора ошибок диагностики 

        /// </summary> 

        public NextActivityKey StartEnumerateDiagnosticsErrors( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _diagnosticsErrorsEnumerator = _diagnosticsErrors.GetEnumerator(); 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Получить очередную ошибку диагностики 

        /// </summary> 

        public NextActivityKey GetNextDiagnosticsError( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            if (!_diagnosticsErrorsEnumerator.MoveNext()) 

                return BpcNextActivityKeys.No; 

 

 

            return new NextActivityKey(_diagnosticsErrorsEnumerator.Current.ToString()); 

        } 

 

 

        /// <summary> 

        /// Установка признака того, что обнаружена критичная ошибка 

        /// </summary> 

        public NextActivityKey SetCriticalErrorFound( 


            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _criticalErrorFound = true; 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Была ли найдена критичная ошибка? 

        /// </summary> 

        public NextActivityKey WasCriticalErrorFound( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _criticalErrorFound ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

    } 

}



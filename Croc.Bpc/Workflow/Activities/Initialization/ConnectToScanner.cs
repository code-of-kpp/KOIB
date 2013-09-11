using System; 
using System.Collections.Generic; 
using Croc.Bpc.Scanner; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.Initialization 
{ 
    [Serializable] 
    public class ConnectToScannerActivity : BpcCompositeActivity 
    { 
        [NonSerialized] 
        private List<ScannerDiagnosticsError> _diagnosticsErrors; 
        [NonSerialized] 
        private IEnumerator<ScannerDiagnosticsError> _diagnosticsErrorsEnumerator; 
        [NonSerialized] private bool _criticalErrorFound; 
        public NextActivityKey EstablishConnectionToScanner( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var maxTryCount = parameters.GetParamValue("MaxTryCount", 3); 
            var delay = TimeSpan.Parse(parameters.GetParamValue("Delay", "0:0:5")); 
            if (!_scannerManager.EstablishConnectionToScanner(maxTryCount, delay)) 
                return BpcNextActivityKeys.No; 
            var errorId = parameters.GetParamValueOrThrow<string>("ErrorId"); 
            _workflowManager.ResetErrorCounter(errorId); 
            return BpcNextActivityKeys.Yes; 
        } 
        public NextActivityKey PerformDiagnostics( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _criticalErrorFound = false; 
            _diagnosticsErrors = _scannerManager.PerformDiagnostics(); 
            return _diagnosticsErrors.Count == 0 
                       ? BpcNextActivityKeys.Yes // т.е. все ОК 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey StartEnumerateDiagnosticsErrors( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _diagnosticsErrorsEnumerator = _diagnosticsErrors.GetEnumerator(); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey GetNextDiagnosticsError( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (!_diagnosticsErrorsEnumerator.MoveNext()) 
                return BpcNextActivityKeys.No; 
            return new NextActivityKey(_diagnosticsErrorsEnumerator.Current.ToString()); 
        } 
        public NextActivityKey SetCriticalErrorFound( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _criticalErrorFound = true; 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey WasCriticalErrorFound( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _criticalErrorFound ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 
        } 
    } 
}

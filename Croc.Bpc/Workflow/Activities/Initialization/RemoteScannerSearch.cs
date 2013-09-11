using System; 
using Croc.Bpc.Scanner; 
using Croc.Bpc.Synchronization; 
using Croc.Bpc.Diagnostics; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.Initialization 
{ 
    [Serializable] 
    public class RemoteScannerSearchActivity : BpcCompositeActivity 
    { 
        public int SearchSecondScannerTimeout 
        { 
            get 
            { 
#if DEBUG 
                return 1000; // 1 сек 
#else 
                return 30000; // 30 сек 
#endif 
            } 
        } 
        public NextActivityKey IsSecondScannerAlreadyConnected( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _syncManager.IsRemoteScannerConnected 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey OpenIncomingInteractionChannel( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _syncManager.OpenIncomingInteractionChannel(_scannerManager.SerialNumber, _scannerManager.IPAddress); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey StartLampsBlinking( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _scannerManager.SetLampsRegime(ScannerLampsRegime.GreenAndRedBlinking); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey StopLampsBlinking( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _scannerManager.SetLampsRegime(ScannerLampsRegime.BothOff); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey CheckApplicationVersions( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var remoteScannerApplicationVersion = _syncManager.RemoteScanner.ApplicationVersion; 
            if (_syncManager.ApplicationVersion == remoteScannerApplicationVersion) 
                return BpcNextActivityKeys.Yes; 
            _logger.LogInfo(Message.WorkflowApplicationVersionsDiffer, 
                            _syncManager.ApplicationVersion, 
                            remoteScannerApplicationVersion); 
            return BpcNextActivityKeys.No; 
        } 
        public NextActivityKey CheckSerialNumbersEquals( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _syncManager.LocalScannerSerialNumber == _syncManager.RemoteScannerSerialNumber 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public void ResetScannerRoleToUndefined(WorkflowExecutionContext context) 
        { 
            _syncManager.ScannerRole = ScannerRole.Undefined; 
        } 
    } 
}

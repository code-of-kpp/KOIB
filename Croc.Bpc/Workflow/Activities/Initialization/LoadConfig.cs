using System; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Synchronization; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.Initialization 
{ 
    [Serializable] 
    public class LoadConfigActivity : BpcCompositeActivity 
    { 
        public NextActivityKey LoadWorkingConfig( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _configManager.LoadWorkingConfig() ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey ResetWorkingConfig( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _configManager.ResetWorkingConfig(); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey LoadPartialConfig( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            const string PARTIALCONFIG_DATANAME = "PartialConfig"; 
            const string NO_PARTIALCONFIG_DATA = "No PartialConfig"; 
            const string BAD_PARTIALCONFIG_DATA = "Bad PartialConfig"; 
            string partialConfigXml = null; 
            if (_syncManager.ScannerRole == ScannerRole.Master) 
            { 
                if (!_configManager.LoadPartialConfig(ref partialConfigXml)) 
                { 
                    _syncManager.RemoteScanner.PutData(PARTIALCONFIG_DATANAME, BAD_PARTIALCONFIG_DATA); 
                    return BpcNextActivityKeys.No; 
                } 
                _syncManager.RemoteScanner.PutData( 
                    PARTIALCONFIG_DATANAME, 
                    string.IsNullOrEmpty(partialConfigXml) 
                        ? NO_PARTIALCONFIG_DATA // если частная конфигурация не была найдена 
                        : partialConfigXml); 
                return BpcNextActivityKeys.Yes; 
            } 
            _scannerManager.SetIndicator(CommonActivity.SYNCHRONIZATION_INDICATOR_TEXT); 
            _logger.LogVerbose(Message.WorkflowWaitForPartialConfigFromMaster); 
            partialConfigXml = (string) _syncManager.GetDataTransmittedFromRemoteScanner( 
                PARTIALCONFIG_DATANAME, context); 
            if (partialConfigXml == null || 
                string.CompareOrdinal(NO_PARTIALCONFIG_DATA, partialConfigXml) == 0) 
            { 
                return BpcNextActivityKeys.Yes; 
            } 
            if (string.CompareOrdinal(BAD_PARTIALCONFIG_DATA, partialConfigXml) == 0) 
            { 
                return BpcNextActivityKeys.No; 
            } 
            return _configManager.LoadPartialConfig(ref partialConfigXml) 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey ApplyConfig( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _configManager.ApplyConfig() 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
    } 
}

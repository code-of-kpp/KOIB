using System; 

using Croc.Bpc.Synchronization; 

using Croc.Workflow.ComponentModel; 

 

 

namespace Croc.Bpc.Workflow.Activities.Initialization 

{ 

    [Serializable] 

    public class LoadConfigActivity : BpcCompositeActivity 

    { 

        /// <summary> 

        /// Загрузка рабочей конфигурации 

        /// </summary> 

        public NextActivityKey LoadWorkingConfig( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _configManager.LoadWorkingConfig() ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// Сбросить рабочую конфигурацию в начальное состояние 

        /// </summary> 

        public NextActivityKey ResetWorkingConfig( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _configManager.ResetWorkingConfig(); 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Загрузка частной конфигурации 

        /// </summary> 

        public NextActivityKey LoadPartialConfig( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            const string PARTIALCONFIG_DATANAME = "PartialConfig"; 

            const string NO_PARTIALCONFIG_DATA = "No PartialConfig"; 

            const string BAD_PARTIALCONFIG_DATA = "Bad PartialConfig"; 

 

 

            string partialConfigXml = null; 

 

 

            // если это Главный сканер 

            if (_syncManager.ScannerRole == ScannerRole.Master) 

            { 

                // пробуем загрузить частную конфигурацию из файла на флеш-диске 

                if (_configManager.LoadPartialConfig(ref partialConfigXml)) 


                { 

                    // если частная конфигурация не была найдена 

                    if (string.IsNullOrEmpty(partialConfigXml)) 

                        // сообщаем подчиненному сканеру, что частной конфигурации нет 

                        _syncManager.RemoteScanner.PutData(PARTIALCONFIG_DATANAME, NO_PARTIALCONFIG_DATA); 

                    else 

                        // передаем содержимое частной конфигурации на подчиненный сканер 

                        _syncManager.RemoteScanner.PutData(PARTIALCONFIG_DATANAME, partialConfigXml); 

 

 

                    return BpcNextActivityKeys.Yes; 

                } 

                else 

                { 

                    // сообщаем подчиненному сканеру, что при загрузке конфигурации главный получил ошибку 

                    _syncManager.RemoteScanner.PutData(PARTIALCONFIG_DATANAME, BAD_PARTIALCONFIG_DATA); 

 

 

                    return BpcNextActivityKeys.No; 

                } 

            } 

            // это подчиненный сканер 

            else 

            { 

                // ждем, когда главный сканер передаст данные с частной конфигурацией 

 

 

                // включаем синхронизацию на время ожидания 

                _scannerManager.SetIndicator(CommonActivity.SYNCHRONIZATION_INDICATOR_TEXT); 

                _syncManager.SynchronizationEnabled = true; 

 

 

                try 

                { 

                    partialConfigXml = (string)_syncManager.GetDataTransmittedFromRemoteScanner( 

                        PARTIALCONFIG_DATANAME, context); 

 

 

                    // если получили null (приложение начало выключаться или потеряна связь со 2-м сканером) 

                    // или главный сообщил, что частного конфига нет 

                    if (partialConfigXml == null || 

                        string.CompareOrdinal(NO_PARTIALCONFIG_DATA, partialConfigXml) == 0) 

                    { 

                        return BpcNextActivityKeys.Yes; 

                    } 

                    // иначе, если при загрузке конфигурации главный получил ошибку 

                    else if (string.CompareOrdinal(BAD_PARTIALCONFIG_DATA, partialConfigXml) == 0) 

                    { 

                        return BpcNextActivityKeys.No; 

                    } 


 
 

                    // получен частный конфиг с главного => пробуем загрузить его 

                    return _configManager.LoadPartialConfig(ref partialConfigXml) 

                        ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

                } 

                finally 

                { 

                    // выключаем синхронизацию 

                    _syncManager.SynchronizationEnabled = false; 

                } 

            } 

        } 

 

 

        /// <summary> 

        /// Применение полученной конфигурации 

        /// </summary> 

        public NextActivityKey ApplyConfig( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _configManager.ApplyConfig() ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

    } 

}



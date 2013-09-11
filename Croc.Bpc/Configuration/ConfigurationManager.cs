using System; 
using System.Collections.Generic; 
using System.IO; 
using Croc.Bpc.Utils; 
using Croc.Bpc.Voting; 
using Croc.Bpc.Configuration.Config; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.FileSystem; 
using Croc.Core; 
using Croc.Core.Configuration; 
using Croc.Core.Utils; 
using Croc.Core.Utils.Xml; 
namespace Croc.Bpc.Configuration 
{ 
    [SubsystemConfigurationElementTypeAttribute(typeof(ConfigurationManagerConfig))] 
    public class ConfigurationManager : 
        Subsystem, //StateSubsystem,  
        IConfigurationManager 
    { 
        private const string CONFIG_FILE_CONTENT_FORMAT = 
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n" + 
            "<configuration>\n" + 
            "  <configSections>\n" + 
            "    <section name=\"croc.application\" type=\"Croc.Core.Configuration.ApplicationConfig, Croc.Core\" />\n" + 
            "  </configSections>\n" + 
            "  {0}\n" + // точка для вставки текста секции croc.application 
            "</configuration>"; 
        private ConfigurationManagerConfig _config; 
        private XmlMerge _mergeUtil; 
        private ApplicationConfig _currentConfig; 
        private string _currentConfigXml; 
        private IFileSystemManager _fileSystemManager; 
        private IElectionManager _electionManager; 
        public override void Init(SubsystemConfig config) 
        { 
            _config = (ConfigurationManagerConfig)config; 
            var keyAttributeNames = new Dictionary<string, string> { { "subsystem", "name" } }; 
            _mergeUtil = new XmlMerge(_config.PrivateConfigElementXPaths.ToList(), keyAttributeNames); 
            _currentConfig = CoreApplication.Instance.Config; 
            _currentConfigXml = _currentConfig.ToXml(); 
            _fileSystemManager = Application.GetSubsystemOrThrow<IFileSystemManager>(); 
            _electionManager = Application.GetSubsystemOrThrow<IElectionManager>(); 
            WorkingConfigLoaded = false; 
            Application.ConfigUpdated += ApplicationConfigUpdated; 
        } 
        #region Обработка события изменения конфигурации приложения 
        private void ApplicationConfigUpdated(object sender, ConfigUpdatedEventArgs e) 
        { 
            _currentConfig = Application.Config; 
            _currentConfigXml = Application.Config.ToXml(); 
            SaveWorkingConfig(); 
            Logger.LogInfo(Message.ConfigSubsystemConfigUpdated, 
                           e.UpdatedParameterName, e.SubsystemName, e.OldValue, e.NewValue); 
        } 
        #endregion 
        #region IConfigurationManager 
        public bool WorkingConfigLoaded 
        { 
            get; 
            private set; 
        } 
        public bool LoadWorkingConfig() 
        { 
            if (!File.Exists(_config.WorkingConfigFilePath.Value)) 
                return true; 
            try 
            { 
                _currentConfig = ConfigurationUtils.GetSection<ApplicationConfig>( 
                    _config.WorkingConfigFilePath.Value, ApplicationConfig.SECTION_NAME); 
                _currentConfigXml = _currentConfig.ToXml(); 
                return true; 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.ConfigLoadWorkingError, ex); 
                return false; 
            } 
        } 
        public bool LoadPartialConfig(ref string partialConfigXml) 
        { 
            if (string.IsNullOrEmpty(partialConfigXml)) 
            { 
                var partialConfigFile = FindPartialConfigFile(); 
                if (partialConfigFile == null) 
                    return true; 
                try 
                { 
                    using (var reader = partialConfigFile.OpenText()) 
                        partialConfigXml = reader.ReadToEnd(); 
                } 
                catch (Exception ex) 
                { 
                    Logger.LogError(Message.ConfigReadPartialError, ex); 
                    return false; 
                } 
            } 
            try 
            { 
                if (_mergeUtil.Merge(partialConfigXml, _currentConfigXml)) 
                { 
                    string resXml; 
                    using (var memStream = new MemoryStream()) 
                    { 
                        using (var xmlWriter = new PrettyPrintXmlWriter(memStream)) 
                        { 
                            _mergeUtil.Result.WriteTo(xmlWriter); 
                            resXml = xmlWriter.ToFormatString(); 
                        } 
                    } 
                    _currentConfig = ApplicationConfig.FromXml(resXml); 
                    _currentConfigXml = resXml; 
                } 
                return true; 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.ConfigLoadPartialError, ex); 
                return false; 
            } 
        } 
        private FileInfo FindPartialConfigFile() 
        { 
            if (_config.PartialConfigFileLocations.IncludeSourceDataPaths) 
            { 
                foreach (var dir in _electionManager.GetSourceDataSearchPaths()) 
                { 
                    var fileName = Path.Combine(dir, _config.PartialConfigFileLocations.FileName); 
                    try 
                    { 
                        var fileInfo = new FileInfo(fileName); 
                        if (fileInfo.Exists) 
                            return fileInfo; 
                    } 
                    catch (Exception ex) 
                    { 
                        Logger.LogError(Message.ConfigFindPartialError, ex, fileName); 
                    } 
                } 
            } 
            foreach (ValueConfig<string> item in _config.PartialConfigFileLocations.RootPaths) 
            { 
                var fileName = Path.Combine(item.Value, _config.PartialConfigFileLocations.FileName); 
                try 
                { 
                    var fileInfo = new FileInfo(fileName); 
                    if (fileInfo.Exists) 
                        return fileInfo; 
                } 
                catch (Exception ex) 
                { 
                    Logger.LogError(Message.ConfigFindPartialError, ex, fileName); 
                } 
            } 
            return null; 
        } 
        public void ResetWorkingConfig() 
        { 
            try 
            { 
                File.Delete(_config.WorkingConfigFilePath.Value); 
                WorkingConfigLoaded = false; 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.ConfigDeleteWorkingError, ex); 
            } 
        } 
        public bool ApplyConfig() 
        { 
            try 
            { 
                var res = CoreApplication.Instance.ApplyNewConfig(_currentConfig, true); 
                if (res) 
                { 
                    SaveWorkingConfig(); 
                } 
                WorkingConfigLoaded = true; 
                return true; 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.ConfigApplyError, ex); 
                return false; 
            } 
        } 
        private void SaveWorkingConfig() 
        { 
            string tempFileName = 
                _fileSystemManager.GetTempFileName( 
                new FileInfo(_config.WorkingConfigFilePath.Value).DirectoryName); 
            var text = string.Format(CONFIG_FILE_CONTENT_FORMAT, _currentConfigXml); 
            _fileSystemManager.WriteTextToFile( 
                tempFileName, 
                FileMode.Create, 
                text, 
                true); 
            SystemHelper.SyncFileSystem(); 
            if (File.Exists(_config.WorkingConfigFilePath.Value)) 
            { 
                File.Delete(_config.WorkingConfigFilePath.Value); 
            } 
            File.Move(tempFileName, _config.WorkingConfigFilePath.Value); 
        } 
        #endregion 
        #region StateSubsystem overrides 
        #endregion 
    } 
}

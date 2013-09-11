using System; 

using System.Collections.Generic; 

using System.IO; 

using System.Text; 

using System.Xml; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Bpc.Configuration.Config; 

using Croc.Bpc.Election; 

using Croc.Bpc.FileSystem; 

using Croc.Core; 

using Croc.Core.Configuration; 

using Croc.Core.Utils; 

using Croc.Core.Utils.Xml; 

 

 

namespace Croc.Bpc.Configuration 

{ 

    /// <summary> 

    /// Менеджер конфигурации 

    /// </summary> 

    [SubsystemConfigurationElementTypeAttribute(typeof(ConfigurationManagerConfig))] 

    public class ConfigurationManager : 

        Subsystem, //StateSubsystem,  

        IConfigurationManager 

    { 

        /// <summary> 

        /// Формат-строка для создания конфиг-файла 

        /// </summary> 

        private const string CONFIG_FILE_CONTENT_FORMAT = 

            "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n" + 

            "<configuration>\n" + 

            "  <configSections>\n" + 

            "    <section name=\"croc.application\" type=\"Croc.Core.Configuration.ApplicationConfig, Croc.Core\" />\n" + 

            "  </configSections>\n" + 

            "  {0}\n" + // точка для вставки текста секции croc.application 

            "</configuration>"; 

 

 

        /// <summary> 

        /// Конфиг данного менеджера 

        /// </summary> 

        private ConfigurationManagerConfig _config; 

        /// <summary> 

        /// Утилита для выполнения слияния xml-ля конфигов 

        /// </summary> 

        private XmlMerge _mergeUtil; 

        /// <summary> 

        /// Текущий конфиг приложения 

        /// </summary> 

        private ApplicationConfig _currentConfig; 


        /// <summary> 

        /// Xml-строка, представляющая текущий конфиг приложения 

        /// </summary> 

        private string _currentConfigXml; 

        /// <summary> 

        /// Менеджер файловой системы 

        /// </summary> 

        private IFileSystemManager _fileSystemManager; 

		/// <summary> 

		/// Менеджер выборов 

		/// </summary> 

		private IElectionManager _electionManager; 

 

 

 

 

        /// <summary> 

        /// Инициализация 

        /// </summary> 

        /// <param name="config"></param> 

        public override void Init(SubsystemConfig config) 

        { 

            _config = (ConfigurationManagerConfig)config; 

 

 

            // подготовим утилиту для слияния xml-ля конфигов 

            var keyAttributeNames = new Dictionary<string, string>(); 

            keyAttributeNames.Add("subsystem", "type"); 

 

 

            _mergeUtil = new XmlMerge(_config.PrivateConfigElementXPaths.ToList(), keyAttributeNames); 

 

 

            // изначально текущей конфигурацией является основая конфигурация приложения,  

            // загруженная при старте приложения из exe.config-файла 

            _currentConfig = CoreApplication.Instance.Config; 

            _currentConfigXml = _currentConfig.ToXml(); 

 

 

            // получим ссылки на другие подсистемы 

            _fileSystemManager = Application.GetSubsystemOrThrow<IFileSystemManager>(); 

			_electionManager = Application.GetSubsystemOrThrow<IElectionManager>(); 

 

 

			// подпишемся на событие изменения конфигурации 

			Application.ConfigUpdated += new EventHandler<ConfigUpdatedEventArgs>(ApplicationConfigUpdated); 

		} 

 

 

		#region Обработка события изменения конфигурации приложения 


 
 

		/// <summary> 

		/// Обработка события изменения конфигурации приложения 

		/// </summary> 

		/// <param name="sender">объект пославший событие</param> 

		/// <param name="e">параметры события</param> 

		private void ApplicationConfigUpdated(object sender, ConfigUpdatedEventArgs e) 

		{ 

			// поменяем текущий конфиг 

			_currentConfig = Application.Config; 

			_currentConfigXml = Application.Config.ToXml(); 

 

 

			// сохраним рабочий конфиг 

			SaveWorkingConfig(); 

		} 

 

 

		#endregion 

 

 

		#region IConfigurationManager 

 

 

		/// <summary> 

        /// Загрузить рабочую конфигурацию 

        /// </summary> 

        /// <returns> 

        /// true - загрузили или рабочая конфигурация не найдена 

        /// false - ошибка загрузки 

        /// </returns> 

        public bool LoadWorkingConfig() 

        { 

            // если рабочий конфиг-файл не существует 

            if (!File.Exists(_config.WorkingConfigFilePath.Value)) 

                return true; 

 

 

            try 

            { 

                // загружаем конфигурацию из рабочего конфиг-файла 

                _currentConfig = ConfigurationUtils.GetSection<ApplicationConfig>( 

                    _config.WorkingConfigFilePath.Value, ApplicationConfig.SectionName); 

                _currentConfigXml = _currentConfig.ToXml(); 

 

 

                return true; 

            } 

            catch (Exception ex) 


            { 

                Logger.LogException(Message.ConfigLoadWorkingException, ex); 

                return false; 

            } 

        } 

 

 

        /// <summary> 

        /// Загрузить частную конфигурацию 

        /// </summary> 

        /// <param name="partialConfigXml">содержимое частного-конфиг файла. 

        /// Если null, то метод сам выполняет поиск и чтение частного-конфиг файла, 

        /// иначе - загрузка частной конфигурации выполняется из данного параметра</param> 

        /// <returns> 

        /// true - загрузили или частная конфигурация не найдена 

        /// false - ошибка загрузки 

        /// </returns> 

        public bool LoadPartialConfig(ref string partialConfigXml) 

        { 

            // если содержимое частного конфиг-файла не передано 

            if (string.IsNullOrEmpty(partialConfigXml)) 

            { 

                // найдем частный конфиг-файл 

                var partialConfigFile = FindPartialConfigFile(); 

                if (partialConfigFile == null) 

                    // частного конфиг-файла нет 

                    return true; 

 

 

                try 

                { 

                    // читаем содержимое частного конфиг-файла 

                    using (var reader = partialConfigFile.OpenText()) 

                        partialConfigXml = reader.ReadToEnd(); 

                } 

                catch (Exception ex) 

                { 

                    Logger.LogException(Message.ConfigReadPartialException, ex); 

                    return false; 

                } 

            } 

 

 

            try 

            { 

                // выполняем слияние частной конфигурации и текущей 

 

 

                // если что-то было слито 

                if (_mergeUtil.Merge(partialConfigXml, _currentConfigXml)) 


                { 

                    // загружаем конфиг из полученного после слияния xml 

                    string resXml; 

                    // делаем так, чтобы получить отформатированный xml, 

                    // иначе, если просто использовать _mergeUtil.Result.OuterXml,  

                    // то получаем xml в одну строку, а при его десериализации в Моно ошибка 

                    using (var memStream = new MemoryStream()) 

                    { 

                        using (var xmlTextWriter = new XmlTextWriter(memStream, Encoding.Unicode)) 

                        { 

                            xmlTextWriter.Formatting = Formatting.Indented; 

                            xmlTextWriter.Indentation = 4; 

                            xmlTextWriter.QuoteChar = '\''; 

 

 

                            _mergeUtil.Result.WriteTo(xmlTextWriter); 

 

 

                            xmlTextWriter.Flush(); 

                            memStream.Position = 0; 

 

 

                            using (var streamReader = new StreamReader(memStream)) 

                            { 

                                resXml = streamReader.ReadToEnd(); 

                            } 

                        } 

                    } 

 

 

                    _currentConfig = ApplicationConfig.FromXml(resXml); 

                    _currentConfigXml = resXml; 

                } 

 

 

                return true; 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.ConfigLoadPartialException, ex); 

                return false; 

            } 

        } 

 

 

        /// <summary> 

        /// Выполняет поиск частного конфиг-файла 

        /// </summary> 

        /// <returns>null, если частный конфиг-файла не найден</returns> 

        private FileInfo FindPartialConfigFile() 


        { 

			// если нужно искать рядом с ИД 

			if (_config.PartialConfigFileLocations.IncludeSourceDataPaths) 

			{ 

				foreach (var dir in _electionManager.GetSourceDataSearchPaths()) 

				{ 

					var fileName = Path.Combine(dir, _config.PartialConfigFileLocations.FileName); 

					try 

					{ 

						var fileInfo = new FileInfo(fileName); 

						// если есть файл 

						if (fileInfo.Exists) 

							return fileInfo; 

					} 

					catch (Exception ex) 

					{ 

						Logger.LogException(Message.ConfigFindPartialError, ex, fileName); 

					} 

				} 

			} 

 

 

			// проверим дополнительные пути 

            foreach (ValueConfig<string> item in _config.PartialConfigFileLocations) 

            { 

				var fileName = Path.Combine(item.Value, _config.PartialConfigFileLocations.FileName); 

				try 

                { 

					var fileInfo = new FileInfo(fileName); 

                    // если есть файл 

					if (fileInfo.Exists) 

                        return fileInfo; 

                } 

                catch (Exception ex) 

                { 

					Logger.LogException(Message.ConfigFindPartialError, ex, fileName); 

                } 

            } 

 

 

            return null; 

        } 

 

 

        /// <summary> 

        /// Сбросить рабочую конфигурацию в начальное состояние 

        /// </summary> 

        public void ResetWorkingConfig() 

        { 

            // достаточно удалить рабочий конфиг-файл 


            try 

            { 

                File.Delete(_config.WorkingConfigFilePath.Value); 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.ConfigException, ex, "Ошибка удаления рабочего конфиг-файла"); 

            } 

        } 

 

 

        /// <summary> 

        /// Применить конфигурацию 

        /// </summary> 

        /// <returns>true - конфигурация успешно применена, false - ошибка применения конфигурации</returns> 

        public bool ApplyConfig() 

        { 

            // применяем конфигурацию 

            try 

            { 

                var res = CoreApplication.Instance.ApplyNewConfig(_currentConfig, true); 

                // если применение выполнено 

                if (res) 

                { 

					SaveWorkingConfig(); 

                } 

 

 

                return true; 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.ConfigException, ex, "Ошибка применения конфигурации"); 

                return false; 

            } 

        } 

 

 

		/// <summary> 

		/// Сохранить рабочий конфиг 

		/// </summary> 

		private void SaveWorkingConfig() 

		{ 

			string tempFileName = 

				_fileSystemManager.GetTempFileName( 

				new FileInfo(_config.WorkingConfigFilePath.Value).DirectoryName); 

 

 

			// то сохраняем конфигурацию в рабочий конфиг-файл 

			var text = string.Format(CONFIG_FILE_CONTENT_FORMAT, _currentConfigXml); 


 
 

			_fileSystemManager.WriteTextToFile( 

				FileType.RuntimeData, 

				tempFileName, 

				FileMode.Create, 

				text); 

 

 

			_fileSystemManager.Sync(); 

 

 

			if (File.Exists(_config.WorkingConfigFilePath.Value)) 

			{ 

				File.Delete(_config.WorkingConfigFilePath.Value); 

			} 

 

 

			File.Move(tempFileName, _config.WorkingConfigFilePath.Value); 

		} 

 

 

        #endregion 

 

 

        #region StateSubsystem overrides 

 

 

        //TODO: состояние подсистемы конфигурации - это собственно сама конфигурация. 

        // данная подсистема сама реализует хранение конифга, но нужно ведь уметь конфигом обмениваться 

        // (синхронизироваться) с удаленным сканером... 

 

 

        //- не нужно выделять отдельно получение частного конфига, а надо делать полную синхронизацию, 

        //    но конфиг все равно синхронизируется не весь - есть секции, которые чисто личные для сканера 

        //      => эти частные секции не должны включаться в состояние 

        //- ситуация: работает 1 сканер, на нем через меню изменили конфиг, потом подключили второй -  

        //    он должен подтянуть все изменения конфига, сделанные на первом. 

 

 

        #endregion 

    } 

}



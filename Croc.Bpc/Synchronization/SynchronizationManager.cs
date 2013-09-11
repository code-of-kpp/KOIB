using System; 

using System.Collections.Generic; 

using System.Collections.Specialized; 

using System.IO; 

using System.Runtime.Remoting; 

using System.Runtime.Remoting.Channels; 

using System.Runtime.Remoting.Channels.Tcp; 

using System.Runtime.Serialization; 

using System.Runtime.Serialization.Formatters.Binary; 

using System.Security.Permissions; 

using System.Text; 

using System.Threading; 

using Croc.Bpc.Common; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Bpc.Election; 

using Croc.Bpc.FileSystem; 

using Croc.Bpc.Printing; 

using Croc.Bpc.Printing.Reports; 

using Croc.Bpc.Scanner; 

using Croc.Bpc.Synchronization.Config; 

using Croc.Core; 

using Croc.Core.Configuration; 

using Croc.Core.Extensions; 

using Croc.Core.Utils.IO; 

using Message = Croc.Bpc.Common.Diagnostics.Message; 

using Croc.Bpc.Configuration; 

 

 

namespace Croc.Bpc.Synchronization 

{ 

    /// <summary> 

    /// Менеджер синхронизации сканеров 

    /// </summary> 

    /// <remarks> 

    /// К задачам данного менеджера относятся: 

    /// - хранение информации о сканерах:  

    ///     - кто локальный, кто удаленный 

    ///     - кто главный, кто подчиненный 

    /// - управление соединением с удаленным сканером 

    ///     - создание соединения 

    ///     - отслеживание обрыва соединения 

    /// - синхронизация сканеров 

    /// </remarks> 

    [SubsystemConfigurationElementTypeAttribute(typeof(SynchronizationManagerConfig))] 

    public class SynchronizationManager :  

        Subsystem,  

        ISynchronizationManager,  

        IScannerInteractionChannel 

    { 

        /// <summary> 


        /// Конфигурация подсистемы 

        /// </summary> 

        private SynchronizationManagerConfig _config; 

        /// <summary> 

        /// Менеджер сканера 

        /// </summary> 

        private IScannerManager _scannerManager; 

        /// <summary> 

        /// Менеджер выборов 

        /// </summary> 

        private IElectionManager _electionManager; 

		/// <summary> 

		/// Менеджер файловой системы 

		/// </summary> 

		private IFileSystemManager _fileSystemManager; 

		/// <summary> 

		/// Менеджер файловой системы 

		/// </summary> 

		private IPrintingManager _printingManager; 

 

 

		/// <summary> 

		/// Менаджер конфигурации 

		/// </summary> 

		private IConfigurationManager _configManager; 

 

 

        /// <summary> 

        /// Инициализация подсистемы 

        /// </summary> 

        /// <param name="config"></param> 

        public override void Init(SubsystemConfig config) 

        { 

            _config = (SynchronizationManagerConfig)config; 

 

 

            // инициализируем взаимодействие с удаленным сканером 

            InitRemoteScannerCommunication(); 

 

 

            // получим ссылки на другие подсистемы 

            _scannerManager = Application.GetSubsystemOrThrow<IScannerManager>(); 

			_scannerManager.RemoteScannerConnected +=  

                new EventHandler<ScannerEventArgs>(ScannerManager_RemoteScannerConnected); 

 

 

            _fileSystemManager = Application.GetSubsystemOrThrow<IFileSystemManager>(); 

            _electionManager = Application.GetSubsystemOrThrow<IElectionManager>(); 

			_printingManager = Application.GetSubsystemOrThrow<IPrintingManager>(); 

			_configManager = Application.GetSubsystemOrThrow<IConfigurationManager>(); 


 
 

            // инициализация состояния 

            InitState(); 

        } 

 

 

        #region Установка соединения с удаленным сканером 

 

 

        /// <summary> 

        /// Порт для удаленного взаимодействия 

        /// </summary> 

        private const int REMOTE_INTERACTION_PORT = 9090; 

        /// <summary> 

        /// Имя объекта взаимодействия 

        /// </summary> 

        private const string REMOTE_INTERACTION_OBJECT = "RemoteInteraction.rem"; 

        /// <summary> 

        /// Формат-строка для Uri для удаленного взаимодействия 

        /// </summary> 

        private const string REMOTE_INTERACTION_URI_FORMAT = "tcp://{0}:{1}/" + REMOTE_INTERACTION_OBJECT; 

        /// <summary> 

        /// Канал, по которому слушаем входящие запросы от удаленных сканеров 

        /// </summary> 

        private TcpChannel _hostChannel; 

        /// <summary> 

        /// Информация о локальном сканере 

        /// </summary> 

        private SyncScannerInfo _localScannerInfo; 

        /// <summary> 

        /// Информация об удаленном сканере 

        /// </summary> 

        private SyncScannerInfo _remoteScannerInfo; 

        /// <summary> 

        /// Интерфейс для работы с удаленным сканером 

        /// </summary> 

        private RemoteScannerInterface _remoteScannerInterface; 

        /// <summary> 

        /// Объект синхронизации работы с удаленным сканером 

        /// </summary> 

        private static object s_remoteScannerSync = new object(); 

 

 

        /// <summary> 

        /// Событие "Соединение с удаленным сканером установлено" 

        /// </summary> 

        public event EventHandler RemoteScannerConnected; 

        /// <summary> 

        /// Событие "Связь с удаленным сканером потеряна" 


        /// </summary> 

        public event EventHandler RemoteScannerDisconnected; 

        /// <summary> 

        /// Событие "Связь с удаленным сканером потеряна" 

        /// </summary> 

        public ManualResetEvent _remoteScannerDisconnected = new ManualResetEvent(true); 

 

 

        /// <summary> 

        /// Открыть канал для подключения удаленных сканеров 

        /// </summary> 

        /// <param name="localSerialNumber">Серийный номер локального сканера</param> 

        /// <param name="localIPAddress">IP-адрес локального сканера</param> 

        public void OpenIncomingInteractionChannel(string localSerialNumber, string localIPAddress) 

        { 

            CodeContract.Requires(!string.IsNullOrEmpty(localSerialNumber)); 

            CodeContract.Requires(!string.IsNullOrEmpty(localIPAddress)); 

 

 

            // создадим инф-цию о локальном сканере 

            _localScannerInfo = new SyncScannerInfo(localSerialNumber, localIPAddress); 

 

 

            try 

            { 

                // если канал был ранее открыт, то закроем его 

                CloseIncomingInteractionChannel(); 

 

 

                // регистрируем канал 

                _hostChannel = new TcpChannel(REMOTE_INTERACTION_PORT); 

                ChannelServices.RegisterChannel(_hostChannel, false); 

 

 

                Logger.LogInfo(Message.SyncChannelOpened, localIPAddress); 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.SyncException, ex, "Ошибка открытия канала для подключения удаленных сканеров"); 

            } 

        } 

 

 

        /// <summary> 

        /// Закрывает канал для подключения удаленных сканеров 

        /// </summary> 

        private void CloseIncomingInteractionChannel() 

        { 

            if (_hostChannel != null) 

            { 


                try 

                { 

                    ChannelServices.UnregisterChannel(_hostChannel); 

                } 

                catch { } 

 

 

                _hostChannel = null; 

                Logger.LogInfo(Message.SyncChannelClosed); 

            } 

        } 

 

 

        /// <summary> 

        /// Создает Uri для удаленного взаимодействия 

        /// </summary> 

        /// <param name="ipAddress"></param> 

        /// <returns></returns> 

        private string GetUriForRemoteInteraction(string ipAddress) 

        { 

            return string.Format(REMOTE_INTERACTION_URI_FORMAT, ipAddress, REMOTE_INTERACTION_PORT); 

        } 

 

 

        /// <summary> 

        /// Инициализация взаимодействия с удаленным сканером 

        /// </summary> 

        private void InitRemoteScannerCommunication() 

        { 

            // создадим интерфейс для работы с удаленным сканером 

            _remoteScannerInterface = new RemoteScannerInterface(Logger,  

                _config.RemoteScannerCallProperties.Common 

                , _config.RemoteScannerCallProperties.Synchronization 

                , _config.RemoteScannerCallProperties.Printing); 

 

 

            // подпишемся на событие потери связи с удаленным сканером 

            _remoteScannerInterface.Disconnected += new EventHandler(RemoteScannerInterface_Disconnected); 

 

 

            // в качестве канала для доступа к локальному сканеру возвращать будет менеджера данного сканера 

            RemoteScannerConnector.GetChannelToLocalScannerEvent += 

                new RemoteScannerConnector.GetChannelToLocalScannerDelegate(() => { return this; }); 

 

 

            RemoteScannerConnector.IsRemoteConnectionAllowEvent += 

                new RemoteScannerConnector.IsRemoteConnectionAllowDelegate(OnIsRemoteConnectionAllow); 

 

 

            RemotingConfiguration.RegisterWellKnownServiceType( 


                typeof(RemoteScannerConnector), 

                REMOTE_INTERACTION_OBJECT, 

                WellKnownObjectMode.Singleton); 

        } 

 

 

        /// <summary> 

        /// Проверяет, разрешено ли удаленному сканеру установить подключение к данному сканеру 

        /// </summary> 

        /// <param name="serialNumber"></param> 

        /// <param name="ipAddress"></param> 

        /// <returns></returns> 

        private bool OnIsRemoteConnectionAllow(string serialNumber, string ipAddress) 

        { 

            if (!IsKnownRemoteScanner(serialNumber, ipAddress)) 

            { 

                Logger.LogVerbose(Message.SyncConnectRejected, serialNumber, ipAddress); 

                return false; 

            } 

 

 

            Logger.LogVerbose(Message.SyncConnectAccepted, serialNumber, ipAddress); 

            return true; 

        } 

 

 

        /// <summary> 

        /// Это известный нам удаленный сканер? 

        /// </summary> 

        /// <param name="remoteSerialNumber">серийный номер удаленного сканера</param> 

        /// <param name="remoteIPAddress">ip-адрес удаленного сканера</param> 

        private bool IsKnownRemoteScanner(string remoteSerialNumber, string remoteIPAddress) 

        { 

            Logger.LogVerbose(Message.LockTryEnter, "s_remoteScannerSync"); 

            lock (s_remoteScannerSync) 

            { 

                Logger.LogVerbose(Message.LockDone, "s_remoteScannerSync"); 

 

 

                return _remoteScannerInfo != null && 

                    _remoteScannerInfo.Equals(new SyncScannerInfo(remoteSerialNumber, remoteIPAddress)); 

            } 

        } 

 

 

        /// <summary> 

        /// Обработчик события подключения удаленного сканера 

        /// </summary> 

        /// <param name="sender"></param> 

        /// <param name="e"></param> 


        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.RemotingConfiguration)] 

        private void ScannerManager_RemoteScannerConnected(object sender, ScannerEventArgs e) 

        { 

            //Logger.LogVerbose(Message.LockTryEnter, "s_remoteScannerSync"); 

            lock (s_remoteScannerSync) 

            { 

                //Logger.LogVerbose(Message.LockDone, "s_remoteScannerSync"); 

 

 

                // если информация об удаленном сканере уже есть и соединение с ним установлено 

                if (_remoteScannerInfo != null && 

                    _remoteScannerInterface.Alive) 

                    // игнорируем сигнал 

                    return; 

 

 

                // запомним информацию об удаленном сканере 

                _remoteScannerInfo = new SyncScannerInfo(e.SerialNumber, e.IPAddress); 

            } 

 

 

            // пробуем установить соединение 

            try 

            { 

                Logger.LogVerbose(Message.DebugVerbose, "Пробуем установить соединение с удаленным сканером"); 

 

 

                // получаем коннектор 

                var uri = GetUriForRemoteInteraction(e.IPAddress); 

                Logger.LogVerbose(Message.DebugVerbose, "Получаем коннектор, Uri: " + uri); 

                var connector = (RemoteScannerConnector)Activator.GetObject(typeof(RemoteScannerConnector), uri); 

 

 

                // проверим, можно ли нам подключаться 

                var allowed = connector.IsRemoteConnectionAllow( 

                    _localScannerInfo.SerialNumber, _localScannerInfo.IPAddress); 

 

 

                if (!allowed) 

                { 

                    Logger.LogVerbose(Message.DebugVerbose, "Удаленное подключение запрещено!"); 

                    return; 

                } 

 

 

                // установим в интерфейсе канал для доступа к удаленному сканеру 

                _remoteScannerInterface.SetInteractionChannel(connector); 

 

 

                // запускаем поток отслеживания того, что удаленный сканер все еще на связи 


                ThreadPool.QueueUserWorkItem((state) => 

                { 

                    Logger.LogVerbose(Message.DebugVerbose, 

                        "Запущен поток отслеживания наличия связи с удаленным сканером"); 

 

 

                    while (true) 

                    { 

                        _remoteScannerInterface.Ping(); 

 

 

                        if (!_remoteScannerInterface.Alive || 

                            _disposeEvent.WaitOne(TimeSpan.FromSeconds(1), false)) 

                            return; 

                    } 

                }); 

 

 

                RemoteScannerConnected.RaiseEvent(this); 

                _remoteScannerDisconnected.Reset(); 

                Logger.LogVerbose(Message.DebugVerbose, "Подключение к удаленному сканеру установлено"); 

            } 

            catch (RemotingException ex) 

            { 

                Logger.LogInfo(Message.SyncCannotConnectToRemoteScanner, ex.Message); 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.SyncException, ex, "Не удалось подключиться к удаленному сканеру"); 

            } 

        } 

 

 

        /// <summary> 

        /// Проверка связи 

        /// </summary> 

        void IScannerInteractionChannel.Ping() 

        { 

            // то, что данный метод вызвался - уже говорит о том,  

            // что удаленный сканер смог достучаться до данного 

        } 

 

 

        /// <summary> 

        /// Обработчик события потери соединения с удаленным сканером 

        /// </summary> 

        /// <param name="sender"></param> 

        /// <param name="e"></param> 

        private void RemoteScannerInterface_Disconnected(object sender, EventArgs e) 

        { 


            // на случай, если был запущен процесс синхронизации, сообщим о его завершении 

            StateSynchronizationFinished(SynchronizationResult.RemoteScannerDisconnected); 

 

 

            //сообщим, что удаленный сканер потерян 

            RemoteScannerDisconnected.RaiseEvent(this, e); 

            _remoteScannerDisconnected.Set(); 

        } 

 

 

        #endregion 

 

 

        #region Работа с удаленным сканером 

 

 

        /// <summary> 

        /// Возвращает версию приложения 

        /// </summary> 

        Version IScannerInteractionChannel.ApplicationVersion 

        { 

            get 

            { 

                return CoreApplication.Instance.ApplicationVersion; 

            } 

        } 

 

 

        /// <summary> 

        /// Устанавливает текущую дату и время на сканере 

        /// </summary> 

        /// <param name="utcDateTime"></param> 

        void IScannerInteractionChannel.SetSystemTime(DateTime utcDateTime) 

        { 

            SystemHelper.SetSystemTime(utcDateTime); 

            Logger.LogInfo(Message.SyncSetSystemTime, utcDateTime); 

        } 

 

 

        /// <summary> 

        /// Сейчас день выборов? 

        /// </summary> 

        bool IScannerInteractionChannel.IsElectionDayNow 

        { 

            get 

            { 

                return _electionManager.IsElectionDayNow(!_isStateInitial); 

            } 

        } 

 


 
        /// <summary> 

        /// Идентификатор исходных данных 

        /// </summary> 

        Guid IScannerInteractionChannel.SourceDataId 

        { 

            get 

            { 

                return _electionManager.SourceData == null ? Guid.Empty : _electionManager.SourceData.Id; 

            } 

        } 

 

 

        /// <summary> 

        /// Интерфейс доступа к удаленному сканеру 

        /// </summary> 

        public IScannerInteractionChannel RemoteScanner 

        { 

            get 

            { 

                return _remoteScannerInterface; 

            } 

        } 

 

 

        /// <summary> 

        /// Удаленный сканер подключен? 

        /// </summary> 

        public bool IsRemoteScannerConnected 

        { 

            get 

            { 

                return _remoteScannerInterface.Alive; 

            } 

        } 

 

 

		/// <summary> 

		/// Подключен ли принтер к удаленному сканеру сканеру 

		/// </summary> 

		/// <returns>true - подключен/false - нет</returns> 

		bool IScannerInteractionChannel.FindRemotePrinter() 

		{ 

			return _printingManager.FindPrinter(); 

		} 

 

 

		/// <summary> 

		/// Распечатать отчет на удаленном принтере 

		/// </summary> 


		/// <returns></returns> 

		bool IScannerInteractionChannel.RemotePrintReport(ReportType reportType, ListDictionary reportParameters) 

		{ 

			return _printingManager.PrintReport(reportType, reportParameters); 

		} 

 

 

		/// <summary> 

		/// Сброс По 

		/// </summary> 

		void IScannerInteractionChannel.ResetSoft() 

		{ 

			// получим префикс архива в зависимости от реальности выборов 

			var archivePrefix = _electionManager.SourceData.IsReal ? String.Empty : "test_"; 

			// архивируем файлы 

			_fileSystemManager.ArchiveFiles(archivePrefix); 

 

 

			// сбрасываем рабочий конфиг 

			_configManager.ResetWorkingConfig(); 

 

 

			// перезапустим приложение 

			Application.Exit(Croc.Core.ApplicationExitType.RestartApplication); 

		} 

 

 

        #endregion 

 

 

        #region Обмен данными с удаленным сканером 

 

 

        /// <summary> 

        /// Таблица данных, которые были переданы с удаленного сканера 

        /// </summary> 

        private Dictionary<string, object> _transmittedData = new Dictionary<string, object>(); 

        /// <summary> 

        /// Объект синхронизации доступа к таблице с данными, переданными с удаленного сканера 

        /// </summary> 

        private static object s_transmittedDataSync = new object(); 

        /// <summary> 

        /// Событие "Удаленный сканер передал новые данные" 

        /// </summary> 

        private ManualResetEvent _newDataTransmitted = new ManualResetEvent(false); 

 

 

        /// <summary> 

        /// Получить данные, которые были переданы с удаленного сканера. 

        /// Если данных нет, то ожидается их поступление 


        /// </summary> 

        /// <param name="name">имя данных</param> 

        /// <param name="waitCtrl">контроллер ожидания событий. Может привносить свою логику в метод,  

        /// например, генерировать исключение в определенных случаях. Например, 

        /// если контроллер - это WorkflowExecutionContext, то во время ожидания он может сгенерировать  

        /// исключение ActivityExecutionInterruptException, как результат обработки сигнала  

        /// о потере связи со 2-м сканером</param> 

        /// <returns>null, если был вызван деструктор или связь с удаленным сканером была потеряна, 

        /// или данные, которые ожидались</returns> 

        public object GetDataTransmittedFromRemoteScanner(string name, IWaitController waitCtrl) 

        { 

            CodeContract.Requires(!string.IsNullOrEmpty(name)); 

 

 

            while (true) 

            { 

                var data = FindTransmittedData(name); 

                if (data != null) 

                    return data; 

 

 

                int index; 

                if (!WaitAny(new[] { _remoteScannerDisconnected, _newDataTransmitted }, out index, waitCtrl) || 

                    index == 0) 

                { 

                    // был вызван деструктор или потеряна связь с удаленным сканером 

                    return null; 

                } 

 

 

                // получены данных  

                // => идем на след. итерацию, чтобы проверить, нужные ли нам данные были получены 

            } 

        } 

 

 

        /// <summary> 

        /// Выполняет поиск данных с заданным именем в таблице данных, переданных с удаленного сканера 

        /// </summary> 

        /// <param name="name"></param> 

        /// <returns>null - если данные не найдены, иначе - данные</returns> 

        private object FindTransmittedData(string name) 

        { 

            lock (s_transmittedDataSync) 

            { 

                object res = null; 

 

 

                if (_transmittedData.ContainsKey(name)) 

                { 


                    res = _transmittedData[name]; 

                    _transmittedData.Remove(name); 

                } 

 

 

                _newDataTransmitted.Reset(); 

                return res; 

            } 

        } 

 

 

        /// <summary> 

        /// Положить данные в таблицу данных, которые передал удаленный сканер 

        /// </summary> 

        /// <param name="name">имя данных</param> 

        /// <param name="data">данные</param> 

        void IScannerInteractionChannel.PutData(string name, object data) 

        { 

            CodeContract.Requires(!string.IsNullOrEmpty(name)); 

            CodeContract.Requires(data != null); 

 

 

            lock (s_transmittedDataSync) 

            { 

                _transmittedData[name] = data; 

                _newDataTransmitted.Set(); 

            } 

        } 

 

 

        #endregion 

 

 

        #region Роль сканера 

 

 

        /// <summary> 

        /// Роль сканера изменилась 

        /// </summary> 

        public event EventHandler ScannerRoleChanged; 

 

 

        /// <summary> 

        /// Событие "Роль сканера определлена" 

        /// </summary> 

        private ManualResetEvent _scannerRoleDefined = new ManualResetEvent(false); 

 

 

        /// <summary> 

        /// Возбудить событие "Роль сканера изменилась" на удаленном сканере 


        /// </summary> 

        /// <remarks>Вызов этого метода "приходит" с удаленного сканера</remarks> 

        void IScannerInteractionChannel.RaiseRemoteScannerRoleChanged() 

        { 

            _remoteScannerInterface.RaiseScannerRoleChanged(); 

        } 

 

 

        /// <summary> 

        /// Роль сканера 

        /// </summary> 

        private ScannerRole _scannerRole = ScannerRole.Undefined; 

        /// <summary> 

        /// Роль данного сканера 

        /// </summary> 

        public ScannerRole ScannerRole 

        { 

            get 

            { 

                return _scannerRole; 

            } 

            set 

            { 

                // если роль не изменится 

                if (_scannerRole == value) 

                    // то ничего не делаем 

                    return; 

 

 

                _scannerRole = value; 

                Logger.LogInfo(Message.SyncScannerRoleSet, _scannerRole); 

 

 

                if (_scannerRole == ScannerRole.Undefined) 

                    _scannerRoleDefined.Reset(); 

                else 

                    _scannerRoleDefined.Set(); 

 

 

                // возбуждаем событие на данном сканере 

                ScannerRoleChanged.RaiseEvent(this); 

                // возбуждаем событие на удаленном сканере 

                _remoteScannerInterface.RaiseRemoteScannerRoleChanged(); 

            } 

        } 

 

 

        /// <summary> 

        /// Ожидает, когда роль сканера будет определена 

        /// </summary> 


        /// <returns>роль, которую принял сканер</returns> 

        ScannerRole IScannerInteractionChannel.WaitForScannerRoleDefined() 

        { 

            // если роль уже определена 

            if (ScannerRole != ScannerRole.Undefined) 

                // вернем ее 

                return ScannerRole; 

 

 

            // иначе - ждем, когда роль будет определена 

            return WaitOne(_scannerRoleDefined, null) 

                ? ScannerRole // роль определена 

                : ScannerRole.Undefined; // был вызван деструктор 

        } 

 

 

        #endregion 

 

 

        #region Работа с состоянием 

 

 

        /// <summary> 

        /// Имя файла с состоянием 

        /// </summary> 

        private const string STATE_FILE_NAME = "state"; 

        /// <summary> 

        /// Расширение файла с состоянием 

        /// </summary> 

        private const string STATE_FILE_EXT = "bin"; 

        /// <summary> 

        /// Путь к файлу состояния 

        /// </summary> 

        private string _stateFilePath; 

        /// <summary> 

        /// Форматер для сериализации состояния 

        /// </summary> 

        private IFormatter _stateSerializationFormatter = new BinaryFormatter(); 

        /// <summary> 

        /// Таблица состояний подсистем: [имя подсистемы, состояние подсистемы] 

        /// </summary> 

        private Dictionary<string, StateItem> _stateDict; 

        /// <summary> 

        /// Объект для синхронизации доступа к состоянию 

        /// </summary> 

        private static object s_stateSync = new object(); 

        /// <summary> 

        /// Имя блокировки (для логирования) 

        /// </summary> 

        private const string STATE_SYNC = "StateSync"; 


        /// <summary> 

        /// Признак того, что состояние в начальном положении 

        /// </summary> 

        private bool _isStateInitial = true; 

        /// <summary> 

        /// Текущее состояние - начальное? 

        /// </summary> 

        /// <remarks>начальное - т.е. оно еще не было ниоткуда загружено или  

        /// получено с удаленного сканера или оно было сброшено в начальное состояние</remarks> 

        bool IScannerInteractionChannel.IsStateInitial 

        { 

            get 

            { 

                return _isStateInitial; 

            } 

        } 

        /// <summary> 

        /// Выполняется ли синхронизация в данный момент 

        /// </summary> 

        /// <remarks>используется для того, чтобы избежать рекурсивной синхронизации, которая  

        /// может возникать, когда в процессе синхронизации мы устанавливаем какой-нибудь подсистеме  

        /// новое состояние, а она вследствие этого запускает синхронизацию еще раз</remarks> 

        private volatile bool _stateSynchronizingNow = false; 

        /// <summary> 

        /// Событие "Синхронизация завершена" 

        /// </summary> 

        private ManualResetEvent _synchronizationFinished = new ManualResetEvent(false); 

        /// <summary> 

        /// Результат последней выполненной синхронизации 

        /// </summary> 

        private volatile SynchronizationResult _lastSynchronizationResult = SynchronizationResult.Succeeded; 

 

 

        /// <summary> 

        /// Включена ли синхронизация 

        /// </summary> 

        /// <remarks> 

        /// если синхронизация состояния выключена, то при изменении состояния  

        /// оно не сохраняется в файл состояния, а также игнорируются запросы  

        /// от удаленного сканера на синхронизацию состояния 

        /// </remarks> 

        private volatile bool _stateSynchronizationEnabled = false; 

        /// <summary> 

        /// Включение/выключение синхронизации состояния с удаленным сканером 

        /// </summary> 

        public bool SynchronizationEnabled 

        { 

            get 

            { 

 


 
                return _stateSynchronizationEnabled; 

            } 

            set 

            { 

                _stateSynchronizationEnabled = value; 

 

 

                if (!_stateSynchronizationEnabled) 

                    StateSynchronizationFinished(SynchronizationResult.SynchronizationDisabled); 

            } 

        } 

 

 

        /// <summary> 

        /// Инициализация работы с состоянием 

        /// </summary> 

        private void InitState() 

        { 

			// директория для хранения состояний 

			var stateDirectory = _fileSystemManager.GetDataDirectoryPath(FileType.State); 

            // вычислим путь к файлу состояния 

            _stateFilePath = Path.Combine( 

				stateDirectory,  

                string.Format("{0}.{1}", STATE_FILE_NAME, STATE_FILE_EXT)); 

 

 

            // получим список всех подсистем, которые имеют состояние 

            var stateSubsystems = Application.GetSubsystems<IStateSubsystem>(); 

 

 

            // создадим таблицу состояний подсистем 

            _stateDict = new Dictionary<string, StateItem>(stateSubsystems.Count); 

 

 

            // заполним таблицу состояний пустыми состояними 

            // и подпишемся на событие изменения состояния подсистем  

            foreach (var item in stateSubsystems) 

            { 

                _stateDict[item.Key] = new StateItem(item.Key); 

                item.Value.StateChanged += new EventHandler<SubsystemStateEventArgs>(StateSubsystem_StateChanged); 

            } 

        } 

 

 

        /// <summary> 

        /// Обработка события "Состояние подсистемы изменилось" 

        /// </summary> 

        /// <param name="sender">подсистема, состояние которой изменилось</param> 

        /// <param name="e"></param> 


        private void StateSubsystem_StateChanged(object sender, SubsystemStateEventArgs e) 

        { 

            if (_stateSynchronizingNow) 

                return; 

 

 

            // сохраним состояние подсистемы 

            SaveSubsystemState(e.SubsystemName, e.State); 

            // запустим синхронизацию с удаленным сканером 

            StartStateSynchronization(false); 

        } 

 

 

        #region Синхронизация состояния 

 

 

        /// <param name="enableSync">нужно ли включить синхронизацию, если она выключена. 

        /// Если значение равно 

        /// true - синхронизация будет включена и выполнена 

        /// false - если будет выполнена, только если она уже включена</param> 

        public void StartStateSynchronization(bool enableSync) 

        { 

            Logger.LogVerbose(Message.DebugVerbose, "Начинаем синхронизацию состояния"); 

 

 

            try 

            { 

                _synchronizationFinished.Reset(); 

 

 

                // если нужно включить синхронизацию 

                if (enableSync) 

                { 

                    // включим синхронизацию 

                    SynchronizationEnabled = true; 

 

 

                    // и сохраним состояние в файл, т.к. оно может быть еще ни разу не сохранялось 

                    Logger.LogVerbose(Message.LockTryEnter, STATE_SYNC); 

                    lock (s_stateSync) 

                    { 

                        Logger.LogVerbose(Message.LockDone, STATE_SYNC); 

 

 

                        SaveState(); 

                    } 

                } 

 

 

                // если синхронизация на данном сканере выключена или 2-ого сканера нет 


                if (!_stateSynchronizationEnabled || !IsRemoteScannerConnected) 

                { 

                    Logger.LogVerbose(Message.DebugVerbose, "Синхронизация выключена или 2-ого сканера нет"); 

                    StateSynchronizationFinished(SynchronizationResult.SynchronizationNotEnabled); 

                    return; 

                } 

 

 

                Logger.LogVerbose(Message.LockTryEnter, STATE_SYNC); 

                lock (s_stateSync) 

                { 

                    Logger.LogVerbose(Message.LockDone, STATE_SYNC); 

 

 

                    // передаем свое состояние на удаленный сканер 

                    TransferStateToRemoteScanner(false); 

                } 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.SyncSynchronizationException, ex); 

                StateSynchronizationFinished(SynchronizationResult.Failed); 

            } 

        } 

 

 

        /// <summary> 

        /// Передать состояние данного сканера на удаленный сканер 

        /// </summary> 

        /// <param name="signalIfSynchronized">нужно ли просигнализировать,  

        /// если все элементы состояния уже синхронизированы</param> 

        private void TransferStateToRemoteScanner(bool signalIfSynchronized) 

        { 

            try 

            { 

                // сформируем список из элементов состояния для синхронизации 

                var stateItemsForSync = new List<StateItem>(); 

 

 

                foreach (var item in _stateDict) 

                    if (!item.Value.Synchronized) 

                    { 

                        signalIfSynchronized = false; 

                        stateItemsForSync.Add(item.Value); 

                    } 

                    else 

                        // если элемент состояния уже синхронизирован,  

                        // то передадим только его пустой клон (т.е. с таким же именем, но с пустым Value), 

                        // у которого проставлен признак синхронизированности 

                        stateItemsForSync.Add(item.Value.GetSynchronizedEmptyClone()); 


 
 

                if (signalIfSynchronized) 

                { 

                    StateSynchronizationFinished(SynchronizationResult.Succeeded); 

 

 

                    new Action(() => 

                    { 

                        _remoteScannerInterface.StateSynchronizationFinished(SynchronizationResult.Succeeded); 

                    }).BeginInvoke(null, null); 

                } 

                else 

                { 

                    Logger.LogVerbose(Message.DebugVerbose, GetStatesForSyncDescription(stateItemsForSync)); 

 

 

                    new Action(() => 

                    { 

                        _remoteScannerInterface.NeedSynchronizeState(stateItemsForSync); 

                    }).BeginInvoke(null, null); 

                } 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.SyncSynchronizationException, ex); 

                StateSynchronizationFinished(SynchronizationResult.Failed); 

            } 

        } 

 

 

        /// <summary> 

        /// Возвращает описание элементов состояния, которые отправляются на синхронизацию 

        /// </summary> 

        /// <param name="items"></param> 

        /// <returns></returns> 

        private string GetStatesForSyncDescription(List<StateItem> items) 

        { 

            var sb = new StringBuilder(); 

            sb.Append("Отправляем на синхронизацию элементы состояния: "); 

 

 

            foreach (var item in items) 

            { 

                sb.Append(item.Name); 

                if (item.Synchronized) 

                    sb.Append('+'); // признак того, что данный элемент состояния уже синхронизирован 

                sb.Append(','); 

            } 

 


 
            if (sb.Length > 0) 

                sb.Length -= 1; 

 

 

            return sb.ToString(); 

        } 

 

 

        /// <summary> 

        /// Нужно синхронизировать состояния 

        /// </summary> 

        /// <param name="newStateItems">элементы состояния, которые были изменены и  

        /// по которым требуется синхронизация</param> 

        void IScannerInteractionChannel.NeedSynchronizeState(List<StateItem> newStateItems) 

        { 

            // если синхронизация выключена 

            if (!_stateSynchronizationEnabled) 

            { 

                // то игнорируем новые состояния 

                // в этом случае, если удаленный сканер ждет подтверждения окончания синхронизации, 

                // то он продолжит ждать до тех пор, пока данный сканер сам не начнет процесс синхронизации 

                Logger.LogVerbose(Message.DebugVerbose, "Игнорируем запрос на синхронизацию"); 

                return; 

            } 

 

 

            // если идет сканирование, то не выполняем синхронизацию 

            if (_scannerManager.IsSheetScanning) 

            { 

                // в этом случае, если удаленный сканер ждет подтверждения окончания синхронизации, 

                // то он продолжит ждать до тех пор, пока данный сканер сам не начнет процесс синхронизации 

                Logger.LogInfo(Message.SyncStateRejectedByScanning); 

                return; 

            } 

 

 

            try 

            { 

                Logger.LogVerbose(Message.DebugVerbose, "Начинаем обработку запроса на синхронизацию"); 

 

 

                _stateSynchronizingNow = true; 

                _synchronizationFinished.Reset(); 

 

 

                Logger.LogVerbose(Message.LockTryEnter, STATE_SYNC); 

                lock (s_stateSync) 

                { 

                    Logger.LogVerbose(Message.LockDone, STATE_SYNC); 


 
 

                    bool needSave = false;  // признак, что нужно будет выполнить сохранение состояния 

 

 

                    foreach (var newStateItem in newStateItems) 

                    { 

                        var subsystemName = newStateItem.Name; 

 

 

                        Logger.LogVerbose(Message.DebugVerbose, "Синхронизация состояния подсистемы: " + subsystemName); 

 

 

                        if (!_stateDict.ContainsKey(subsystemName)) 

                            continue; 

 

 

                        var curStateItem = _stateDict[subsystemName]; 

 

 

                        if (newStateItem.Synchronized) 

                        { 

                            Logger.LogVerbose(Message.DebugVerbose, "Синхронизация не требуется"); 

                            curStateItem.Synchronized = true; 

                        } 

                        else 

                        { 

                            var subsystem = (IStateSubsystem)Application.GetSubsystem(subsystemName); 

 

 

                            var res = subsystem.AcceptNewState(newStateItem.Value); 

                            Logger.LogVerbose(Message.DebugVerbose, 

                                string.Format("Результат принятия нового состояния подсистемой {0}: {1}", subsystemName, res)); 

 

 

                            switch (res) 

                            { 

                                case SubsystemStateAcceptanceResult.Accepted: 

                                    curStateItem.Value = newStateItem.Value; 

                                    curStateItem.Synchronized = true; 

                                    needSave = true; 

                                    break; 

 

 

                                case SubsystemStateAcceptanceResult.AcceptedByMerge: 

                                    curStateItem.Value = subsystem.GetState(); 

                                    needSave = true; 

                                    break; 

 

 


                                case SubsystemStateAcceptanceResult.Rejected: 

                                    curStateItem.Synchronized = false; 

                                    break; 

 

 

                                default: 

                                    throw new Exception("Неизвестный результат"); 

                            } 

                        } 

                    } 

 

 

                    if (needSave) 

                        SaveState(); 

 

 

                    TransferStateToRemoteScanner(true); 

                } 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.SyncSynchronizationException, ex); 

                StateSynchronizationFinished(SynchronizationResult.Failed); 

            } 

            finally 

            { 

                _stateSynchronizingNow = false; 

                Logger.LogVerbose(Message.DebugVerbose, "Запрос на синхронизацию обработан"); 

            } 

        } 

 

 

        /// <summary> 

        /// Синхронизация состояния завершена 

        /// </summary> 

        /// <param name="succeeded">успешно ли выполнена синхронизация</param> 

        /// <remarks>этот метод вызывает удаленный сканер с целью сообщить, 

        /// что синхронизация состояния завершена</remarks> 

        public void StateSynchronizationFinished(SynchronizationResult syncResult) 

        { 

            Logger.LogVerbose(Message.DebugVerbose, "Синхронизация завершена: " + syncResult); 

 

 

            Logger.LogVerbose(Message.LockTryEnter, STATE_SYNC); 

            lock (s_stateSync) 

            { 

                Logger.LogVerbose(Message.LockDone, STATE_SYNC); 

 

 

                var synchronized = (syncResult == SynchronizationResult.Succeeded); 


                foreach (var item in _stateDict) 

                    item.Value.Synchronized = synchronized; 

            } 

 

 

            _lastSynchronizationResult = syncResult; 

            _synchronizationFinished.Set(); 

        } 

 

 

        /// <summary> 

        /// Ожидает, когда завершится очередная синхронизация с удаленным сканером 

        /// </summary> 

        /// <returns>true - синхронизация выполнена успешно или ее не нужно было выполнять, иначе - false</returns> 

        public bool WaitForSynchronizationFinished(IWaitController waitCtrl) 

        { 

            if (!WaitOne(_synchronizationFinished, waitCtrl)) 

                // был вызван деструктор 

                return false; 

 

 

            var res = _lastSynchronizationResult; 

            return (res == SynchronizationResult.SynchronizationNotEnabled || res == SynchronizationResult.Succeeded); 

        } 

 

 

        #endregion 

 

 

        #region Загрузка/Сохранение/Сброс состояния 

 

 

        private bool _stateLoaded = false; 

 

 

        /// <summary> 

        /// Загрузить состояние с диска из файла состояния 

        /// </summary> 

        public void LoadState() 

        { 

            Logger.LogVerbose(Message.LockTryEnter, STATE_SYNC); 

            lock (s_stateSync) 

            { 

                Logger.LogVerbose(Message.LockDone, STATE_SYNC); 

 

 

                _stateLoaded = true; 

 

 

                FileInfo stateFile = new FileInfo(_stateFilePath); 


 
 

                // если файл с состоянием не найден или имеет нулевой размер 

                if (!stateFile.Exists || stateFile.Length <= 0) 

                { 

                    if(stateFile.Exists && stateFile.Length <= 0) 

                    { 

                        // протоколируем факт того, что файл нулевого размера 

                        Logger.LogWarning(Message.SyncStateHasZeroSize); 

                        try 

                        { 

                            stateFile.Delete(); 

                        } 

                        catch (Exception ex) 

                        { 

                            // не смогли удалить, протоколируем 

                            Logger.LogException(Message.SyncStateDeleteError, ex); 

                        } 

                    } 

 

 

                    // TODO: подумать как перенести подъем бакапа состояния в FileSystemManager 

                    // по-хорошему там надо сделать зеркальный для SafeSerialization метод 

                    string backupFilePath = _stateFilePath + ".bkp"; 

                    if (File.Exists(backupFilePath)) 

                    { 

                        // пробуем поднять бакап 

                        File.Copy(backupFilePath, _stateFilePath); 

                        stateFile = new FileInfo(_stateFilePath); 

                        if (!stateFile.Exists) 

                        { 

                            // считаем, что состояние находится в начальном положении 

                            Logger.LogInfo(Message.SyncInitialState); 

                            _isStateInitial = true; 

                            return; 

                        } 

                    } 

                    else 

                    { 

                        // считаем, что состояние находится в начальном положении 

                        Logger.LogInfo(Message.SyncInitialState); 

                        _isStateInitial = true; 

                        return; 

                    } 

                } 

 

 

                Dictionary<string, StateItem> newStateDict = null; 

 

 


                // читаем данные из файла и десериализуем состояние 

                using (var stream = File.Open(_stateFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)) 

                { 

                    newStateDict = (Dictionary<string, StateItem>)_stateSerializationFormatter.Deserialize(stream); 

                } 

 

 

                // установим состояния подсистемам 

                foreach (var subsystemName in newStateDict.Keys) 

                { 

                    if (!_stateDict.ContainsKey(subsystemName)) 

                        continue; 

 

 

                    var stateItem = newStateDict[subsystemName]; 

                    var subsystem = (IStateSubsystem)Application.GetSubsystem(subsystemName); 

 

 

                    // если значение состояния задано 

                    if (stateItem.Value != null) 

                        // то восстановим состояние подсистемы 

                        try 

                        { 

                            subsystem.RestoreState(stateItem.Value); 

                        } 

                        catch (Exception ex) 

                        { 

                            Logger.LogException(Message.SyncSubsystemRestoreStateFailed, ex); 

                            throw ex; 

                        } 

                    // иначе - сбросим 

                    // (значение может быть пустым, если ранее состояние было сброшено) 

                    else 

                        subsystem.ResetState(false); 

 

 

                    stateItem.Synchronized = false; 

                    _stateDict[subsystemName] = stateItem; 

                } 

 

 

                // выставим признак того, что состояние уже не в начальном положении 

                _isStateInitial = false; 

            } 

        } 

 

 

        /// <summary> 

        /// Сохранить состояние конкретной подсистемы 

        /// </summary> 


        /// <param name="subsystemName"></param> 

        /// <param name="subsystemState"></param> 

        private void SaveSubsystemState(string subsystemName, object subsystemState) 

        { 

            Logger.LogVerbose(Message.LockTryEnter, STATE_SYNC); 

            lock (s_stateSync) 

            { 

                Logger.LogVerbose(Message.LockDone, STATE_SYNC); 

 

 

                _stateDict[subsystemName].Value = subsystemState; 

 

 

                // сохраняем состояние в файл состояние, только если 

                if (// состояние уже пытались загрузить ранее  

                    // (если нет, то сохранять нельзя, чтобы не перезаписать файл) 

                    _stateLoaded && 

                    // и синхронизация включена 

                    _stateSynchronizationEnabled) 

                { 

                    SaveState(); 

                } 

            } 

        } 

 

 

        /// <summary> 

        /// Архивирует текущее состояние и сбрасываеи его в начальное 

        /// </summary> 

        void IScannerInteractionChannel.ResetState() 

        { 

            Logger.LogVerbose(Message.LockTryEnter, STATE_SYNC); 

            lock (s_stateSync) 

            { 

                Logger.LogVerbose(Message.LockDone, STATE_SYNC); 

 

 

                // архивируем состояние 

                ArchiveState(); 

 

 

                // сбрасываем состояния подсистем 

                var subsystemNames = new string[_stateDict.Keys.Count]; 

                _stateDict.Keys.CopyTo(subsystemNames, 0); 

 

 

                foreach (var subsystemName in subsystemNames) 

                { 

                    var subsystem = (IStateSubsystem)Application.GetSubsystem(subsystemName); 

                    subsystem.ResetState(false); 


                    _stateDict[subsystemName] = new StateItem(subsystemName); 

                } 

 

 

                // выставляем признак того, что состояние находится в начальном положении 

                _isStateInitial = true; 

 

 

                // сохраним состояние 

                SaveState(); 

            } 

        } 

 

 

        /// <summary> 

        /// Сохраняет текущее состояние в файл 

        /// </summary> 

        private void SaveState() 

        { 

            try 

            { 

                if (_fileSystemManager.SafeSerialization(FileType.State, _stateDict, 

                    _stateSerializationFormatter, _stateFilePath)) 

                { 

                    Logger.LogVerbose(Message.SyncStateSaved); 

                } 

                else 

                { 

                    Logger.LogError(Message.SyncStateSaveFailed); 

                } 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.SyncStateSaveException, ex); 

            } 

        } 

 

 

        /// <summary> 

        /// Архивация текущего состояния 

        /// </summary> 

        private void ArchiveState() 

        { 

            // сохраняем состояние в отдельный файл 

            try 

            { 

				// директория для хранения состояний 

				var stateDirectory = _fileSystemManager.GetDataDirectoryPath(FileType.State); 

 

 


                _fileSystemManager.SafeSerialization(FileType.State, _stateDict, _stateSerializationFormatter,  

                    FileUtils.GetUniqueName(stateDirectory, STATE_FILE_NAME, STATE_FILE_EXT, 3)); 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.SyncStateArchiveException, ex); 

            } 

        } 

 

 

        #endregion 

 

 

        #endregion 

 

 

        #region IScannersInfo Members 

 

 

        /// <summary> 

        /// Серийный номер локального сканера 

        /// </summary> 

        public string LocalScannerSerialNumber 

        { 

            get 

            { 

                return _scannerManager.SerialNumber; 

            } 

        } 

 

 

        /// <summary> 

        /// Получить серийные номера всех сканеров 

        /// </summary> 

        /// <returns></returns> 

		public List<ScannerInfo> GetScannerInfos() 

        { 

			List<ScannerInfo> infos = new List<ScannerInfo>(); 

 

 

			// добавим текущий сканер 

			infos.Add(new ScannerInfo(_scannerManager.SerialNumber, _scannerManager.IPAddress)); 

            // добавим удаленный 

			if(IsRemoteScannerConnected) 

				infos.Add(new ScannerInfo(_remoteScannerInfo.SerialNumber, _remoteScannerInfo.IPAddress)); 

 

 

			//просмотрим результаты голосования, возможно был еще какой-либо сканер 

			foreach (var scanner in _electionManager.VotingResults.GetScannerInfos()) 

			{ 


				if (!infos.Contains(scanner)) 

					infos.Add(scanner); 

			} 

 

 

			return infos; 

        } 

 

 

        #endregion 

 

 

        #region IDisposable Members 

 

 

        public override void Dispose() 

        { 

            base.Dispose(); 

            CloseIncomingInteractionChannel(); 

        } 

 

 

        #endregion 

    } 

}



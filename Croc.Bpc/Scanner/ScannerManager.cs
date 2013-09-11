using System; 

using System.Collections.Generic; 

using System.Threading; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Core; 

using Croc.Bpc.Common.Images; 

using Croc.Core.Extensions; 

using Croc.Core.Configuration; 

using Croc.Bpc.Common; 

using Croc.Bpc.Election; 

using Croc.Bpc.Election.Voting; 

using Croc.Bpc.Scanner.Config; 

using Croc.Bpc.Recognizer; 

using Croc.Core.Utils.Text; 

 

 

namespace Croc.Bpc.Scanner 

{ 

    /// <summary> 

    /// Менеджер сканера 

    /// </summary> 

    [SubsystemConfigurationElementTypeAttribute(typeof(ScannerManagerConfig))] 

    public class ScannerManager :  

        Subsystem, 

        IScannerConnectorEventHandler, 

        IScannerEventHandler, 

        IScannerManager 

    { 

        /// <summary> 

        /// Менеджер выборов 

        /// </summary> 

        private IElectionManager _electionManager; 

        /// <summary> 

        /// Менеджер распознавания 

        /// </summary> 

        private IRecognitionManager _recognitionManager; 

        /// <summary> 

        /// Сканер 

        /// </summary> 

        private IScanner _scanner; 

        /// <summary> 

        /// Конфиг менеджера сканера 

        /// </summary> 

        private ScannerManagerConfig _config; 

        /// <summary> 

        /// Конфиг со значениями параметров сканера 

        /// </summary> 

        private ScannerParametersConfig _scannerConfig; 

        /// <summary> 

        /// Статус сканера 


        /// </summary> 

        private int _scannerStatus; 

 

 

 

 

        /// <summary> 

        /// Инициализация подсистемы 

        /// </summary> 

        /// <param name="config"></param> 

        public override void Init(SubsystemConfig config) 

        { 

            // получим ссылки другие подсистемы 

            _electionManager = Application.GetSubsystemOrThrow<IElectionManager>(); 

            _recognitionManager = Application.GetSubsystemOrThrow<IRecognitionManager>(); 

 

 

            // применим конфиг 

            ApplyNewConfig(config); 

        } 

 

 

        /// <summary> 

        /// Применение нового конфига 

        /// </summary> 

        /// <param name="newConfig"></param> 

        public override void ApplyNewConfig(SubsystemConfig newConfig) 

        { 

            lock (s_loadParametersSync) 

            { 

                _config = (ScannerManagerConfig)newConfig; 

 

 

                if (_config.ScannerParametersVersions.Count == 0) 

                    throw new Exception("Не заданы параметры сканера"); 

 

 

                // получим конфиг, который соответствует версии сканера 

                if (_scanner != null) 

                { 

                    var versionName = _scanner.Version.ToString(); 

                    _scannerConfig = _config.ScannerParametersVersions[versionName]; 

                    if (_scannerConfig == null) 

                        throw new Exception(string.Format("Не найдены настройки для сканера версии '{0}'", versionName)); 

                } 

                else 

                { 

                    // если сканер еще не подключен, то просто возьмем первый конфиг 

                    _scannerConfig = _config.ScannerParametersVersions[0]; 

                } 


 
 

                // сбросим признак загруженности параметров сканера 

                _parametersLoaded = false; 

            } 

        } 

 

 

        #region IScannerConnectorEventHandler Members 

 

 

        /// <summary> 

        /// Событие о завершении подключении сканера 

        /// </summary> 

        /// <param name="serialNumber">Серийный номер сканера</param> 

        /// <param name="ipAddress">IP адрес сканера</param> 

        /// <param name="status">статус сканера</param> 

        public void Connected(string serialNumber, string ipAddress, int status) 

        { 

            // если это не локальный IP 

            if (!NetHelper.IsLocalIPAddress(ipAddress)) 

            { 

                // сообщим, что подключился удаленный сканер 

                RemoteScannerConnected.RaiseEvent(this, new ScannerEventArgs(serialNumber, ipAddress)); 

                return; 

            } 

 

 

            // если уже подключен, то выходим 

            if (_scannerConnected) 

                return; 

        } 

 

 

        /// <summary> 

        /// Событие о подключении сканера 

        /// </summary> 

        /// <param name="serialNumber">Серийный номер сканера</param> 

        /// <param name="ipAddress">IP адрес сканера</param> 

        /// <param name="scannerStatus">Состояние сканера</param> 

        public void WantToConnect(string serialNumber, string ipAddress, int scannerStatus) 

        { 

            // если это не локальный IP 

            if (!NetHelper.IsLocalIPAddress(ipAddress)) 

                // ничего не делаем 

                return; 

 

 

            // запомним статус сканера 

            _scannerStatus = scannerStatus; 


 
 

            // подключаемся к сканеру 

            try 

            { 

                // если уже подключились, то выходим 

                if (_scannerConnected) 

                    return; 

 

 

                // получаем интерфейс сканера 

                _scanner = _scannerConnector.GetConnectedScanner(serialNumber); 

 

 

                // запомним серийный номер сканера 

                SerialNumber = _scanner.SerialNumber; 

                IntSerialNumber = int.Parse(SerialNumber); 

 

 

                // устанавливаем себя, как обработчик событий сканера 

                _scanner.SetEventsHandler(this); 

 

 

                // установить признак подключенности сканера 

                _scannerConnected = true; 

 

 

                // сигнализируем, что соединение установлено 

                _connectionDone.Set(); 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.ScannerManagerException, ex, "Ошибка подключения к сканеру"); 

            } 

        } 

 

 

        /// <summary> 

        /// Событие о разрыве соединения со сканером 

        /// </summary> 

        /// <param name="serialNumber">Серийный номер сканера</param> 

        public void Disconnected(string serialNumber) 

        { 

            // запоминаем, что подключение не установлено 

            _scannerConnected = false; 

        } 

 

 

        #endregion         

 


 
        #region Загрузка параметров в сканер 

 

 

        /// <summary> 

        /// Объект синхронизации загрузки параметров в сканер 

        /// </summary> 

        private static object s_loadParametersSync = new object(); 

        //TODO: флаги загрузки параметров нужно сбрасывать при изменении конфига извне 

        /// <summary> 

        /// Признак того, что параметры сканера загружены 

        /// </summary> 

        private bool _parametersLoaded = false; 

 

 

 

 

		/// <summary> 

		/// Перезагрузка параметров сканера 

		/// </summary> 

		public void ReLoadParameters() 

		{ 

			// выставим признак незагруженности параметров 

			_parametersLoaded = false; 

 

 

			// загрузим параметры 

			LoadParameters(); 

		} 

 

 

        /// <summary> 

        /// Загрузить параметры в сканер 

        /// </summary> 

        private void LoadParameters() 

        { 

            lock (s_loadParametersSync) 

            { 

                Logger.LogVerbose(Message.DebugVerbose, "call"); 

 

 

                // если параметры уже загружены 

                if (_parametersLoaded) 

                    // то выходим 

                    return; 

 

 

                // Выключаю сканирование 

                bool oldScanningEnabled = _scanner.ScanningEnabled; 

                _scanner.ScanningEnabled = false; 


 
 

                // устанаваливаем параметры 

                _scanner.SetWorkZone(ScannedSide.Top, _scannerConfig.WorkZone.SideTopX, _scannerConfig.WorkZone.SideTopY); 

                _scanner.SetWorkZone(ScannedSide.Bottom, _scannerConfig.WorkZone.SideBottomX, _scannerConfig.WorkZone.SideBottomY); 

 

 

                // загрузим параметры бланков 

                LoadBlankParameters(); 

 

 

                // загрузим параметры ДДЛ 

                LoadDoubleSheetSensorParameters(); 

 

 

                // задаем включенность режима настройки на яркость листа 

                _scanner.TuningEnabled = _scannerConfig.Tuning.Enabled; 

 

 

                // восстанавливаем режим работы сканера 

                _scanner.ScanningEnabled = oldScanningEnabled; 

 

 

                // запоминаем, что параметры загружены 

                _parametersLoaded = true; 

            } 

        } 

 

 

        /// <summary> 

        /// Загрузить параметры бланков в сканер 

        /// </summary> 

        private void LoadBlankParameters() 

        { 

            Logger.LogVerbose(Message.DebugVerbose, "call"); 

 

 

            // Минимальная длина листа в строках 

            int minSheetLength = 0; 

            // Максимальная длина листа в строках 

            int maxSheetLength = 0; 

 

 

            // очищу старые данные 

            _scanner.ValidLength_ClearAll(); 

            _scanner.PageOffset_ClearAll(); 

 

 

            // если ИД загружены 

            if (null != _electionManager.SourceData) 


            { 

                var blankSizes = new List<BlankSize>(); 

                var blankOffsets = new List<BlankOffset>(); 

 

 

                // по всем бланкам 

                foreach (var blank in _electionManager.SourceData.Blanks) 

                { 

                    // Высота бланка в строках 

                    int blankHeight = (int)(blank.Height * _scanner.DpiYTop / 25.4); 

                    int delta = (int)(blank.Delta * _scanner.DpiYTop / 25.4); 

 

 

                    // Корректирую при необходимости минимальную и максимальную длины листа 

                    if (minSheetLength == 0 || blankHeight - delta < minSheetLength) 

                        minSheetLength = blankHeight - delta; 

 

 

                    if (maxSheetLength == 0 || blankHeight + delta > maxSheetLength) 

                        maxSheetLength = blankHeight + delta; 

 

 

                    // если задана портретная ориентация 

                    if (blank.Orientation == BlankOrientation.Portrait || 

                        blank.Orientation == BlankOrientation.PortraitAndLandscape) 

                    { 

                        // размер бланка 

                        var blankSize = new BlankSize(blank.Width, blank.Height, blank.Delta); 

 

 

                        // если еще нет такого размера, то добавлю 

                        if (!blankSizes.Contains(blankSize)) 

                            blankSizes.Add(blankSize); 

 

 

                        // если задано смещение, то 

                        if (blank.MaxPortraitShift > 0) 

                        { 

                            var blankOffset = new BlankOffset(blank.Width, blank.MaxPortraitShift); 

 

 

                            // если еще нет такого смещения, то добавлю 

                            if (!blankOffsets.Contains(blankOffset)) 

                                blankOffsets.Add(blankOffset); 

                        } 

                    } 

 

 

                    // если задана альбомная ориентация 

                    if (blank.Orientation == BlankOrientation.Landscape || 


                        blank.Orientation == BlankOrientation.PortraitAndLandscape) 

                    { 

                        // размер бланка 

                        var blankSize = new BlankSize(blank.Height, blank.Width, blank.Delta); 

 

 

                        // если еще нет такого размера, то добавлю 

                        if (!blankSizes.Contains(blankSize)) 

                            blankSizes.Add(blankSize); 

 

 

                        // если задано смещение, то 

                        if (blank.MaxPortraitShift > 0) 

                        { 

                            var blankOffset = new BlankOffset(blank.Height, blank.MaxLandscapeShift); 

 

 

                            // если еще нет такого смещения, то добавлю 

                            if (!blankOffsets.Contains(blankOffset)) 

                                blankOffsets.Add(blankOffset); 

                        } 

                    } 

                } 

 

 

                if (_scannerConfig.CheckFormat.Enabled) 

                { 

                    foreach (var blankSize in blankSizes) 

                    { 

                        _scanner.ValidLength_AddItem( 

                            blankSize.Width, 

                            (blankSize.Height - blankSize.Delta), 

                            (blankSize.Height + blankSize.Delta)); 

                    } 

                    foreach (var blankOffset in blankOffsets) 

                    { 

                        _scanner.PageOffset_AddItem(blankOffset.Width, blankOffset.MaxShift); 

                    } 

                } 

            } 

 

 

            // Устанавливаю вычисленные минимальную и максимальную длины листа 

            _scanner.MinSheetLength = (short)minSheetLength; 

            _scanner.MaxSheetLength = (short)maxSheetLength; 

 

 

            // задаем режим распознавания формата бланков 

            _scanner.LengthValidationEnabled = _scannerConfig.CheckFormat.Enabled; 

        } 


 
 

        /// <summary> 

        /// Загрузить параметры датчика двойного листа в сканер 

        /// </summary> 

        private void LoadDoubleSheetSensorParameters() 

        { 

            Logger.LogVerbose(Message.DebugVerbose, "call"); 

 

 

            // определение наличия грязи на линейках 

            _scanner.DirtDetectionEnabled = _scannerConfig.DirtDetection.Enabled; 

 

 

            // если датчик не работает 

            if (!IsDoubleSheetSensorWork) 

                return; 

 

 

            _scanner.DoubleSheetSensorEnabled = _scannerConfig.DoubleSheetSensor.Enabled; 

 

 

            // если ДДЛ выключен 

            if (!_scannerConfig.DoubleSheetSensor.Enabled) 

                // то больше ничего не делаем 

                return; 

 

 

            // получим текущие пороги левого и правого датчиков 

            short levelLeft; 

            short levelRight; 

            _scanner.GetDoubleSheetSensorLevel(out levelLeft, out levelRight); 

 

 

            // если в конфиге заданы параметры отличные от нуля, то используем их вместо текущих порогов 

            if (_scannerConfig.DoubleSheetSensor.LevelLeft != 0) 

                levelLeft = _scannerConfig.DoubleSheetSensor.LevelLeft; 

 

 

            if (_scannerConfig.DoubleSheetSensor.LevelRigth != 0) 

                levelRight = _scannerConfig.DoubleSheetSensor.LevelRigth; 

 

 

            // установим новые пороги 

            _scanner.SetDoubleSheetSensorLevel(levelLeft, levelRight); 

        } 

 

 

        #endregion 

 


 
        #region IScannerEventHandler Members 

 

 

        /// <summary> 

        /// Получен заказанный доп. буфер с идентификатором nID 

        /// </summary> 

        /// <param name="scanner">Сканер</param> 

        /// <param name="bufferId">Идентификатор запрашиваемого буфера</param> 

        public void BufferIsReady(IScanner scanner, short bufferId) 

        { 

            Logger.LogVerbose(Message.ScannerManagerBufferIsReady, bufferId); 

        } 

 

 

        /// <summary> 

        /// Получено отладочное сообщение от сканера 

        /// </summary> 

        /// <param name="scanner">Сканер</param> 

        /// <param name="message">Сообщение</param> 

        /// <param name="messageLength">длина строки</param> 

        public void DebugMessage(IScanner scanner, string message, int messageLength) 

        { 

            Logger.LogVerbose(Message.ScannerManagerDebugMessage, message, messageLength); 

        } 

 

 

        /// <summary> 

        /// Получена ошибка от сканера 

        /// </summary> 

        /// <param name="scanner">Сканер, от которого пришло событие</param> 

        /// <param name="error">ошибка</param> 

        public void Error(IScanner scanner, ScannerError error) 

        { 

			// возобновим логирование 

			CoreApplication.Instance.LoggerEnabled.Set(); 

 

 

			Logger.LogError(Message.ScannerManagerError, scanner.SerialNumber, error); 

 

 

            try 

            { 

                // сигнализируем об ошибке 

                AlertAboutError((int)error, true); 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.ScannerManagerException, ex, "Ошибка при обработке ошибки от сканера"); 

            } 


        } 

 

 

        /// <summary> 

        /// Указанный сканер - это наш сканер? 

        /// </summary> 

        /// <param name="scanner"></param> 

        /// <returns></returns> 

        private bool IsOurScanner(IScanner scanner) 

        { 

            return scanner != null && _scanner.SerialNumber.Equals(scanner.SerialNumber); 

        } 

 

 

        /// <summary> 

        /// Поступил новый лист 

        /// </summary> 

        /// <param name="scanner">Объект сканера</param> 

        public void NewSheet(IScanner scanner) 

        { 

            // приостановим протоколирование 

            CoreApplication.Instance.LoggerEnabled.Reset(); 

            // останавливаем прокрутку 

            GetRollTextMachine().Stop(); 

 

 

            Logger.LogVerbose(Message.DebugVerbose, "call"); 

 

 

            try 

            { 

                // если это не наш сканер 

                if (!IsOurScanner(scanner)) 

                    // то ничего не делаем 

                    return; 

 

 

                // протоколируем 

                Logger.LogInfo(Message.ScannerManagerNewSheet); 

 

 

                // Установим режим работы ламп = сканирование 

                SetLampsRegime(ScannerLampsRegime.Scanning); 

 

 

                // открываем новую сессию обработки листа 

                OpenNewSheetProcessingSession(); 

 

 

                // сбрасываем команду на реверс 


                ResetReverseCommand(); 

 

 

                // запускаем распознавание 

                var lineWidth = _scanner.WorkZoneW / 8; 

                _recognitionManager.RunRecognition(lineWidth); 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.ScannerManagerException, ex, "Ошибка при обработке поступления нового листа"); 

            } 

            finally 

            { 

                Logger.LogVerbose(Message.DebugVerbose, "return"); 

            } 

        } 

 

 

        /// <summary> 

        /// Получен следующий буфер 

        /// </summary> 

        /// <param name="scanner">Объект сканера</param> 

        /// <param name="str0">Общее количество накопленных строк с лицевой стороны</param> 

        /// <param name="str1">Общее количество накопленных строк с обратной стороны</param> 

        public void NextBuffer(IScanner scanner, short str0, short str1) 

        { 

            Logger.LogVerbose(Message.ScannerManagerNextBufferCall, str0, str1); 

 

 

            try 

            { 

                // если это не наш сканер 

                if (!IsOurScanner(scanner)) 

                    // то ничего не делаем 

                    return; 

 

 

                // запоминаем, сколько было отсканировано строк (берем макс. значение) 

                ScannedLinesCountLast = Math.Max(str0, str1); 

 

 

                // распознаем буфер 

                int blankMarker; 

 

 

                // если удалось распознать бюллетень в режиме online и ДДЛ работает 

                if (_recognitionManager.ProcessNextBuffer(str0, str1, out blankMarker) && 

                    IsDoubleSheetSensorWork) 

                { 

                    // то установим плотность бумаги для ДДЛ 


 
 

                    // получим плотность бумаги бланка 

                    var density = GetBlankPaperDensity(blankMarker); 

 

 

                    // получим текущие пороги левого и правого датчиков двойного листа 

                    short leftLevel = 0; 

                    short rightLevel = 0; 

                    scanner.GetDoubleSheetSensorLevel(out leftLevel, out rightLevel); 

 

 

                    // изменим пороги 

                    leftLevel += density; 

                    rightLevel += density; 

 

 

                    // зададим новые пороги 

                    scanner.SetDoubleSheetSensorCurrentSheetLevel(leftLevel, rightLevel); 

 

 

                    Logger.LogVerbose(Message.ScannerManagerDensitySet, blankMarker, density); 

                } 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.ScannerManagerException, ex, "Ошибка при обработке следующего буфера листа"); 

            } 

            finally 

            { 

                Logger.LogVerbose(Message.ScannerManagerNextBufferCall, str0, str1); 

            } 

        } 

 

 

        /// <summary> 

        /// Возвращает плотность бумаги бланка 

        /// </summary> 

        /// <param name="blankMarker">маркер боанка</param> 

        /// <returns></returns> 

        private short GetBlankPaperDensity(int blankMarker) 

        { 

            // получим тип бумаги бланка 

            var paperType = _scannerConfig.BlankPaperTypes.GetPaperTypeByMarker(blankMarker); 

 

 

            switch (paperType) 

            { 

                case PaperType.Thin:    return _scannerConfig.DoubleSheetSensor.Thin; 

                case PaperType.Thick:   return _scannerConfig.DoubleSheetSensor.Thick; 


                default:                return 0; 

            } 

        } 

 

 

        /// <summary> 

        /// Лист сброшен 

        /// </summary> 

        /// <param name="scanner">Объект сканера</param> 

        /// <param name="result">Результат выполнения сброса листа</param> 

        public void SheetDroped(IScanner scanner, DropResult result) 

        { 

            Logger.LogVerbose(Message.DebugVerbose, string.Format("call: result = {0}", result)); 

 

 

            try 

            { 

                // если это не наш сканер 

                if (!IsOurScanner(scanner)) 

                    // то ничего не делаем 

                    return; 

 

 

                // протоколируем 

                Logger.LogInfo(Message.ScannerManagerSheetDroped, result); 

 

 

                // восстановим предыдущий режим работы ламп 

                RestorePreviousLampsRegime(); 

 

 

                // установим результат зброса сессии 

                _sheetProcessingSession.DropResult = result; 

                // закрываем сессию обработки листа с соотв. результатом распознавания 

                CloseSheetProcessingSession(_electionManager.LastVotingResult, null); 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.ScannerManagerException, ex, "Ошибка при обработке события сброса листа"); 

            } 

            finally 

            { 

                Logger.LogVerbose(Message.DebugVerbose, string.Format("return: result = {0}", result)); 

            } 

        } 

 

 

        /// <summary> 

        /// Лист отсканирован 

        /// </summary> 


        /// <param name="scanner">Объект сканера</param> 

        /// <param name="str0">Количество строк с лицевой стороны</param> 

        /// <param name="str1">Количество строк с обратной стороны</param> 

        public void SheetIsReady(IScanner scanner, short str0, short str1) 

        { 

            // возобновим протоколирование 

            CoreApplication.Instance.LoggerEnabled.Set(); 

 

 

            Logger.LogVerbose(Message.ScannerManagerSheetIsReadyCall, str0, str1); 

 

 

            try 

            { 

                // если это не наш сканер 

                if (!IsOurScanner(scanner)) 

                    // то ничего не делаем 

                    return; 

 

 

                // протоколируем 

                Logger.LogInfo(Message.ScannerManagerSheetIsReady, str0, str1); 

 

 

                // TODO: зачем? так было сделано в сибапп в 2006, причины не ясны 

                //Thread.Sleep(500); 

 

 

                // сбросим счетчики ошибок 

                _config.Alerts.ResetErrorCounters(); 

 

 

                // Проверка на превышение размера буфера 

                if (str0 > _scanner.WorkZoneH || str1 > _scanner.WorkZoneH) 

                    Logger.LogError(Message.ScannerManagerSheetIsReadyTooLarge, _scanner.WorkZoneH); 

 

 

                // Проверка на разное количество строк на разных сторонах 

                if (str0 != str1) 

                    Logger.LogWarning(Message.ScannerManagerSheetIsReadyDontMatch, str0, str1); 

 

 

                // запоминаем, сколько было отсканировано строк (берем макс. значение) 

                ScannedLinesCountLast = Math.Max(str0, str1); 

 

 

                // Запускаем распознавание в отдельном высокоприоритетном потоке 

                var recognizeThread = new Thread(() => { _recognitionManager.EndRecognition(); }) 

                { 

                    Name = "Recognize", 


                    IsBackground = true, 

                    Priority = ThreadPriority.Highest 

                }; 

                recognizeThread.Start(); 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.ScannerManagerException, ex, "Ошибка при обработке события готовности листа"); 

            } 

            finally 

            { 

                Logger.LogVerbose(Message.ScannerManagerSheetIsReadyReturn, str0, str1); 

            } 

        } 

 

 

        /// <summary> 

        /// Готов к приему листа 

        /// </summary> 

        /// <param name="scanner">Объект сканера</param> 

        public void ReadyToScanning(IScanner scanner) 

        { 

            Logger.LogVerbose(Message.DebugVerbose, "call"); 

 

 

            try 

            { 

                // если это не наш сканер 

                if (!IsOurScanner(scanner)) 

                    // то ничего не делаем 

                    return; 

 

 

                // возобновляем прокрутку 

                GetRollTextMachine().Start(); 

 

 

                // протоколируем 

                Logger.LogInfo(Message.ScannerManagerReadyToScanning); 

 

 

                // обновим параметры сканера 

                LoadParameters(); 

 

 

                // контрольный сброс состояния распознавалки на случай реверса листа 

                _recognitionManager.ResetRecognition(); 

            } 

            catch (Exception ex) 

            { 


                Logger.LogException(Message.ScannerManagerException, ex, "Ошибка при обработке события готовности к приему листа"); 

            } 

            finally 

            { 

                Logger.LogVerbose(Message.DebugVerbose, "return"); 

            } 

        } 

 

 

        #endregion 

 

 

        #region IScannerManager Members 

 

 

        #region Подключение к сканеру и диагностика 

 

 

        /// <summary> 

        /// Подключатель сканера 

        /// </summary> 

        private ScannerConnector _scannerConnector; 

        /// <summary> 

        /// Признак того, что сканер подключен 

        /// </summary> 

        private volatile bool _scannerConnected; 

        /// <summary> 

        /// Объект для синхронизации подключения сканера 

        /// </summary> 

        private static object s_connectionSync = new object(); 

        /// <summary> 

        /// Событие "Соединение со сканером установлено" 

        /// </summary> 

        private AutoResetEvent _connectionDone = new AutoResetEvent(false); 

 

 

        /// <summary> 

        /// Подключен ли сканер 

        /// </summary> 

        public bool ScannerConnected 

        { 

            get 

            { 

                return _scannerConnected; 

            } 

        } 

 

 

        /// <summary> 

        /// Установить соединение со сканером 


        /// </summary> 

        /// <param name="maxTryCount">максимальное кол-во попыток установить подключение</param> 

        /// <param name="delay">задержка между попытками</param> 

        /// <returns> 

        /// true - соединение установлено, false - не удалось установить соединение 

        /// </returns> 

        public bool EstablishConnectionToScanner(int maxTryCount, TimeSpan delay) 

        { 

            CodeContract.Requires(maxTryCount > 0); 

 

 

            // получим тип класса коннектора по имени типа 

            Type connectorClassType; 

            try 

            { 

                connectorClassType = Type.GetType(_config.ScannerConnector.TypeName, true); 

            } 

            catch (Exception ex) 

            { 

                throw new Exception(string.Format( 

                    "Ошибка получения типа класса коннектора сканера по имени типа '{0}'", 

                    _config.ScannerConnector.TypeName), ex); 

            } 

 

 

            if (!connectorClassType.CanCastToType(typeof(ScannerConnector))) 

                throw new Exception(string.Format( 

                    "Тип {0} должен быть унаследован от ScannerConnector", connectorClassType.FullName)); 

 

 

            // настройки коннектора 

            var scannerConnectorSettings = _config.ScannerConnector.Settings.ToNameValueCollection(); 

 

 

            int tryCount = 0; 

            while (true) 

            { 

                try 

                { 

                    // удалим старый коннектор 

                    if (_scannerConnector != null) 

                        _scannerConnector.Dispose(); 

 

 

                    // сбросим признак подключенности 

                    _scannerConnected = false; 

 

 

                    // создадим коннектор 

                    _scannerConnector = (ScannerConnector)Activator.CreateInstance(connectorClassType); 


 
 

                    // проинициализируем 

                    _scannerConnector.Init(_config.ScannerConnector.BroadcastIPAddress, scannerConnectorSettings); 

 

 

                    // начнем подключаться 

                    _connectionDone.Reset(); 

                    _scannerConnector.Connect(this); 

 

 

                    // ждем, когда соединение будет установлено 

                    if (!_connectionDone.WaitOne(TimeSpan.FromSeconds(10), false)) 

                        throw new Exception("Не дождались установки соединения со сканером"); 

 

 

                    Logger.LogInfo(Message.ScannerManagerConnected, _scanner.SerialNumber, _scanner.IPAddress); 

 

 

                    // запустим поток управления лампами 

                    StartManageLampsThread(); 

 

 

                    return true; 

                } 

                catch (Exception ex) 

                { 

                    Logger.LogException(Message.ScannerManagerCantConnect, ex, ++tryCount); 

 

 

                    if (tryCount >= maxTryCount) 

                        return false; 

 

 

                    // подождем и потом попробуем еще раз 

                    Thread.Sleep(delay); 

                } 

            } 

        } 

 

 

        /// <summary> 

        /// Событие "Удаленный сканер подключился" 

        /// </summary> 

        public event EventHandler<ScannerEventArgs> RemoteScannerConnected; 

 

 

        /// <summary> 

        /// Выполнить диагностику сканера 

        /// </summary> 


        /// <returns>список ошибок, обнаруженных в результате диагностики</returns> 

        public List<ScannerDiagnosticsError> PerformDiagnostics() 

        { 

            if (_scanner == null) 

                throw new InvalidOperationException("Сканер не подключен"); 

 

 

            var errorList = new List<ScannerDiagnosticsError>(); 

 

 

            // проверим, работает звук. канал, на котором "висят" датчики 

            if (((int)ScannerStatus.BAD_TUNE & _scannerStatus) != 0) 

                errorList.Add(ScannerDiagnosticsError.DoubleSheetSensorNotWork); 

 

 

            // проверим левый и правый ДДЛ 

            bool leftWork, rightWork; 

            _scanner.CheckDoubleSheetSensor(out leftWork, out rightWork); 

            if (!leftWork) 

                errorList.Add(ScannerDiagnosticsError.LeftDoubleSheetSensorNotWork); 

            if (!rightWork) 

                errorList.Add(ScannerDiagnosticsError.RightDoubleSheetSensorNotWork); 

 

 

            // проверим версию драйвера 

            if (_scanner.DriverVersion != _config.DriverVersion.Value) 

                errorList.Add(ScannerDiagnosticsError.WrongDriverVersion); 

 

 

            // проверим конфиг драйвера 

            if (((int)ScannerStatus.BAD_CONF & _scannerStatus) != 0) 

                errorList.Add(ScannerDiagnosticsError.WrongDriverConfig); 

 

 

            // проверим файл коэффициентов яркости 

            if (((int)ScannerStatus.BAD_LIGHT & _scannerStatus) != 0) 

                errorList.Add(ScannerDiagnosticsError.WrongBrightnessCoefFile); 

 

 

            // если ошибок нет и в настройках указано, что нужно выполнять диагностику железа 

            if (errorList.Count == 0 && _config.CheckHardware.Enabled) 

                CheckHardware(); 

 

 

            return errorList; 

        } 

 

 

        /// <summary> 

        /// Задержка между процедурами диагностики 


        /// </summary> 

#if DEBUG 

        private const int DIAGNOSTICS_DELAY = 100; 

#else 

        private const int DIAGNOSTICS_DELAY = 500; 

#endif 

 

 

        /// <summary> 

        /// Проверка работоспособности оборудования 

        /// </summary> 

        private void CheckHardware() 

        { 

            try 

            { 

                // проверяем работу индикатора 

                for (int i = 3; i > 0; --i) 

                { 

                    SetIndicator(i.ToString()); 

                    Thread.Sleep(DIAGNOSTICS_DELAY); 

                } 

 

 

                // проверяем лампы 

                _scanner.EnableLamps(true); 

 

 

                // проверка моторов (1 - первый, 2 - второй) 

                for (short j = 1; j <= _scanner.MotorCount; j++) 

                { 

                    CheckMotor(j); 

                } 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.ScannerManagerException, ex, "Ошибка диагностики оборудования"); 

            } 

            finally 

            { 

                // выключаем лампы 

                _scanner.EnableLamps(false); 

                // очищаем индикатор 

                _scanner.SetIndicator(""); 

            } 

        } 

 

 

        /// <summary> 

        /// Проверка работы мотора 

        /// </summary> 


        /// <param name="motorNumber">1 - первый мотор, 2 - второй мотор, 3 - оба мотора одновременно</param> 

        /// <returns>true - работает, false - не работает</returns> 

        private bool CheckMotor(short motorNumber) 

        { 

            bool motorOK = true; 

 

 

            // по всем направлениям вращения (0 и 1) 

            for (int direction = 0; direction < 2; direction++) 

            { 

                // пробуем запустить мотор 

                try 

                { 

                    _scanner.Motor(motorNumber, true, direction, 1); 

                    // путь поработает 

                    Thread.Sleep(DIAGNOSTICS_DELAY); 

                } 

                catch (Exception ex) 

                { 

                    Logger.LogException(Message.ScannerManagerMotorException, ex, "запуске", motorNumber, direction, 1); 

                    motorOK = false; 

                } 

 

 

                // останавливаем мотор 

                try 

                { 

                    _scanner.Motor(motorNumber, false, direction, 1); 

                } 

                catch (Exception ex) 

                { 

                    Logger.LogException(Message.ScannerManagerMotorException, ex, "остановке", motorNumber, direction, 1); 

                    motorOK = false; 

                } 

            } 

 

 

            return motorOK; 

        } 

 

 

        /// <summary> 

        /// Работает ли датчик двойного листа 

        /// </summary> 

        public bool IsDoubleSheetSensorWork 

        { 

            get 

            { 

                return 

                    (0 == ((int)ScannerStatus.BAD_TUNE & _scannerStatus)) && 


                    (0 == ((int)ScannerStatus.BAD_LEFT_DOUBLE_LIST & _scannerStatus)) && 

                    (0 == ((int)ScannerStatus.BAD_RIGHT_DOUBLE_LIST & _scannerStatus)); 

            } 

        } 

 

 

        #endregion 

 

 

        #region Основные атрибуты 

 

 

        /// <summary> 

        /// Серийный номер сканера 

        /// </summary> 

        public string SerialNumber 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Целочисленный серийный номер сканера 

        /// </summary> 

        public int IntSerialNumber 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Версия драйвера сканера 

        /// </summary> 

        public int DriverVersion 

        { 

            get 

            { 

                return _scanner.DriverVersion; 

            } 

        } 

 

 

        /// <summary> 

        /// Версия сканера 

        /// </summary> 

        public ScannerVersion Version 

        { 

            get 


            { 

                return _scanner.Version; 

            } 

        } 

 

 

        /// <summary> 

        /// IP адрес сканера 

        /// </summary> 

        public string IPAddress 

        { 

            get 

            { 

                return _scanner.IPAddress; 

            } 

        } 

 

 

        #endregion 

 

 

        #region Параметры сканера 

 

 

        /// <summary> 

        /// Разрешение сканера по оси X на стороне 0 

        /// </summary> 

        public short DpiXTop 

        { 

            get 

            { 

                return _scanner.DpiXTop; 

            } 

        } 

 

 

        /// <summary> 

        /// Разрешение сканера по оси Y на стороне 0 

        /// </summary> 

        public short DpiYTop 

        { 

            get 

            { 

                return _scanner.DpiYTop; 

            } 

        } 

 

 

        /// <summary> 

        /// Разрешение сканера по оси X на стороне 1 


        /// </summary> 

        public short DpiXBottom 

        { 

            get 

            { 

                return _scanner.DpiXBottom; 

            } 

        } 

 

 

        /// <summary> 

        /// Разрешение сканера по оси Y на стороне 1 

        /// </summary> 

        public short DpiYBottom 

        { 

            get 

            { 

                return _scanner.DpiYBottom; 

            } 

        } 

 

 

        /// <summary> 

        /// Порог бинаризации стороны 0 (с учетом коррекции) 

        /// </summary> 

        public short BinarizationThresholdTop 

        { 

            get 

            { 

                return _scanner.CurrentBinaryThresholdTop; 

            } 

        } 

 

 

        /// <summary> 

        /// Порог бинаризации стороны 1 (с учетом коррекции) 

        /// </summary> 

        public short BinarizationThresholdBottom 

        { 

            get 

            { 

                return _scanner.CurrentBinaryThresholdBottom; 

            } 

        } 

 

 

		/// <summary> 

		/// Включен ли датчик двойного листа 

		/// </summary> 

		public bool DoubleSheetSensorEnabled  


		{  

			get 

			{ 

				return _scannerConfig.DoubleSheetSensor.Enabled; 

			} 

			set 

			{ 

				// сохраним старое значение 

				var oldValue = _scannerConfig.DoubleSheetSensor.Enabled; 

 

 

				_scannerConfig.DoubleSheetSensor.Enabled = value; 

 

 

				// вызовем событие изменения конфигурации 

				var configUpdatedArgs = new ConfigUpdatedEventArgs 

					(Name, "DoubleSheetSensorEnabled", oldValue, value); 

				RaiseConfigUpdatedEvent(configUpdatedArgs); 

			} 

		} 

 

 

        #endregion 

 

 

        #region Сессия обработки листа 

 

 

        /// <summary> 

        /// Событие "Поступил новый лист" 

        /// </summary> 

        public event EventHandler<SheetEventArgs> NewSheetReceived; 

        /// <summary> 

        /// Событие "Лист обработан" 

        /// </summary> 

        public event EventHandler<SheetEventArgs> SheetProcessed; 

 

 

        /// <summary> 

        /// Текущая сессия обработки листа 

        /// </summary> 

        /// <remarks>инициализируем пустой закрытой сессией</remarks> 

        private SheetProcessingSession _sheetProcessingSession = SheetProcessingSession.ClosedEmptySession; 

        /// <summary> 

        /// Текущая сессия обработки листа 

        /// </summary> 

        public SheetProcessingSession SheetProcessingSession 

        { 

            get 

            { 


                return _sheetProcessingSession; 

            } 

        } 

 

 

        /// <summary> 

        /// Открыть новую сессию обработки листа 

        /// </summary> 

        private void OpenNewSheetProcessingSession() 

        { 

            Logger.LogVerbose(Message.DebugVerbose, "call"); 

 

 

            // создаем новую сессию 

            _sheetProcessingSession = new SheetProcessingSession(); 

 

 

            // сообщим, что поступил новый лист 

            NewSheetReceived.RaiseEvent(this, new SheetEventArgs(_sheetProcessingSession)); 

        } 

 

 

        /// <summary> 

        /// Закрыть сессию обработки листа 

        /// </summary> 

        private void CloseSheetProcessingSession( 

            VotingResult votingResult, SheetProcessingError error) 

        { 

			// если сессия уже закрыта выйдем 

			if (_sheetProcessingSession.Closed) 

				return; 

 

 

            Logger.LogVerbose(Message.ScannerManagerSheetProcessed, votingResult, error); 

 

 

            // сохраняем результаты в сессии 

            _sheetProcessingSession.VotingResult = votingResult; 

            _sheetProcessingSession.Error = error; 

 

 

            // сообщаем, что обработка листа завершена 

            SheetProcessed.RaiseEvent(this, new SheetEventArgs(_sheetProcessingSession)); 

 

 

            // закрываем сессию 

            _sheetProcessingSession.Close(); 

        } 

 

 


        #endregion 

 

 

        #region Управление процессом сканирования 

 

 

        /// <summary> 

        /// Занят ли в данный момент сканер 

        /// </summary> 

        /// <remarks>сканер занят, когда в нем находится лист (не путать со сканированием листа, т.к. лист после того,  

        /// как сканирование завершено, еще некоторое время "висит" в сканере)</remarks> 

        public bool IsBusy 

        { 

            get 

            { 

                return _scanner.ScannerBusy; 

            } 

        } 

 

 

        /// <summary> 

        /// Занят ли в данный момент сканер сканированием листа 

        /// </summary> 

        public bool IsSheetScanning 

        { 

            get 

            { 

                return _scanner.SheetScanning; 

            } 

        } 

 

 

        /// <summary> 

        /// Объект синхронизации процесса сканирования 

        /// </summary> 

        private static object s_scanningSync = new object(); 

 

 

        /// <summary> 

        /// Запустить сканирование 

        /// </summary> 

        /// <returns>запущено ли сканирование</returns> 

        public bool StartScanning() 

        { 

            // загрузим параметры сканера 

            LoadParameters(); 

            // переинициализировать распознавалку 

            _recognitionManager.InitRecognition(); 

            // запустить сканирование 

            return StartScanningInternal(); 


        } 

 

 

        /// <summary> 

        /// Внутренний метод запуска сканирования 

        /// </summary> 

        /// <returns></returns> 

        private bool StartScanningInternal() 

        { 

            lock (s_scanningSync) 

            { 

                Logger.LogVerbose(Message.DebugVerbose, "call"); 

 

 

                // если сканер и так уже сканирует 

                if (_scanner.ScanningEnabled) 

                    // то ничего не делаем 

                    return false; 

 

 

                _scanner.ScanningIndicatorMessage("ЖДИТЕ!"); 

                _scanner.ScanningEnabled = true; 

 

 

                return true; 

            } 

        } 

 

 

        /// <summary> 

        /// Остановить сканирование 

        /// </summary> 

        /// <returns>остановлено ли сканирование</returns> 

        public bool StopScanning() 

        { 

            lock (s_scanningSync) 

            { 

                Logger.LogVerbose(Message.DebugVerbose, "call"); 

 

 

                // если сканер и так не сканирует 

                if (!_scanner.ScanningEnabled) 

                    // то ничего не делаем 

                    return false; 

 

 

                // подождем завершение текущего сканирования 

                int i = 0; 

                while ((_scanner.SheetScanning || _scanner.ScannerBusy) && i++ < 100) 

                { 


                    Thread.Sleep(100); 

                } 

 

 

                _scanner.ScanningEnabled = false; 

 

 

                // TODO: выключить моргание зеленой лампой 

 

 

                return true; 

            } 

        } 

 

 

        /// <summary> 

        /// Перезапустить сканирование 

        /// </summary> 

        /// <remarks>если сканирование не было начато, то ничего не делает,  

        /// иначе последовательно вызывается StopScanning -> StartScanning</remarks> 

        /// <returns>перезапущено ли сканирование</returns> 

        public bool RestoreScanningAfterError() 

        { 

            lock (s_scanningSync) 

            { 

                if (!StopScanning()) 

                    return false; 

 

 

                //TODO: в исходном коде сканирование запускали только, если текущий режим < VotingMode.Results 

                return StartScanningInternal(); 

            } 

        } 

 

 

        /// <summary> 

        /// Сбросить лист 

        /// </summary> 

        /// <param name="markingCode">код метода маркировки листа  

        /// (по сути - это кол-во проколов, которые нужно сделать в листе)</param> 

        public DropResult DropSheet(short markingCode) 

        { 

            Logger.LogVerbose(Message.ScannerManagerDropSheet, markingCode); 

 

 

			return _scanner.Drop(markingCode); 

        } 

 

 

        #endregion 


 
 

        #region Реверс листа 

 

 

        /// <summary> 

        /// Текущая команда реверса листа 

        /// </summary> 

        /// <remarks>используется так: когда на сканер отправляется команда на реверс, то параметры этой 

        /// команды сохраняются в данном объекте. Но сканер может с первого раза не принять команду, и тогда  

        /// она будет отправлена еще раз, когда произойдет вызов метода EnsureSheetReversed</remarks> 

        private ReverseCommand _currentReverseCommand; 

 

 

        /// <summary> 

        /// Реверсировать лист, опущенный в сканер 

        /// </summary> 

        /// <param name="reasonCode">Код причины реверса</param> 

        public void ReverseSheet(int reasonCode) 

        { 

            // создаем команду реверса 

            var reverseCmd = new ReverseCommand(_scanner, reasonCode); 

            // выполняем реверс 

            ExecuteReverseCommand(reverseCmd); 

        } 

 

 

        /// <summary> 

        /// Убедиться, что реверс листа выполнен 

        /// </summary> 

        /// <remarks> 

        /// Метод проверяет, была ли ранее команда на реверс листа, и если команда была, 

        /// но она не была выполнена, то команла реверса листа выполняется повторно 

        /// </remarks> 

        /// <returns> 

        /// true - реверс уже выполнен или команда на реверс отправлена повторно,  

        /// false - реверсировать лист не нужно, т.к. команды на реверс не было 

        /// </returns> 

        public bool EnsureSheetReversed() 

        { 

            // если команды на реверс не было 

            if (_currentReverseCommand == null) 

                return false; 

 

 

            // если команда не была выполнена 

            if (!_currentReverseCommand.Completed) 

                // выполняем её же еще раз 

                ExecuteReverseCommand(_currentReverseCommand); 

 


 
            return true; 

        } 

 

 

        /// <summary> 

        /// Выполнить команду реверса листа 

        /// </summary> 

        /// <param name="reverseCommand">команда реверса</param> 

        private void ExecuteReverseCommand(ReverseCommand reverseCommand) 

        { 

            try 

            { 

                // запоминаем команду 

                _currentReverseCommand = reverseCommand; 

 

 

                Logger.LogInfo(Message.ScannerManagerRevers, _currentReverseCommand.ReasonCode); 

 

 

                // если отправка команды на сканер завершилась неудачно 

                if (!_currentReverseCommand.SendCommand()) 

                { 

                    Logger.LogInfo(Message.ScannerManagerReversRejected); 

                    return; 

                } 

                // иначе - сканер принял команду 

 

 

                Logger.LogVerbose(Message.DebugVerbose, "Выполняется реверс..."); 

 

 

                // ждем освобождения драйвера 

                WaitForScannerFree(); 

 

 

                Logger.LogInfo(Message.ScannerManagerReversSuccessfull); 

 

 

                // сигнализируем о реверсе 

                AlertAboutError(reverseCommand.ReasonCode, false); 

 

 

                // сбросим состояние распознавалки, т.к. в случае с реверсом листа 

                // не будет события SheetIsReady, при обработке которого мы вызываем EndRecognize 

                _recognitionManager.ResetRecognition(); 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.ScannerManagerException, ex, "Ошибка при выполнении команды реверса"); 


                // ждем освобождения драйвера 

                WaitForScannerFree(); 

                // восстановим сканирование 

                RestoreScanningAfterError(); 

            } 

        } 

 

 

        /// <summary> 

        /// Ожидает, когда сканер освободится. Если сканер не занят, то метод сразу же завершает выполнение 

        /// </summary> 

        private void WaitForScannerFree() 

        { 

            while (_scanner.ScannerBusy) 

                Thread.Sleep(200); 

        } 

 

 

        /// <summary> 

        /// Сбросить команду на реверс 

        /// </summary> 

        private void ResetReverseCommand() 

        { 

            _currentReverseCommand = null; 

        } 

 

 

        #endregion 

 

 

        #region Сканирование и его результаты 

 

 

        /// <summary> 

        ///	Кол-во строк отсканированных в последний раз 

        /// </summary> 

        public int ScannedLinesCountLast 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Рабочий буфер верхней стороны 

        /// </summary> 

        public MemoryBlock WorkBufferTop 

        { 

            get 

            { 


                return _scanner.WorkBufferTop; 

            } 

        } 

 

 

        /// <summary> 

        /// Рабочий буфер нижней стороны 

        /// </summary> 

        public MemoryBlock WorkBufferBottom 

        { 

            get 

            { 

                return _scanner.WorkBufferBottom; 

            } 

        } 

 

 

        /// <summary> 

        /// Сканировать полутон в указанный буфер 

        /// </summary> 

        /// <remarks>xCoord и width должны быть кратны 2 и в сумме не превышать 2688</remarks> 

        /// <param name="side">сторона</param> 

        /// <param name="xCoord">координата X</param> 

        /// <param name="yCoord">координата Y</param> 

        /// <param name="width">ширина</param> 

        /// <param name="height">высота</param> 

        /// <param name="image">память, в которую нужно записать запрашиваемый полутон</param> 

        /// <returns>true - дождались события готовности буфера; false - не дождались</returns> 

        public bool GetHalftoneBuffer(ScannedSide side, short xCoord, short yCoord, short width, short height, MemoryBlock image) 

        { 

            // размеры по осям 

            short absX = xCoord; 

            short absY = yCoord; 

 

 

            // получим размеры рабочей зоны 

            if (side == 0) 

            { 

                absX += _scannerConfig.WorkZone.SideTopX; 

                absY += _scannerConfig.WorkZone.SideTopY; 

            } 

            else 

            { 

                absX += _scannerConfig.WorkZone.SideBottomX; 

                absY += _scannerConfig.WorkZone.SideBottomY; 

            } 

 

 

            // Запрашиваем буфер 

            short bufferId; 


            _scanner.GetHalftoneBuffer(side, absX, absY, width, height, image, out bufferId); 

 

 

            Logger.LogVerbose(Message.ScannerManagerGetHalftone, side, xCoord, yCoord, width, height, bufferId); 

 

 

            return true; 

        } 

 

 

        /// <summary> 

        /// Сохраняет изображение буфера в файл 

        /// </summary> 

        /// <param name="filePath">Имя файла</param> 

        /// <param name="imageType">тип сохраняемого изображения</param> 

        /// <param name="side">сторона, которую нужно сохранить. Если Undefined - значит обе</param> 

        /// <param name="bufferSize">размер сохраняемого буфера</param> 

        /// <returns>true - Сохранение прошло успешно; false - Ошибка сохранения</returns> 

        public void SaveBuffer(string filePath, ImageType imageType, ScannedSide side, BufferSize bufferSize) 

        { 

            var res = _scanner.SaveBuffer(filePath, imageType, side, bufferSize); 

 

 

            if (res) // OK 

                Logger.LogVerbose(Message.ScannerManagerBufferSaved, imageType, bufferSize, side, filePath); 

            else // Error 

                Logger.LogError(Message.ScannerManagerBufferSaveError, imageType, bufferSize, side, filePath, res); 

        } 

 

 

        /// <summary> 

        /// Размер буфера изображения 

        /// </summary> 

        /// <param name="imageType">тип сохраняемого изображения</param> 

        /// <param name="bufferSize">размер сохраняемого буфера</param> 

        /// <returns>Размер буфера изображения</returns> 

        public long GetBufferSize(ImageType imageType, BufferSize bufferSize) 

        { 

            return _scanner.GetBufferSize(imageType, bufferSize); 

        } 

 

 

        #endregion 

 

 

        #region Управление индикатором 

 

 

        /// <summary> 

        /// Прокрутчик текста 


        /// </summary> 

        private RollTextMachine _rollTextMachine; 

        /// <summary> 

        /// Объект для синхронизации создания прокрутчика текста 

        /// </summary> 

        private static object s_rollTextMachineSync = new object(); 

 

 

        /// <summary> 

        /// Получить прокрутчик текста 

        /// </summary> 

        /// <remarks>отложенно (не при инициализации подсистемы) создавать прокрутчика приходится потому, 

        /// что ему нужно передать длину индикатора сканера, которую мы сможем получить только после 

        /// того, как будет установлено соединение со сканером</remarks> 

        /// <returns></returns> 

        private RollTextMachine GetRollTextMachine() 

        { 

            if (_rollTextMachine == null) 

                lock (s_rollTextMachineSync) 

                    if (_rollTextMachine == null) 

                    { 

                        // если соединение со сканером не установлено 

                        if (!_scannerConnected) 

                            // то не можем создать прокрутчика 

                            return null; 

 

 

                        // создадим и запустим прокрутчика текста 

                        _rollTextMachine = new RollTextMachine(_scanner.IndicatorLength, false); 

                        _rollTextMachine.NeedSetText += 

                            new RollTextMachine.NeedSetTextDelegate(RollTextMachine_NeedSetText); 

 

 

                        _rollTextMachine.Start(); 

                    } 

 

 

            return _rollTextMachine; 

        } 

 

 

        /// <summary> 

        /// Обработчик события "Установить новый текст" 

        /// </summary> 

        /// <param name="text"></param> 

        private void RollTextMachine_NeedSetText(string text) 

        { 

            if (_scanner != null && !_disposed) 

                _scanner.SetIndicator(text); 

        } 


 
 

        /// <summary> 

        /// Длина индикатора сканера 

        /// </summary> 

        /// <returns></returns> 

        public int IndicatorLength 

        { 

            get 

            { 

                return _scanner == null ? 0 : _scanner.IndicatorLength; 

            } 

        } 

 

 

        /// <summary> 

        /// Установить текст на индикаторе 

        /// </summary> 

        /// <param name="text"></param> 

        public void SetIndicator(string text) 

        { 

            CodeContract.Requires(text != null); 

            if (_scanner != null) 

            { 

                GetRollTextMachine().RolledText = text; 

            } 

        } 

 

 

        #endregion 

 

 

        #region Управление лампами 

 

 

        /// <summary> 

        /// Объект для синхронизации доступа к режиму работы ламп 

        /// </summary> 

        private static object s_lampsRegimeSync = new object(); 

        /// <summary> 

        /// Текущий режим работы ламп 

        /// </summary> 

        private ScannerLampsRegime _lampsRegime = ScannerLampsRegime.BothOff; 

        /// <summary> 

        /// Предыдущий режим работы ламп  

        /// (используется для восстановления режима после того, как закончили предупреждать в режиме Alerting) 

        /// </summary> 

        private ScannerLampsRegime _previousLampsRegime = ScannerLampsRegime.BothOff; 

        /// <summary> 

        /// Событие "Режим работы ламп изменился" 


        /// </summary> 

        private AutoResetEvent _lampsRegimeChanged = new AutoResetEvent(false); 

        /// <summary> 

        /// Счетчик кол-ва миганий во время режима ламп - предупреждение 

        /// </summary> 

        private int _alertingBlinkCount = 0; 

 

 

        /// <summary> 

        /// Установить режим работы ламп 

        /// </summary> 

        /// <param name="regime"></param> 

        public void SetLampsRegime(ScannerLampsRegime regime) 

        { 

            Logger.LogVerbose(Message.ScannerManagerLampsRegime, regime); 

 

 

            lock (s_lampsRegimeSync) 

            { 

                // если хотят установить такой же режим, как текущий 

                if (_lampsRegime == regime) 

                    // то ничего не делаем 

                    return; 

 

 

                if (_lampsRegime != ScannerLampsRegime.Alerting) 

                    _previousLampsRegime = _lampsRegime; 

 

 

                _lampsRegime = regime; 

                _alertingBlinkCount = 0; 

                _lampsRegimeChanged.Set(); 

            } 

        } 

 

 

        /// <summary> 

        /// Восстановить предыдущий режим работы ламп 

        /// </summary> 

        private void RestorePreviousLampsRegime() 

        { 

            lock (s_lampsRegimeSync) 

            { 

                _lampsRegime = _previousLampsRegime; 

                _alertingBlinkCount = 0; 

                _lampsRegimeChanged.Set(); 

            } 

        } 

 

 


        /// <summary> 

        /// Запустить поток управления лампами 

        /// </summary> 

        private void StartManageLampsThread() 

        { 

            // запускаем мигание лампами 

            new Action(ManageLampsThread).BeginInvoke(null, null); 

        } 

 

 

        /// <summary> 

        /// Метод потока управления лампами 

        /// </summary> 

        private void ManageLampsThread() 

        { 

            // максимальное кол-во миганий при предупреждении 

            const int MAX_ALERTING_BLINKS = 6; 

 

 

            while (true) 

            { 

                // признак того, что нужно бесконечно ждать слежующей смены режима работы ламп 

                bool waitInfinity = false; 

 

 

                // текущий режим работы ламп 

                ScannerLampsRegime currentLampsRegime; 

                lock (s_lampsRegimeSync) 

                { 

                    currentLampsRegime = _lampsRegime; 

                } 

 

 

                switch (currentLampsRegime) 

                { 

                    case ScannerLampsRegime.BothOff: 

                        // выключим лампы 

                        _scanner.Red = false; 

                        _scanner.Green = false; 

                        waitInfinity = true; 

                        break; 

 

 

                    case ScannerLampsRegime.GreenOn: 

                        // выключим красную и включим зеленую 

                        _scanner.Red = false; 

                        _scanner.Green = true; 

                        waitInfinity = true; 

                        break; 

 


 
                    case ScannerLampsRegime.GreenBlinking: 

                        // выключим красную, если она горит 

                        if (_scanner.Red) 

                            _scanner.Red = false; 

                        // инвертируем положение зеленой 

                        _scanner.Green = !_scanner.Green; 

                        break; 

 

 

                    case ScannerLampsRegime.Alerting: 

                        // убедимся, чтобы положение ламп было противоположное 

                        if (_scanner.Red == _scanner.Green) 

                        { 

                            _scanner.Red = true; 

                            _scanner.Green = false; 

                        } 

                        else 

                        { 

                            // инвертируем положения обоих ламп 

                            _scanner.Red = !_scanner.Red; 

                            _scanner.Green = !_scanner.Green; 

                        } 

 

 

                        // если уже помигали достаточное кол-во раз 

                        if (++_alertingBlinkCount >= MAX_ALERTING_BLINKS) 

                        { 

                            // восстановим предыдущий режим работы ламп 

                            RestorePreviousLampsRegime(); 

                            continue; 

                        } 

 

 

                        break; 

 

 

                    case ScannerLampsRegime.Scanning: 

                        // выключим зеленую 

                        _scanner.Green = false; 

                        waitInfinity = true; 

                        break; 

 

 

                    default: 

                        throw new Exception("Неизвестный режим работы ламп"); 

                } 

 

 

                // если нужно ждать бесконечно 


                if (waitInfinity) 

                { 

                    // ждем, когда режим изменят 

                    if (!WaitOne(_lampsRegimeChanged, null)) 

                        // был вызван деструктор 

                        return; 

                } 

                else 

                { 

                    // ждем в течение 1 сек 

                    if (WaitHandle.WaitAny( 

                        new WaitHandle[] { _disposeEvent, _lampsRegimeChanged }, 1000, false) == 0) 

                        // был вызван деструктор 

                        return; 

                } 

            } 

        } 

 

 

        /// <summary> 

        /// Сигнализировать об ошибке 

        /// </summary> 

        /// <param name="errorCode"></param> 

		/// <param name="driverError">Признак, что ошибка в драйвере</param> 

		private void AlertAboutError(int errorCode, bool driverError) 

        { 

            // получим конфиг ошибки 

            var errorConfig = GetErrorConfig(errorCode); 

            // если не нашли 

            if (errorConfig == null) 

                // то игнорируем ошибку 

                return; 

 

 

			// если ошибка драйвера и это реверс и нужно сохранять изображения 

			if (driverError && errorConfig.IsReverse && _recognitionManager.NeedSaveImageOnDriverReverse) 

                _recognitionManager.SaveLastImageOnDriverError(errorCode); 

 

 

			// восстановим предыдущий режим работы ламп 

            RestorePreviousLampsRegime(); 

 

 

            // если нужно сигнализировать об этой ошибке и описание ошибки задано 

            if (_config.Alerts.NeedAlertAboutError(errorConfig) && 

                !string.IsNullOrEmpty(errorConfig.Description)) 

            { 

                Logger.LogInfo(Message.ScannerManagerAlertError, errorCode, errorConfig.Description); 

                SetLampsRegime(ScannerLampsRegime.Alerting); 

            } 


 
 

            // закрываем сессию обработки листа 

            var error = new SheetProcessingError(errorConfig.Code, errorConfig.Description, errorConfig.IsReverse); 

            CloseSheetProcessingSession(VotingResult.Empty, error); 

        } 

 

 

        /// <summary> 

        /// Возвращает конфиг ошибки 

        /// </summary> 

        /// <param name="errorCode"></param> 

        /// <returns></returns> 

        private ErrorConfig GetErrorConfig(int errorCode) 

        { 

            if (Enum.IsDefined(typeof(LogicalReverseReason), errorCode)) 

                return new ErrorConfig() 

                { 

                    Code = errorCode, 

                    Description = errorCode.ToString(), 

                    IsReverse = true, 

                    Enabled = true, 

                }; 

 

 

            return _config.Alerts.GetError(errorCode); 

        } 

 

 

        #endregion 

 

 

        #endregion 

 

 

        #region IDisposable Members 

 

 

        public override void Dispose() 

        { 

            base.Dispose(); 

 

 

            if (_scannerConnector != null) 

            { 

                _scannerConnector.Dispose(); 

                _scannerConnector = null; 

            } 

 

 


            if (_rollTextMachine != null) 

            { 

                _rollTextMachine.Dispose(); 

                _rollTextMachine = null; 

            } 

 

 

            if (_sheetProcessingSession != null) 

            { 

                _sheetProcessingSession.Close(); 

                _sheetProcessingSession = null; 

            } 

 

 

            GC.SuppressFinalize(this); 

        } 

 

 

        #endregion 

    } 

}



using System; 
using System.Collections.Generic; 
using System.Diagnostics; 
using System.Linq; 
using System.Threading; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Recognizer; 
using Croc.Bpc.Scanner.Config; 
using Croc.Bpc.Utils; 
using Croc.Bpc.Utils.Images; 
using Croc.Bpc.Voting; 
using Croc.Core; 
using Croc.Core.Configuration; 
using Croc.Core.Extensions; 
using Croc.Core.Utils.Text; 
using Croc.Core.Utils.Threading; 
namespace Croc.Bpc.Scanner 
{ 
    [SubsystemConfigurationElementTypeAttribute(typeof(ScannerManagerConfig))] 
    public class ScannerManager :  
        Subsystem, 
        IScannerConnectorEventHandler, 
        IScannerEventHandler, 
        IScannerManager 
    { 
        private IElectionManager _electionManager; 
        private IVotingResultManager _votingResultManager; 
        private IRecognitionManager _recognitionManager; 
        private IScanner _scanner; 
        private ScannerManagerConfig _config; 
        private ScannerParametersConfig _scannerConfig; 
        private int _scannerStatus; 
        public override void Init(SubsystemConfig config) 
        { 
            _electionManager = Application.GetSubsystemOrThrow<IElectionManager>(); 
            _votingResultManager = Application.GetSubsystemOrThrow<IVotingResultManager>(); 
            _recognitionManager = Application.GetSubsystemOrThrow<IRecognitionManager>(); 
            _sheetProcessingSession = SheetProcessingSession.GetClosedSheetProcessingSession(Logger); 
            ApplyNewConfig(config); 
        } 
        public override void ApplyNewConfig(SubsystemConfig newConfig) 
        { 
            lock (s_loadParametersSync) 
            { 
                _config = (ScannerManagerConfig)newConfig; 
                SelectScannerConfig(); 
                ResetParametersLoaded(); 
            } 
        } 
        private void SelectScannerConfig() 
        { 
            if (_config.ScannerParametersVersions.Count == 0) 
                throw new Exception("Не заданы параметры сканера"); 
            if (_scanner == null) 
            { 
                _scannerConfig = _config.ScannerParametersVersions[0]; 
                return; 
            } 
            var versionName = _scanner.Version.ToString(); 
            _scannerConfig = _config.ScannerParametersVersions[versionName]; 
            if (_scannerConfig == null) 
                throw new Exception(string.Format("Не найдены настройки для сканера версии '{0}'", versionName)); 
        } 
        #region IScannerConnectorEventHandler Members 
        public void Connected(string serialNumber, string ipAddress) 
        { 
            if (NetHelper.IsLocalIPAddress(ipAddress)) 
                return; 
            RemoteScannerConnected.RaiseEvent(this, new ScannerEventArgs(serialNumber, ipAddress)); 
        } 
        public void WantToConnect(string serialNumber, string ipAddress, int scannerStatus) 
        { 
            if (!NetHelper.IsLocalIPAddress(ipAddress)) 
                return; 
            _scannerStatus = scannerStatus; 
            try 
            { 
                if (_scannerConnected) 
                    return; 
                _scanner = _scannerConnector.GetConnectedScanner(serialNumber); 
                SerialNumber = _scanner.SerialNumber; 
                IntSerialNumber = int.Parse(SerialNumber); 
                _scanner.SetEventsHandler(this); 
                SelectScannerConfig(); 
                SetScannerIndicatorText("Инициализация.."); 
                _scannerConnected = true; 
                _connectionDone.Set(); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.ScannerManagerConnectingScannerError, ex); 
            } 
        } 
        public void Disconnected(string serialNumber) 
        { 
            _scannerConnected = false; 
        } 
        #endregion         
        #region Загрузка параметров в сканер 
        private static readonly object s_loadParametersSync = new object(); 
        private bool _parametersLoaded; 
        private void ResetParametersLoaded() 
        { 
            lock (s_loadParametersSync) 
            { 
                _parametersLoaded = false; 
            } 
        } 
        private void LoadParameters() 
        { 
            lock (s_loadParametersSync) 
            { 
                Logger.LogVerbose(Message.Common_DebugCall); 
                if (_parametersLoaded) 
                    return; 
                var oldScanningEnabled = _scanner.ScanningEnabled; 
                _scanner.ScanningEnabled = false; 
                _scanner.SetWorkZone(ScannedSide.Top, _scannerConfig.WorkZone.SideTopX, _scannerConfig.WorkZone.SideTopY); 
                _scanner.SetWorkZone(ScannedSide.Bottom, _scannerConfig.WorkZone.SideBottomX, _scannerConfig.WorkZone.SideBottomY); 
                LoadBlankParameters(); 
                LoadDoubleSheetSensorParameters(); 
                _scanner.TuningEnabled = _scannerConfig.Tuning.Enabled; 
                _scanner.ScanningEnabled = oldScanningEnabled; 
                _parametersLoaded = true; 
            } 
        } 
        private void LoadBlankParameters() 
        { 
            Logger.LogVerbose(Message.Common_DebugCall); 
            var minSheetLength = 0; 
            var maxSheetLength = 0; 
            _scanner.ValidLength_ClearAll(); 
            _scanner.PageOffset_ClearAll(); 
            if (null != _electionManager.SourceData) 
            { 
                var blankSizes = new List<BlankSize>(); 
                var blankOffsets = new List<BlankOffset>(); 
                foreach (var blank in _electionManager.SourceData.Blanks) 
                { 
                    var blankHeight = (int)(blank.Height * _scanner.DpiYTop / 25.4); 
                    var delta = (int)(blank.Delta * _scanner.DpiYTop / 25.4); 
                    if (minSheetLength == 0 || blankHeight - delta < minSheetLength) 
                        minSheetLength = blankHeight - delta; 
                    if (maxSheetLength == 0 || blankHeight + delta > maxSheetLength) 
                        maxSheetLength = blankHeight + delta; 
                    if (blank.Orientation == BlankOrientation.Portrait || 
                        blank.Orientation == BlankOrientation.PortraitAndLandscape) 
                    { 
                        var blankSize = new BlankSize(blank.Width, blank.Height, blank.Delta); 
                        if (!blankSizes.Contains(blankSize)) 
                            blankSizes.Add(blankSize); 
                        if (blank.MaxPortraitShift > 0) 
                        { 
                            var blankOffset = new BlankOffset(blank.Width, blank.MaxPortraitShift); 
                            if (!blankOffsets.Contains(blankOffset)) 
                                blankOffsets.Add(blankOffset); 
                        } 
                    } 
                    if (blank.Orientation == BlankOrientation.Landscape || 
                        blank.Orientation == BlankOrientation.PortraitAndLandscape) 
                    { 
                        var blankSize = new BlankSize(blank.Height, blank.Width, blank.Delta); 
                        if (!blankSizes.Contains(blankSize)) 
                            blankSizes.Add(blankSize); 
                        if (blank.MaxPortraitShift > 0) 
                        { 
                            var blankOffset = new BlankOffset(blank.Height, blank.MaxLandscapeShift); 
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
            _scanner.MinSheetLength = (short)minSheetLength; 
            _scanner.MaxSheetLength = (short)maxSheetLength; 
            _scanner.LengthValidationEnabled = _scannerConfig.CheckFormat.Enabled; 
        } 
        private void LoadDoubleSheetSensorParameters() 
        { 
            Logger.LogVerbose(Message.Common_DebugCall); 
            if (!IsDoubleSheetSensorWork) 
                return; 
            _scanner.DoubleSheetSensorEnabled = _scannerConfig.DoubleSheetSensor.Enabled; 
            if (!_scannerConfig.DoubleSheetSensor.Enabled) 
                return; 
            short levelLeft; 
            short levelRight; 
            _scanner.GetDoubleSheetSensorLevel(out levelLeft, out levelRight); 
            if (_scannerConfig.DoubleSheetSensor.LevelLeft != 0) 
                levelLeft = _scannerConfig.DoubleSheetSensor.LevelLeft; 
            if (_scannerConfig.DoubleSheetSensor.LevelRigth != 0) 
                levelRight = _scannerConfig.DoubleSheetSensor.LevelRigth; 
            _scanner.SetDoubleSheetSensorLevel(levelLeft, levelRight); 
        } 
        #endregion 
        #region IScannerEventHandler Members 
        private int _blankMarker; 
        public void BufferIsReady(IScanner scanner, short bufferId) 
        { 
            Logger.LogVerbose(Message.ScannerManagerBufferIsReady, bufferId); 
        } 
        public void DebugMessage(IScanner scanner, string message, int messageLength) 
        { 
            Logger.LogVerbose(Message.ScannerManagerDebugMessage, message, messageLength); 
        } 
        public void PowerStatistics(IScanner scanner, bool powerFailure, uint min, uint max, uint avg) 
        { 
            if (powerFailure) 
            { 
                Logger.LogError(Message.ScannerManagerPowerFailure, min, max, avg); 
            } 
            else 
            { 
                Logger.LogVerbose(Message.ScannerManagerPowerStatistics, min, max, avg); 
            } 
        } 
        public void NewSheet(IScanner scanner) 
        { 
            Logger.LogInfo(Message.ScannerManagerNewSheet); 
            try 
            { 
                OpenNewSheetProcessingSession(); 
                StopRollTextMachine(SCANNING_INDICATOR_MESSAGE); 
                SetScannerIndicatorText(SCANNING_INDICATOR_MESSAGE); 
                SetLampsRegime(ScannerLampsRegime.Scanning); 
                _blankMarker = -1; 
                var lineWidth = _scanner.WorkZoneW / 8; 
                _recognitionManager.RunRecognition(lineWidth); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.ScannerManagerNewSheetError, ex); 
            } 
            finally 
            { 
                Logger.LogVerbose(Message.Common_DebugReturn); 
            } 
        } 
        public void NextBuffer(IScanner scanner, short linesCount) 
        { 
            Logger.LogVerbose(Message.ScannerManagerNextBufferCall, linesCount); 
            try 
            { 
                int blankMarker; 
                if (!_recognitionManager.ProcessNextBuffer(linesCount, out blankMarker)) 
                    return; 
                if (_blankMarker != blankMarker && IsDoubleSheetSensorWork) 
                { 
                    _blankMarker = blankMarker; 
                    var density = GetBlankPaperDensity(blankMarker); 
                    short leftLevel; 
                    short rightLevel; 
                    scanner.GetDoubleSheetSensorLevel(out leftLevel, out rightLevel); 
                    leftLevel += density; 
                    rightLevel += density; 
                    scanner.SetDoubleSheetSensorCurrentSheetLevel(leftLevel, rightLevel); 
                    Logger.LogVerbose(Message.ScannerManagerDensitySet, blankMarker, density); 
                } 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.ScannerManagerNextBufferError, ex); 
            } 
            finally 
            { 
                Logger.LogVerbose(Message.ScannerManagerNextBufferCall, linesCount); 
            } 
        } 
        private short GetBlankPaperDensity(int blankMarker) 
        { 
            var paperType = _scannerConfig.BlankPaperTypes.GetPaperTypeByMarker(blankMarker); 
            switch (paperType) 
            { 
                case PaperType.Thin:    return _scannerConfig.DoubleSheetSensor.Thin; 
                case PaperType.Thick:   return _scannerConfig.DoubleSheetSensor.Thick; 
                default:                return 0; 
            } 
        } 
        public void SheetIsReady(IScanner scanner, short linesCount, SheetType sheetType) 
        { 
            Logger.LogInfo(Message.ScannerManagerSheetIsReady, linesCount, sheetType); 
            try 
            { 
                _sheetProcessingSession.SheetType = sheetType; 
                _config.Alerts.ResetErrorCounters(); 
                if (linesCount > _scanner.WorkZoneH) 
                    Logger.LogError(Message.ScannerManagerSheetIsReadyTooLarge, _scanner.WorkZoneH); 
                var recognizeThread = new Thread(() => _recognitionManager.EndRecognition(linesCount, sheetType)) 
                { 
                    Name = "Recognize", 
                    IsBackground = true, 
                    Priority = ThreadPriority.Highest 
                }; 
                recognizeThread.Start(); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.ScannerManagerSheetIsReadyError, ex); 
            } 
            finally 
            { 
                Logger.LogVerbose(Message.Common_DebugReturn); 
            } 
        } 
        public void Error(IScanner scanner, ScannerError error) 
        { 
            Logger.LogError(Message.ScannerManagerError, scanner.SerialNumber, error); 
            try 
            { 
                CloseSheetProcessingSessionWithError((int)error, true); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.ScannerManagerAlertingErrorError, ex); 
            } 
        } 
        public void SheetDroped(IScanner scanner, BlankMarking marking, DropResult result) 
        { 
            Logger.LogInfo(Message.ScannerManagerSheetDroped, marking, result); 
            try 
            { 
                var lastVotingResult = _votingResultManager.LastVotingResult; 
                if (marking == BlankMarking.Reverse && result == DropResult.Dropped) 
                { 
                    result = DropResult.ProbablyDropped; 
                    Logger.LogInfo(Message.ScannerManagerSheetDropedResultAdjusted, result); 
                } 
                else if (result == DropResult.Timeout) 
                { 
                    switch (marking) 
                    { 
                        case BlankMarking.DropWithoutMark: 
                        case BlankMarking.DropAndMarkType1: 
                        case BlankMarking.DropAndMarkType2: 
                        case BlankMarking.DropLongWithoutMark: 
                        case BlankMarking.DropLongAndMarkType1: 
                        case BlankMarking.DropLongAndMarkType2: 
                        case BlankMarking.DropWithoutMarkNew: 
                            result = DropResult.Dropped; 
                            break; 
                        case BlankMarking.Reverse: 
                            result = DropResult.Reversed; 
                            break; 
                    } 
                    Logger.LogInfo(Message.ScannerManagerSheetDropedResultAdjusted, result); 
                } 
                CloseSheetProcessingSession(lastVotingResult, result); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.ScannerManagerSheetDropError, ex); 
            } 
            finally 
            { 
                Logger.LogVerbose(Message.Common_DebugReturn); 
            } 
        } 
        public void ReadyToScanning(IScanner scanner) 
        { 
            Logger.LogInfo(Message.ScannerManagerReadyToScanning); 
            try 
            { 
                ExecIfSheetProcessingSessionClosed( 
                    () => 
                        { 
                            if (_sheetProcessingSession.Error != null && _sheetProcessingSession.Error.NeedAlert) 
                            { 
                                Logger.LogInfo(Message.ScannerManagerAlertError, _sheetProcessingSession.Error.Code); 
                                SetLampsRegime(ScannerLampsRegime.Alerting); 
                            } 
                            else 
                            { 
                                RestorePreviousLampsRegime(); 
                            } 
                            LoadParameters(); 
                            _recognitionManager.ResetRecognition(); 
                        }, 
                    1000); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.ScannerManagerReadyToScanError, ex); 
            } 
            finally 
            { 
                Logger.LogVerbose(Message.Common_DebugReturn); 
            } 
        } 
        #endregion 
        #region IQuietMode 
        public bool QuietMode { get; set; } 
        #endregion 
        #region IScannerManager Members 
        #region Подключение к сканеру и диагностика 
        private ScannerConnector _scannerConnector; 
        private volatile bool _scannerConnected; 
        private readonly AutoResetEvent _connectionDone = new AutoResetEvent(false); 
        public bool ScannerConnected 
        { 
            get 
            { 
                return _scannerConnected; 
            } 
        } 
        public bool EstablishConnectionToScanner(int maxTryCount, TimeSpan delay) 
        { 
            CodeContract.Requires(maxTryCount > 0); 
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
            var scannerConnectorSettings = _config.ScannerConnector.Settings.ToNameValueCollection(); 
            var tryCount = 0; 
            while (true) 
            { 
                try 
                { 
                    if (_scannerConnector != null) 
                        _scannerConnector.Dispose(); 
                    _scannerConnected = false; 
                    _scannerConnector = (ScannerConnector)Activator.CreateInstance(connectorClassType); 
                    _scannerConnector.Init(_config.ScannerConnector.BroadcastIPAddress, scannerConnectorSettings); 
                    _connectionDone.Reset(); 
                    _scannerConnector.Connect(this); 
                    if (!_connectionDone.WaitOne(TimeSpan.FromSeconds(10), false)) 
                        throw new Exception("Не дождались установки соединения со сканером"); 
                    Logger.LogInfo(Message.ScannerManagerConnected, _scanner.SerialNumber, _scanner.IPAddress); 
                    StartManageLampsThread(); 
                    return true; 
                } 
                catch (Exception ex) 
                { 
                    if (tryCount >= maxTryCount) 
                    { 
                        Logger.LogError(Message.ScannerManagerCantConnect, ex, ++tryCount); 
                        return false; 
                    } 
                    Logger.LogWarning(Message.ScannerManagerCantConnect, ex, ++tryCount); 
                    Thread.Sleep(delay); 
                } 
            } 
        } 
        public event EventHandler<ScannerEventArgs> RemoteScannerConnected; 
        public void RestartBroadcasting() 
        { 
            _scannerConnector.RestartBroadcasting(); 
        } 
        public List<ScannerDiagnosticsError> PerformDiagnostics() 
        { 
            if (_scanner == null) 
                throw new InvalidOperationException("Сканер не подключен"); 
            var errorList = new List<ScannerDiagnosticsError>(); 
            if (((int)ScannerStatus.BAD_TUNE & _scannerStatus) != 0) 
                errorList.Add(ScannerDiagnosticsError.DoubleSheetSensorNotWork); 
            bool leftWork, rightWork; 
            _scanner.CheckDoubleSheetSensor(out leftWork, out rightWork); 
            if (!leftWork) 
                errorList.Add(ScannerDiagnosticsError.LeftDoubleSheetSensorNotWork); 
            if (!rightWork) 
                errorList.Add(ScannerDiagnosticsError.RightDoubleSheetSensorNotWork); 
            if (_scanner.DriverVersion != _config.DriverVersion.Value) 
                errorList.Add(ScannerDiagnosticsError.WrongDriverVersion); 
            if (((int)ScannerStatus.BAD_CONF & _scannerStatus) != 0) 
                errorList.Add(ScannerDiagnosticsError.WrongDriverConfig); 
            if (((int)ScannerStatus.BAD_LIGHT & _scannerStatus) != 0) 
                errorList.Add(ScannerDiagnosticsError.WrongBrightnessCoefFile); 
            if (((int)ScannerStatus.BAD_VOLT & _scannerStatus) != 0) 
                errorList.Add(ScannerDiagnosticsError.SupplyPowerFailure); 
            if (errorList.Count == 0 && _config.CheckHardware.Enabled) 
                CheckHardware(); 
            return errorList; 
        } 


#if DEBUG 
        private const int DIAGNOSTICS_DELAY = 100; 
#else 
        private const int DIAGNOSTICS_DELAY = 500; 
#endif 
        private void CheckHardware() 
        { 
            try 
            { 
                for (int i = 3; i > 0; --i) 
                { 
                    SetIndicator(i.ToString()); 
                    Thread.Sleep(DIAGNOSTICS_DELAY); 
                } 
                _scanner.EnableLamps(true); 
                for (short j = 1; j <= _scanner.MotorCount; j++) 
                { 
                    CheckMotor(j); 
                } 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.ScannerManagerDiagnosticError, ex); 
            } 
            finally 
            { 
                _scanner.EnableLamps(false); 
                SetScannerIndicatorText(""); 
            } 
        } 
        private void CheckMotor(short motorNumber) 
        { 
            var motorOk = true; 
            for (var direction = 0; direction < 2; direction++) 
            { 
                try 
                { 
                    _scanner.Motor(motorNumber, true, direction, 1); 
                    Thread.Sleep(DIAGNOSTICS_DELAY); 
                } 
                catch (Exception ex) 
                { 
                    Logger.LogError(Message.ScannerManagerMotorError, ex, "запуске", motorNumber, direction, 1); 
                    motorOk = false; 
                } 
                try 
                { 
                    _scanner.Motor(motorNumber, false, direction, 1); 
                } 
                catch (Exception ex) 
                { 
                    Logger.LogError(Message.ScannerManagerMotorError, ex, "остановке", motorNumber, direction, 1); 
                    motorOk = false; 
                } 
            } 
            if(!motorOk) 
                throw new ApplicationException("При проверке мотора произошли исключения"); 
        } 
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
        public string SerialNumber 
        { 
            get; 
            private set; 
        } 
        public int IntSerialNumber 
        { 
            get; 
            private set; 
        } 
        public int DriverVersion 
        { 
            get 
            { 
                return _scanner.DriverVersion; 
            } 
        } 
        public ScannerVersion Version 
        { 
            get 
            { 
                return _scanner.Version; 
            } 
        } 
        public string IPAddress 
        { 
            get 
            { 
                return _scanner.IPAddress; 
            } 
        } 
        public Dictionary<int, PaperType> BlanksPaperType 
        { 
            get 
            { 
                return 
                    _scannerConfig.BlankPaperTypes.Cast<BlankPaperTypeConfig>(). 
                        ToDictionary(pc => pc.BlankMarker, pc => pc.PaperType); 
            } 
        } 
        public void SetBlankPaperType(int blankMarker, PaperType type) 
        { 
            var args = new ConfigUpdatedEventArgs( 
                Name, "BlankPaperType: " + blankMarker, BlanksPaperType[blankMarker], type); 
            _scannerConfig.BlankPaperTypes.Cast<BlankPaperTypeConfig>(). 
                Where(pc => pc.BlankMarker == blankMarker).First().PaperType = type; 
            RaiseConfigUpdatedEvent(args); 
        } 
        #endregion 
        #region Параметры сканера 
        public short DpiXTop 
        { 
            get 
            { 
                return _scanner.DpiXTop; 
            } 
        } 
        public short DpiYTop 
        { 
            get 
            { 
                return _scanner.DpiYTop; 
            } 
        } 
        public short DpiXBottom 
        { 
            get 
            { 
                return _scanner.DpiXBottom; 
            } 
        } 
        public short DpiYBottom 
        { 
            get 
            { 
                return _scanner.DpiYBottom; 
            } 
        } 
        public short DssLeftLevel 
        { 
            get 
            { 
                return _scannerConfig.DoubleSheetSensor.LevelLeft; 
            } 
        } 
        public short DssRightLevel 
        { 
            get 
            { 
                return _scannerConfig.DoubleSheetSensor.LevelRigth; 
            } 
        } 
        public short BinarizationThresholdTop 
        { 
            get 
            { 
                return _scanner.CurrentBinaryThresholdTop; 
            } 
        } 
        public short BinarizationThresholdBottom 
        { 
            get 
            { 
                return _scanner.CurrentBinaryThresholdBottom; 
            } 
        } 
        public bool DoubleSheetSensorEnabled  
        {  
            get 
            { 
                return _scannerConfig.DoubleSheetSensor.Enabled; 
            } 
            set 
            { 
                var oldValue = _scannerConfig.DoubleSheetSensor.Enabled; 
                _scannerConfig.DoubleSheetSensor.Enabled = value; 
                RaiseConfigUpdatedEvent(new ConfigUpdatedEventArgs(Name, "DoubleSheetSensorEnabled", oldValue, value)); 
            } 
        } 
        public void GetDoubleSheetSensorLevel(out short left, out short right) 
        { 
            if (_scanner == null) 
            { 
                left = 0; 
                right = 0; 
                return; 
            } 
            _scanner.GetDoubleSheetSensorLevel(out left, out right); 
        } 
        public void SetDoubleSheetSensorLevel(short left, short right) 
        { 
            var oldValue = GetDoubleSheetSensorLevelString(); 
            _scannerConfig.DoubleSheetSensor.LevelLeft = left; 
            _scannerConfig.DoubleSheetSensor.LevelRigth = right; 
            var value = GetDoubleSheetSensorLevelString(); 
            RaiseConfigUpdatedEvent(new ConfigUpdatedEventArgs(Name, "DoubleSheetSensorLevel", oldValue, value)); 
        } 
        private string GetDoubleSheetSensorLevelString() 
        { 
            return string.Format("left = {0}, right = {1}", 
                                 _scannerConfig.DoubleSheetSensor.LevelLeft, 
                                 _scannerConfig.DoubleSheetSensor.LevelRigth); 
        } 
        public void GetRelativePaperDensity(out short thick, out short thin) 
        { 
            thick = _scannerConfig.DoubleSheetSensor.Thick; 
            thin = _scannerConfig.DoubleSheetSensor.Thin; 
        } 
        public void SetRelativePaperDensity(short thick, short thin) 
        { 
            var oldValue = GetRelativePaperDensityString(); 
            _scannerConfig.DoubleSheetSensor.Thick = thick; 
            _scannerConfig.DoubleSheetSensor.Thin = thin; 
            var value = GetRelativePaperDensityString(); 
            RaiseConfigUpdatedEvent(new ConfigUpdatedEventArgs(Name, "RelativePaperDensity", oldValue, value)); 
        } 
        private string GetRelativePaperDensityString() 
        { 
            return string.Format("thick = {0}, thin = {1}",  
                _scannerConfig.DoubleSheetSensor.Thick, 
                _scannerConfig.DoubleSheetSensor.Thin); 
        } 
        public void GetMarkerParameters( 
            out short on, 
            out short off, 
            out short markingTime, 
            out short rollbackTime, 
            out short downTime) 
        { 
            _scanner.GetMarkerParameters(out on, out off, out markingTime, out rollbackTime, out downTime); 
        } 
        public void SetMarkerParameters( 
            short on, 
            short off, 
            short markingTime, 
            short rollbackTime, 
            short downTime) 
        { 
            _scanner.SetMarkerParameters(on, off, markingTime, rollbackTime, downTime); 
        } 
        #endregion 
        #region Сессия обработки листа 
        public event EventHandler<SheetEventArgs> NewSheetReceived; 
        public event EventHandler<SheetEventArgs> SheetProcessed; 
        private static readonly object s_sheetProcessingSessionSync = new object(); 
        private volatile SheetProcessingSession _sheetProcessingSession; 
        public SheetProcessingSession SheetProcessingSession 
        { 
            get 
            { 
                return _sheetProcessingSession; 
            } 
        } 
        private void OpenNewSheetProcessingSession() 
        { 
            Logger.LogVerbose(Message.ScannerManagerOpenNewSheetProcessingSession); 
            lock (s_sheetProcessingSessionSync) 
            { 
                _sheetProcessingSession.Open(); 
                Logger.LogVerbose(Message.ScannerManagerOpenNewSheetProcessingSessionDone, _sheetProcessingSession.Id); 
                NewSheetReceived.RaiseEvent(this, new SheetEventArgs(_sheetProcessingSession)); 
            } 
        } 
        private void CloseSheetProcessingSessionWithError(int errorCode, bool driverError) 
        { 
            var errorConfig = GetErrorConfig(errorCode); 
            if (errorConfig == null) 
            { 
                Logger.LogWarning(Message.ScannerManagerUnknownErrorOnClosingSheetProcessingSession, 
                    _sheetProcessingSession.Id, errorCode, driverError); 
                return; 
            } 
            lock (s_sheetProcessingSessionSync) 
            { 
                Logger.LogVerbose(Message.ScannerManagerCloseSheetProcessingSessionWithError, 
                              _sheetProcessingSession.Id, errorCode, driverError); 
                var needAlert = 
                    _config.Alerts.NeedAlertAboutError(errorConfig) && 
                    !string.IsNullOrEmpty(errorConfig.Description); 
                var error = new SheetProcessingError( 
                    errorConfig.Code, 
                    errorConfig.Description, 
                    errorConfig.IsReverse, 
                    needAlert); 
                _sheetProcessingSession.Error = error; 
                if (driverError && errorConfig.IsReverse && _recognitionManager.NeedSaveImageOnDriverReverse) 
                    _recognitionManager.SaveLastImageOnDriverError(errorCode);                 
                if (_sheetProcessingSession.Closed) 
                { 
                    Logger.LogVerbose(Message.ScannerManagerTryToCloseAlreadyClosedSheetProcessingSessionWithError, 
                        _sheetProcessingSession.Id, errorCode, driverError); 
                    return; 
                } 
                CoreApplication.Instance.LoggerEnabled = true; 
                Logger.LogVerbose( 
                    Message.ScannerManagerSheetProcessedWithError, 
                    _sheetProcessingSession.Error); 
                _sheetProcessingSession.Close(); 
                StartRollTextMachine(); 
                SheetProcessed.RaiseEvent(this, new SheetEventArgs(_sheetProcessingSession)); 
            } 
        } 
        private ErrorConfig GetErrorConfig(int errorCode) 
        { 
            if (Enum.IsDefined(typeof(LogicalReverseReason), errorCode)) 
                return new ErrorConfig 
                { 
                    Code = errorCode, 
                    Description = errorCode.ToString(), 
                    IsReverse = true, 
                    Enabled = true, 
                }; 
            return _config.Alerts.GetError(errorCode); 
        } 
        private void CloseSheetProcessingSession(VotingResult votingResult, DropResult dropResult) 
        { 
            lock (s_sheetProcessingSessionSync) 
            { 
                if (_sheetProcessingSession.Closed) 
                { 
                    Logger.LogVerbose(Message.ScannerManagerTryToCloseAlreadyClosedSheetProcessingSession,  
                        _sheetProcessingSession.Id); 
                    return; 
                } 
                Logger.LogVerbose(Message.ScannerManagerCloseSheetProcessingSession, _sheetProcessingSession.Id); 
                _sheetProcessingSession.VotingResult = votingResult; 
                _sheetProcessingSession.DropResult = dropResult; 
                CoreApplication.Instance.LoggerEnabled = true; 
                Logger.LogVerbose( 
                    Message.ScannerManagerSheetProcessed, 
                    () => new object[] 
                              { 
                                  _sheetProcessingSession.VotingResult.ToString(), 
                                  _sheetProcessingSession.DropResult 
                              }); 
                _sheetProcessingSession.Close(); 
                StartRollTextMachine(); 
                SheetProcessed.RaiseEvent(this, new SheetEventArgs(_sheetProcessingSession)); 
            } 
        } 
        private bool ExecIfSheetProcessingSessionClosed(Action action, int timeout) 
        { 
            Logger.LogVerbose(Message.Common_DebugCall); 
            _sheetProcessingSession.WaitForClose(timeout); 
            lock (s_sheetProcessingSessionSync) 
            { 
                if (!_sheetProcessingSession.Closed) 
                { 
                    Logger.LogVerbose(Message.Common_Debug, "Ничего не делаем, т.к. сессия не закрыта"); 
                    return false; 
                } 
                action(); 
            } 
            Logger.LogVerbose(Message.Common_DebugReturn); 
            return true; 
        } 
        #endregion 
        #region Управление процессом сканирования 
        private static readonly object s_scanningSync = new object(); 
        public bool StartScanning(ScannerLampsRegime regime) 
        { 
            Logger.LogVerbose(Message.Common_DebugCall); 
            LoadParameters(); 
            _recognitionManager.InitRecognition(); 
            _sheetProcessingSession.Reset(); 
            var res = StartScanningInternal(); 
            SetLampsRegime(regime); 
            Logger.LogVerbose(Message.Common_DebugReturn); 
            return res; 
        } 
        private const string SCANNING_INDICATOR_MESSAGE = "ЖДИТЕ!"; 
        private bool StartScanningInternal() 
        { 
            Logger.LogVerbose(Message.Common_DebugCall); 
            lock (s_scanningSync) 
            { 
                if (_scanner.ScanningEnabled) 
                    return false; 
                _scanner.ScanningIndicatorMessage(SCANNING_INDICATOR_MESSAGE); 
                _scanner.ScanningEnabled = true; 
                return true; 
            } 
        } 
        public bool StopScanning() 
        { 
            Logger.LogVerbose(Message.Common_DebugCall); 
            var stopped = StopScanningInternal(); 
            if (stopped) 
            { 
                while (true) 
                { 
                    while (!ExecIfSheetProcessingSessionClosed( 
                        () => 
                            { 
                                if (_sheetProcessingSession.Error != null && 
                                    _sheetProcessingSession.Error.Code == (int) ScannerError.CantRevercePaper && 
                                    !_sheetProcessingSession.Error.IsRepeated) 
                                { 
                                    _sheetProcessingSession.ErrorSpecified.Reset(); 
                                } 
                            }, 
                        Timeout.Infinite)) 
                    { 
                        Thread.Sleep(500); 
                    } 
                    if (_sheetProcessingSession.Error != null) 
                    { 
                        _sheetProcessingSession.ErrorSpecified.WaitOne(Timeout.Infinite); 
                        _scanner.RestoreNormalState(); 
                    } 
                    Thread.Sleep(500); 
                    if (_sheetProcessingSession.WaitForClose(0)) 
                        break; 
                } 
                SetLampsRegime(ScannerLampsRegime.BothOff); 
                StartRollTextMachine(); 
            } 
            return stopped; 
        } 
        private bool StopScanningInternal() 
        { 
            lock (s_scanningSync) 
            { 
                Logger.LogVerbose(Message.Common_DebugCall); 
                if (!_scanner.ScanningEnabled) 
                    return false; 
                _scanner.ScanningEnabled = false; 
                return true; 
            } 
        } 
        public bool RestoreScanningAfterError() 
        { 
            lock (s_scanningSync) 
            { 
                return StopScanningInternal() && StartScanningInternal(); 
            } 
        } 
        public DropResult DropSheet(BlankMarking marking) 
        { 
            Logger.LogVerbose(Message.ScannerManagerDropSheet, (short)marking); 
            try 
            { 
                return _scanner.Drop(marking); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.ScannerManagerDropSheetFailed, ex, (short)marking); 
                throw; 
            } 
        } 
        public int ExpectedLength 
        { 
            set 
            { 
                _scanner.ExpectedLength = value; 
            } 
        } 
        #endregion 
        #region Реверс листа 
        public void ReverseSheet(int reasonCode) 
        { 
            Logger.LogInfo(Message.ScannerManagerRevers, reasonCode); 
            try 
            { 
                var reverseResult = _scanner.Reverse(); 
                if (reverseResult == ReverseCommandResult.Impossible) 
                { 
                    Logger.LogInfo(Message.ScannerManagerReversRejected); 
                    return; 
                } 
                Logger.LogVerbose(Message.ScannerExecutingRevers); 
                CloseSheetProcessingSessionWithError(reasonCode, false); 
                WaitForScannerFree(); 
                Logger.LogInfo(Message.ScannerManagerReversSuccessfull); 
                _recognitionManager.ResetRecognition(); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.ScannerManagerReverseError, ex); 
                WaitForScannerFree(); 
                RestoreScanningAfterError(); 
            } 
        } 
        private void WaitForScannerFree() 
        { 
            while (_scanner.ScannerBusy) 
                Thread.Sleep(50); 
        } 
        #endregion 
        #region Сканирование и его результаты 
        public MemoryBlock WorkBufferTop 
        { 
            get 
            { 
                return _scanner.WorkBufferTop; 
            } 
        } 
        public MemoryBlock WorkBufferBottom 
        { 
            get 
            { 
                return _scanner.WorkBufferBottom; 
            } 
        } 
        public bool GetHalftoneBuffer(ScannedSide side, short xCoord, short yCoord, short width, short height, MemoryBlock image) 
        { 
            short absX = xCoord; 
            short absY = yCoord; 
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
            short bufferId; 
            _scanner.GetHalftoneBuffer(side, absX, absY, width, height, image, out bufferId); 
            Logger.LogVerbose(Message.ScannerManagerGetHalftone, side, xCoord, yCoord, width, height, bufferId); 
            return true; 
        } 
        public void SaveBuffer(string filePath, ImageType imageType, ScannedSide side, BufferSize bufferSize) 
        { 
            var res = _scanner.SaveBuffer(filePath, imageType, side, bufferSize); 
            if (res) // OK 
                Logger.LogVerbose(Message.ScannerManagerBufferSaved, imageType, bufferSize, side, filePath); 
            else // Error 
                Logger.LogError(Message.ScannerManagerBufferSaveError, imageType, bufferSize, side, filePath, res); 
        } 
        public long GetBufferSize(ImageType imageType, BufferSize bufferSize) 
        { 
            return _scanner.GetBufferSize(imageType, bufferSize); 
        } 
        #endregion 
        #region Управление индикатором 
        private RollTextMachine _rollTextMachine; 
        private static readonly object s_rollTextMachineSync = new object(); 
        private RollTextMachine GetRollTextMachine() 
        { 
            if (_rollTextMachine == null) 
            { 
                if (!_scannerConnected) 
                    return null; 
                _rollTextMachine = new RollTextMachine(_scanner.IndicatorLength, false); 
                _rollTextMachine.NeedSetText += RollTextMachine_NeedSetText; 
                _rollTextMachine.Start(); 
            } 
            return _rollTextMachine; 
        } 
        private bool RollTextMachine_NeedSetText(string text) 
        { 
            if (// сканер еще не подключен 
                _scanner == null || 
                _scanner.SheetScanning || 
                _disposed) 
            { 
                return false; 
            } 
            SetScannerIndicatorText(text); 
            return true; 
        } 
        private void SetScannerIndicatorText(string text) 
        { 
            if (QuietMode) 
                return; 
            _scanner.SetIndicator(text); 
        } 
        private void StartRollTextMachine() 
        { 
            lock (s_rollTextMachineSync) 
            { 
                var rollTextMachine = GetRollTextMachine(); 
                rollTextMachine.Start(); 
                Logger.LogVerbose(Message.RollTextMachineStarted, rollTextMachine.RolledText); 
            } 
        } 
        private void StopRollTextMachine(string textToRollAfterStart) 
        { 
            lock (s_rollTextMachineSync) 
            { 
                var rollTextMachine = GetRollTextMachine(); 
                rollTextMachine.Stop(); 
                if (!string.IsNullOrEmpty(textToRollAfterStart)) 
                    rollTextMachine.RolledText = textToRollAfterStart; 
                Logger.LogVerbose(Message.RollTextMachineStopped, 
                                  () => new object[] 
                                            { 
                                                rollTextMachine.RolledText, 
                                                textToRollAfterStart ?? "<не задан>" 
                                            }); 
            } 
        } 
        public int IndicatorLength 
        { 
            get 
            { 
                return _scanner == null ? 0 : _scanner.IndicatorLength; 
            } 
        } 
        public void SetIndicator(string text) 
        { 
            CodeContract.Requires(text != null); 
            if (_scanner == null) 
                return; 
            lock (s_rollTextMachineSync) 
            { 
                var rollTextMachine = GetRollTextMachine(); 
                if (rollTextMachine.State != RollTextMachine.MachineState.Running) 
                { 
                    Logger.LogVerbose(Message.ScannerManagerTryToSetTextToRollWhenStopped, text); 
                    return; 
                } 
                Logger.LogVerbose(Message.ScannerManagerSetTextToRoll, text); 
                rollTextMachine.RolledText = text; 
            } 
        } 
        #endregion 
        #region Управление лампами 
        private static readonly object s_lampsRegimeSync = new object(); 
        private ScannerLampsRegime _lampsRegime = ScannerLampsRegime.BothOff; 
        private ScannerLampsRegime _previousLampsRegime = ScannerLampsRegime.BothOff; 
        private readonly AutoResetEvent _lampsRegimeChanged = new AutoResetEvent(false); 
        private int _alertingBlinkCount; 
        private static bool IsTemporaryLampsRegime(ScannerLampsRegime regime) 
        { 
            return 
                regime == ScannerLampsRegime.Alerting || 
                regime == ScannerLampsRegime.Scanning; 
        } 
        public void SetLampsRegime(ScannerLampsRegime lampsRegime) 
        { 
            lock (s_lampsRegimeSync) 
            { 
                if (_lampsRegime == lampsRegime) 
                { 
                    Logger.LogVerbose(Message.ScannerManagerTrySetEqualsLampsRegime, lampsRegime); 
                    return; 
                } 
                ChangeLampsRegime(lampsRegime); 
                _alertingBlinkCount = 0; 
                _lampsRegimeChanged.Set(); 
            } 
        } 
        private void ChangeLampsRegime(ScannerLampsRegime newRegime) 
        { 
            Logger.LogVerbose(Message.ScannerManagerSetLampsRegime, _lampsRegime, newRegime); 
            if (!IsTemporaryLampsRegime(_lampsRegime) && IsTemporaryLampsRegime(newRegime)) 
                _previousLampsRegime = _lampsRegime; 
            _lampsRegime = newRegime; 
        } 
        private void RestorePreviousLampsRegime() 
        { 
            lock (s_lampsRegimeSync) 
            { 
                if (!IsTemporaryLampsRegime(_lampsRegime)) 
                { 
                    Logger.LogVerbose( 
                        Message.ScannerManagerLateTryRestoreLampsRegime, _lampsRegime, _previousLampsRegime); 
                    return; 
                } 
                Logger.LogVerbose(Message.ScannerManagerRestoreLampsRegime, _lampsRegime, _previousLampsRegime); 
                _lampsRegime = _previousLampsRegime; 
                _alertingBlinkCount = 0; 
                _lampsRegimeChanged.Set(); 
            } 
        } 
        private void StartManageLampsThread() 
        { 
            ThreadUtils.StartBackgroundThread(ManageLampsThread); 
        } 
        private void ManageLampsThread() 
        { 
            const int MAX_ALERTING_BLINKS = 10; 
            while (!_disposed) 
            { 
                var waitInfinity = false; 
                ScannerLampsRegime currentLampsRegime; 
                lock (s_lampsRegimeSync) 
                { 
                    if (_scanner.SheetScanning && _lampsRegime != ScannerLampsRegime.Scanning) 
                        ChangeLampsRegime(ScannerLampsRegime.Scanning); 
                    currentLampsRegime = _lampsRegime; 
                } 
                switch (currentLampsRegime) 
                { 
                    case ScannerLampsRegime.BothOff: 
                        _scanner.Red = false; 
                        _scanner.Green = false; 
                        waitInfinity = true; 
                        break; 
                    case ScannerLampsRegime.GreenOn: 
                        _scanner.Red = false; 
                        _scanner.Green = true; 
                        waitInfinity = true; 
                        break; 
                    case ScannerLampsRegime.GreenBlinking: 
                        if (_scanner.Red) 
                            _scanner.Red = false; 
                        _scanner.Green = !_scanner.Green; 
                        break; 
                    case ScannerLampsRegime.GreenAndRedBlinking: 
                        if (_scanner.Red != _scanner.Green) 
                        { 
                            _scanner.Red = true; 
                            _scanner.Green = true; 
                        } 
                        else 
                        { 
                            _scanner.Red = !_scanner.Red; 
                            _scanner.Green = !_scanner.Green; 
                        } 
                        break; 
                    case ScannerLampsRegime.Alerting: 
                        if (_scanner.Red == _scanner.Green) 
                        { 
                            _scanner.Red = true; 
                            _scanner.Green = false; 
                        } 
                        else 
                        { 
                            _scanner.Red = !_scanner.Red; 
                            _scanner.Green = !_scanner.Green; 
                        } 
                        if (++_alertingBlinkCount >= MAX_ALERTING_BLINKS) 
                        { 
                            RestorePreviousLampsRegime(); 
                            continue; 
                        } 
                        break; 
                    case ScannerLampsRegime.Scanning: 
                        _scanner.Red = true; 
                        _scanner.Green = false; 
                        waitInfinity = true; 
                        break; 
                    default: 
                        throw new Exception("Неизвестный режим работы ламп"); 
                } 
                if (waitInfinity) 
                { 
                    if (!WaitOne(_lampsRegimeChanged, null)) 
                        return; 
                } 
                else 
                { 
                    if (WaitHandle.WaitAny( 
                        new WaitHandle[] { _disposeEvent, _lampsRegimeChanged }, 1000, false) == 0) 
                        return; 
                } 
            } 
        } 
        #endregion 
        #endregion 
        #region IDisposable Members 
        public override void Dispose() 
        { 
            base.Dispose(); 
            StopScanning(); 
            Thread.Sleep(1000); 
            if (_rollTextMachine != null) 
            { 
                _rollTextMachine.Dispose(); 
                _rollTextMachine = null; 
            } 
            if (_scannerConnector != null) 
            { 
                SetLampsRegime(ScannerLampsRegime.BothOff); 
                _scannerConnector.Dispose(); 
                _scannerConnector = null; 
            } 
            GC.SuppressFinalize(this); 
        } 
        #endregion 
    } 
}

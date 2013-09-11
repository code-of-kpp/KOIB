using System; 
using System.IO; 
using System.Linq; 
using System.Text; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.FileSystem; 
using Croc.Bpc.Recognizer.Config; 
using Croc.Bpc.Recognizer.Ocr; 
using Croc.Bpc.Scanner; 
using Croc.Bpc.Utils; 
using Croc.Bpc.Utils.Images; 
using Croc.Bpc.Utils.Images.Tiff; 
using Croc.Bpc.Voting; 
using Croc.Core; 
using Croc.Core.Configuration; 
using Croc.Core.Diagnostics; 
using Croc.Core.Utils.IO; 
using Croc.Core.Utils; 
namespace Croc.Bpc.Recognizer 
{ 
    [SubsystemConfigurationElementTypeAttribute(typeof(RecognitionManagerConfig))] 
    public class RecognitionManager : 
        Subsystem, 
        IOcrEventHandler, 
        IRecognitionManager 
    { 
        private const string MODEL_FILE_NAME = "MODEL.DAT"; 
        private const string MODEL_TEXT_FILE_NAME_FORMAT = "model.{0}.txt"; 
        private const string OCR_LOG_NAME = "ocr.txt"; 
        private RecognitionManagerConfig _config; 
        private IElectionManager _electionManager; 
        private IVotingResultManager _votingResultManager; 
        private IScannerManager _scannerManager; 
        private IFileSystemManager _fileSystemManager; 
        private IOcr _ocr; 
        private string _modelTextFilePath; 
        private string _recErrorFilePathFormat; 
        private string _imageFileNamePrefixFormat; 
        private ILogger _recognitionResultLogger; 
        private string OcrLogFilePath 
        { 
            get 
            { 
                return Path.Combine( 
                    _fileSystemManager.GetDataDirectoryPath(FileType.Log), 
                    OCR_LOG_NAME); 
            } 
        } 
        private bool AllowSuperiorStamp 
        { 
            get 
            { 
                return 
                    _electionManager.CurrentVotingMode == VotingMode.Test || 
                    _config.SuperiorStamp.Enabled; 
            } 
        } 
        #region Инициализация подсистемы 
        public override void Init(SubsystemConfig config) 
        { 
            _config = (RecognitionManagerConfig)config; 
            _electionManager = Application.GetSubsystemOrThrow<IElectionManager>(); 
            _votingResultManager = Application.GetSubsystemOrThrow<IVotingResultManager>(); 
            _scannerManager = Application.GetSubsystemOrThrow<IScannerManager>(); 
            _fileSystemManager = Application.GetSubsystemOrThrow<IFileSystemManager>(); 
            CreateOcr(); 
            GenerateFilePaths(); 
            _recognitionResultLogger = new RecognitionResultLogger(LogFileFolder); 
            RecognitionMode = RecognitionMode.BulletinRecognition; 
        } 
        public override void ApplyNewConfig(SubsystemConfig newConfig) 
        { 
            _config = (RecognitionManagerConfig)newConfig; 
            InitRecognition(); 
        } 
        private void CreateOcr() 
        { 
            _ocr = new Ocr.Ocr(); 
            _ocr.SetEventsHandler(this); 
            _ocr.StampTestLevel = _config.Ocr.Stamp.TestLevel; 
            _ocr.ModelFilePath = Path.Combine( 
                _fileSystemManager.GetDataDirectoryPath(FileType.RuntimeData), 
                MODEL_FILE_NAME); 
            _ocr.Init(); 
        } 
        private void GenerateFilePaths() 
        { 
            var runtimeDataDirPath = _fileSystemManager.GetDataDirectoryPath(FileType.RuntimeData); 
            _modelTextFilePath = string.Format(MODEL_TEXT_FILE_NAME_FORMAT, _scannerManager.SerialNumber); 
            _modelTextFilePath = Path.Combine(runtimeDataDirPath, _modelTextFilePath); 
            var sb = new StringBuilder(); 
            sb.Append(runtimeDataDirPath); 
            sb.Append("{0:ddMM_HHmm}_{1}_{2}_REC_ERROR.txt"); 
            _recErrorFilePathFormat = sb.ToString(); 
            sb.Length = 0; 
            sb.Append("{0:ddMMHHmm}_{1}_{2}_"); 
            _imageFileNamePrefixFormat = sb.ToString(); 
        } 
        #endregion 
        #region Инициализация распознавалки 
        private static readonly object s_initRecognitionSync = new object(); 
        public void CheckCreateModel(SourceData sourceData) 
        { 
            CreateModel(sourceData); 
        } 
        private Model CreateModel(SourceData sourceData) 
        { 
            if (_ocr == null) 
                throw new Exception("Модуль распознавания не создан"); 
            try 
            { 
                var model = new Model(_config.Ocr.Marker.Type); 
                model.Create(sourceData); 
                Logger.LogVerbose(Message.RecognizerModelCreated); 
                return model; 
            } 
            catch (Exception ex) 
            { 
                throw new Exception("Ошибка создания модели: " + ex.Message, ex); 
            } 
        } 
        public void InitRecognition() 
        { 
            Logger.LogVerbose(Message.Common_DebugCall); 
            lock (s_initRecognitionSync) 
            { 
                if (_ocr == null) 
                    throw new Exception("Модуль распознавания не создан"); 
                if (_electionManager.SourceData == null) 
                    return; 
                var model = CreateModel(_electionManager.SourceData); 
                _ocr.ClearStamps(); 
                if (_config.Ocr.Stamp.TestLevel != StampTestLevel.Halftone && 
                    _config.Ocr.Stamp.TestLevel != StampTestLevel.None) 
                { 
                    _ocr.AddStamp(_electionManager.SourceData.Uik); 
                    if (AllowSuperiorStamp) 
                    { 
                        foreach (var election in _electionManager.SourceData.Elections) 
                            foreach (var stampCommittee in election.StampCommittees) 
                            { 
                                if (stampCommittee.Num > 0) 
                                { 
                                    try 
                                    { 
                                        _ocr.AddStamp(stampCommittee.Num); 
                                    } 
                                    catch (Exception ex) 
                                    { 
                                        Logger.LogError(Message.RecognizerSuperiorStampError, ex); 
                                    } 
                                } 
                            } 
                    } 
                } 
                _ocr.InitRecognize(); 
                LoadOcrParameters(); 
#if DEBUG 
                model.SaveAsText(_modelTextFilePath); 
#endif 
                RefreshCanceledCandidatesInfo(); 
                _votingResultManager.ResetLastVotingResult(); 
            } 
        } 
        private void LoadOcrParameters() 
        { 
            _ocr.StampTestLevel = _config.Ocr.Stamp.TestLevel == StampTestLevel.Halftone 
                ? StampTestLevel.None : _config.Ocr.Stamp.TestLevel; 
            _ocr.OnlineRecognitionLevel = _config.Ocr.OnlineRecognize.Level; 
            _ocr.CutWeakCheck = _config.Ocr.CutWeakCheck.Enabled; 
            _ocr.LookForLostSquare = _config.Ocr.LookForLostSquare.Enabled; 
            _ocr.StampDigitXsize = _config.Ocr.Stamp.Digital.XSize; 
            _ocr.StampDigitYsize = _config.Ocr.Stamp.Digital.YSize; 
            _ocr.StampDigitGap = _config.Ocr.Stamp.Digital.Gap; 
            _ocr.StampDigitDistBotLine = _config.Ocr.Stamp.Digital.DistBottom; 
            _ocr.StampDigitDistLftLine = _config.Ocr.Stamp.Digital.DistLeft; 
            _ocr.StampDigitDistRghLine = _config.Ocr.Stamp.Digital.DistRight; 
            _ocr.StampFrameWidth = _config.Ocr.Stamp.FrameWidth; 
            _ocr.StampDigitMinLineWidth = _config.Ocr.Stamp.Digital.MinLineWidth; 
            _ocr.StampDigitMaxLineWidth = _config.Ocr.Stamp.Digital.MaxLineWidth; 
            _ocr.StampVSize = _config.Ocr.Stamp.VerticalSize; 
            _ocr.MinMarkerWid = _config.Ocr.Marker.Digital.Width.Min; 
            _ocr.MaxMarkerWid = _config.Ocr.Marker.Digital.Width.Max; 
            _ocr.MinMarkerHgh = _config.Ocr.Marker.Digital.Height.Min; 
            _ocr.MaxMarkerHgh = _config.Ocr.Marker.Digital.Height.Max; 
            _ocr.MinMarkerRio = _config.Ocr.Marker.Digital.Rio.Min; 
            _ocr.MaxMarkerRio = _config.Ocr.Marker.Digital.Rio.Max; 
            _ocr.BlankTestStart = _config.Ocr.OnlineRecognize.BlankTestZone.StartAtLine; 
            _ocr.BlankTestStop = _config.Ocr.OnlineRecognize.BlankTestZone.StopAtLine; 
            _ocr.MinCheckArea = _config.Ocr.MinCheckArea.Value; 
            _ocr.SeekBottomRightLine = _config.Ocr.SeekBottomRightLine.Enabled; 
            _ocr.MaxOnlineSkew = _config.Ocr.MaxOnlineSkew.Value; 
            _ocr.MinStandartMarkerWid = _config.Ocr.Marker.Standard.Width.Min; 
            _ocr.MaxStandartMarkerWid = _config.Ocr.Marker.Standard.Width.Max; 
            _ocr.MinStandartMarkerHgh = _config.Ocr.Marker.Standard.Height.Min; 
            _ocr.MaxStandartMarkerHgh = _config.Ocr.Marker.Standard.Height.Max; 
            _ocr.StandartMarkerZone = _config.Ocr.Marker.Standard.Zone.Value; 
            _ocr.StampLowThr = _config.Ocr.Stamp.LowThr; 
            _ocr.OffsetFirstRule = _scannerManager.Version == ScannerVersion.V2003 ? 0 : -1; 
        } 
        private void RefreshCanceledCandidatesInfo() 
        { 
            if (_electionManager.SourceData == null) 
                return; 
            for (int blankIndex = 0; blankIndex < _electionManager.SourceData.Blanks.Length; blankIndex++) 
            { 
                var curBlank = _electionManager.SourceData.Blanks[blankIndex]; 
                for (int i = 0; i < curBlank.Sections.Length; i++) 
                { 
                    var curElection = _electionManager.SourceData.GetElectionByNum(curBlank.Sections[i]); 
                    foreach (var curCand in curElection.Candidates) 
                    { 
                        if (curCand.NoneAbove) 
                            continue; 
                        var square = curCand.Number - 1; 
                        int res; 
                        _ocr.OCR_IsSquareValid(blankIndex, i, square, out res); 
                        if (curCand.Disabled && res == 1) 
                        { 
                            _ocr.OCR_ExcludeSquare(blankIndex, i, square); 
                        } 
                        else if (!curCand.Disabled && res == 0) 
                        { 
                            _ocr.OCR_RestoreSquare(blankIndex, i, square); 
                        } 
                    } 
                } 
            } 
        } 
        #endregion 
        #region IRecognitionManager Members 
        public RecognitionMode RecognitionMode { get; set; } 
        public bool NeedSaveImageOnDriverReverse 
        { 
            get 
            { 
                return _config.DebugImageSaving.DriverReverse; 
            } 
        } 
        public bool StampControlEnabled 
        { 
            get 
            { 
                return _config.Ocr.Stamp.TestLevel != StampTestLevel.None; 
            } 
            set 
            { 
                var oldValue = _config.Ocr.Stamp.TestLevel; 
                _config.Ocr.Stamp.TestLevel = value ? StampTestLevel.Halftone : StampTestLevel.None; 
                RaiseConfigUpdatedEvent( 
                    new ConfigUpdatedEventArgs(Name, "StampControlEnabled", oldValue, _config.Ocr.Stamp.TestLevel)); 
            } 
        } 
        public BlankMarking GetBlankMarking(BlankType blankType) 
        { 
            var blankConfig = _config.Blanks.Get(blankType, SheetType.Normal); 
            return blankConfig != null 
                       ? blankConfig.Marking 
                       : BlankMarking.DropWithoutMark; 
        } 
        public void SetBlankMarking(BlankType blankType, BlankMarking marking) 
        { 
            var blankConfig = _config.Blanks.Get(blankType, SheetType.Normal); 
            if (blankConfig == null) 
                return; 
            var oldValue = blankConfig.Marking; 
            blankConfig.Marking = marking; 
            RaiseConfigUpdatedEvent( 
                new ConfigUpdatedEventArgs(Name, blankType + "BlankMarking", oldValue, marking)); 
        } 
        public RecognitionResult LastRecognitionResult 
        { 
            get; 
            private set; 
        } 
        #region Процесс распознавания 
        private volatile bool _recognitionPerformNow; 
        private VotingMode _recognitionStartVotingMode; 
        private int _scannedLinesCount; 
        private SheetType _sheetType; 
        private bool _reverseCommandWasSent; 
        public void RunRecognition(int lineWidth) 
        { 
            Logger.LogVerbose(Message.Common_DebugCall); 
            if (_ocr == null) 
                return; 
            _recognitionPerformNow = true; 
            _recognitionStartVotingMode = _electionManager.CurrentVotingMode; 
            _scannedLinesCount = 0; 
            _reverseCommandWasSent = false; 
            SetOcrDpi(); 
            if (File.Exists(OcrLogFilePath)) 
            { 
                try 
                { 
                    _ocr.EnableLogging(null); 
                    File.Delete(OcrLogFilePath); 
                } 
                catch (Exception ex) 
                { 
                    Logger.LogWarning(Message.RecognizerOcrLogDeletingError, ex); 
                } 
            } 
            _ocr.EnableLogging(_config.Ocr.LoggingEnabled.Enabled ? OcrLogFilePath : null); 
            Logger.LogVerbose(Message.RecognizerOCRRunRecognizeCall); 
            _ocr.RunRecognize(_scannerManager.WorkBufferTop, _scannerManager.WorkBufferBottom, lineWidth, lineWidth, 0, 0); 
            Logger.LogVerbose(Message.RecognizerOCRRunRecognizeReturn); 
        } 
        private void SetOcrDpi() 
        { 
            Logger.LogVerbose(Message.RecognizerSetDpi, _scannerManager.DpiXTop, _scannerManager.DpiYTop, 
                _scannerManager.DpiXBottom, _scannerManager.DpiYBottom); 
            _ocr.SetDpi( 
                _scannerManager.DpiXTop, _scannerManager.DpiYTop, 
                _scannerManager.DpiXBottom, _scannerManager.DpiYBottom); 
        } 
        public bool ProcessNextBuffer(short linesCount, out int blankMarker) 
        { 
            try 
            { 
                Logger.LogVerbose(Message.RecognizerNextBufferCall, linesCount); 
                blankMarker = -1; 
                if (_scannedLinesCount == linesCount) 
                { 
                    return false; 
                } 
                _scannedLinesCount = linesCount; 
                if (_config.DebugImageSaving.NextBuffer) 
                { 
                    SaveLastImage("NEXTBUFFER_" + _scannedLinesCount, ImageSavingType.Binary, null); 
                } 
                if (RecognitionMode == RecognitionMode.BulletinRecognition && 
                    !_scannerManager.SheetProcessingSession.ReceivingAllowed) 
                { 
                    ReverseSheet((int)LogicalReverseReason.SheetReceivingForbidden); 
                } 
                if ( // ранее была отправлена команда на реверс 
                    _reverseCommandWasSent || 
                    (RecognitionMode == RecognitionMode.BulletinRecognition && !_config.Ocr.OnlineRecognize.Enabled) || 
                    _ocr == null || 
                    _scannedLinesCount < _config.Ocr.OnlineRecognize.RunZone.StartAtLine || 
                    (_scannedLinesCount > _config.Ocr.OnlineRecognize.RunZone.StopAtLine && 
                        _config.Ocr.OnlineRecognize.RunZone.StopAtLine != LineZoneConfig.INFINITY)) 
                { 
                    return false; 
                } 
                Logger.LogVerbose(Message.RecognizerOcrNextBufferCall, _scannedLinesCount); 
                var blankIndex = _ocr.NextBuffer(_scannedLinesCount); 
#if DEBUG 
                if (!PlatformDetector.IsUnix) 
                    blankIndex = 0; 
#endif 
                Logger.LogVerbose(Message.RecognizerOcrNextBufferReturn, _scannedLinesCount, blankIndex); 
                if (RecognitionMode != RecognitionMode.BulletinRecognition) 
                { 
                    return false; 
                } 
                if (blankIndex == (int)OnlineMarkerResult.Impossible) 
                { 
                    ReverseSheet((int)LogicalReverseReason.InvalidBlankNumber); 
                    return false; 
                } 
                if (blankIndex < 0) 
                { 
                    ReverseSheet(blankIndex); 
                    return false; 
                } 
                if (blankIndex >= _electionManager.SourceData.Blanks.Length) 
                { 
                    ReverseSheet((int)LogicalReverseReason.InvalidBlankNumber); 
                    return false; 
                } 
                var blank = _electionManager.SourceData.Blanks[blankIndex]; 
                if (!_electionManager.SourceData.IsVotingModeValidForBlank(blank, _recognitionStartVotingMode)) 
                { 
                    ReverseSheet((int)LogicalReverseReason.BlankHasNoCurrentVoteRegime); 
                    return false; 
                } 
                Logger.LogVerbose(Message.RecognizerOnlineBulletinValid); 
                blankMarker = blank.Marker; 
                _scannerManager.ExpectedLength = blank.Height + blank.Delta; 
                return true; 
            } 
            finally 
            { 
                Logger.LogVerbose(Message.Common_DebugReturn); 
            } 
        } 
        private void ReverseSheet(int reasonCode) 
        { 
            if (_config.DebugImageSaving.Reverse) 
                SaveLastImage("REVERSE", ImageSavingType.Binary, null); 
            _scannerManager.ReverseSheet(reasonCode); 
            _reverseCommandWasSent = true; 
        } 
        #endregion 
        #region Завершение распознавания 
        public void ResetRecognition() 
        { 
            Logger.LogVerbose(Message.Common_DebugCall); 
            if (_ocr == null || !_recognitionPerformNow) 
                return; 
            try 
            { 
                _recognitionPerformNow = false; 
                _ocr.EndRecognize(MarkerType.None); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.RecognizerResetError, ex); 
            } 
        } 
        public void EndRecognition(short linesCount, SheetType sheetType) 
        { 
            _scannedLinesCount = linesCount; 
            _sheetType = sheetType; 
            switch (RecognitionMode) 
            { 
                case RecognitionMode.BulletinRecognition: 
                    EndRecognizeBulletin(); 
                    break; 
                case RecognitionMode.BulletinGeometryTesting: 
                    break; 
                default: 
                    throw new Exception("Неизвестный режим работы: " + RecognitionMode); 
            } 
        } 
        private void EndRecognizeBulletin() 
        { 
            Logger.LogVerbose(Message.Common_DebugCall); 
            var marking = _config.Blanks.Get(BlankType.Bad).Marking; 
            RecognitionResult recResult = null; 
            try 
            { 
                if (_ocr != null) 
                { 
                    _recognitionPerformNow = false; 
                    SetOcrDpi(); 
                    int recResultCode; 
                    try 
                    { 
                        Logger.LogVerbose(Message.RecognizerLinesInfo, _scannedLinesCount); 
                        if (_config.DebugImageSaving.NextBuffer) 
                            SaveLastImage("LAST_NEXTBUFFER_" + _scannedLinesCount, ImageSavingType.Binary, null); 
                        Logger.LogVerbose(Message.RecognizerBeforeNextBufferCall); 
                        _ocr.NextBuffer(_scannedLinesCount); 
                        if (_config.DebugImageSaving.Presave) 
                            SaveLastImage("PRESAVE", ImageSavingType.All, null); 
                        Logger.LogVerbose(Message.RecognizerBeforeEndRecognizeCall); 
                        recResultCode = _ocr.EndRecognize(_config.Ocr.Marker.Type); 
                        Logger.LogVerbose(Message.RecognizerEndRecognizeReturn); 
                    } 
                    catch (Exception ex) 
                    { 
                        recResultCode = (int)OcrRecognitionResult.ERROR; 
                        Logger.LogError(Message.RecognizerBulletinError, ex); 
                        try 
                        { 
                            SaveLastImage("REC-ERROR", ImageSavingType.All, null); 
                        } 
                        catch (Exception exInner) 
                        { 
                            Logger.LogError(Message.RecognizerSaveImageError, exInner); 
                        } 
                        try 
                        { 
                            if (_config.Ocr.LoggingEnabled.Enabled) 
                            { 
                                _ocr.EnableLogging(null); 
                                var filePath = GetRecErrorFilePath(_scannerManager.SerialNumber); 
                                File.Copy(OcrLogFilePath, filePath, true); 
                            } 
                        } 
                        catch (Exception exInner) 
                        { 
                            Logger.LogWarning(Message.RecognizerSaveOrcLogCopyError, exInner); 
                        } 
                    } 
                    try 
                    { 
                        recResult = RecognitionResultAnalisys(recResultCode); 
                        marking = recResult.Marking; 
                        Logger.LogVerbose(Message.RecognizerRecognitionResultSaved); 
                    } 
                    catch (Exception ex) 
                    { 
                        Logger.LogError(Message.RecognizerSaveResultError, ex); 
                        marking = _config.Blanks.Get(BlankType.Bad).Marking; 
                    } 
                    if (recResultCode == (int)OcrRecognitionResult.CALL || 
                        recResultCode == (int)OcrRecognitionResult.ERROR) 
                    { 
                        try 
                        { 
                            InitRecognition(); 
                        } 
                        catch (Exception ex) 
                        { 
                            Logger.LogError(Message.RecognizerInitError, ex); 
                        } 
                    } 
                } 
                Logger.LogVerbose(Message.RecognizerMarkSheet, marking); 
                var dropResult = _scannerManager.DropSheet(marking); 
                Logger.LogVerbose(Message.RecognizerSheetDroped, dropResult); 
                switch (dropResult) 
                { 
                    case DropResult.Timeout: 
                    case DropResult.Reversed: 
                        break; 
                    default: 
                        if (recResult != null && 
                            (recResult.BlankType == BlankType.Bad || recResult.BlankType == BlankType.BadMode)) 
                        { 
                            AddVotingResult(recResult); 
                        } 
                        break; 
                } 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.RecognizerEndBulletinRecognitionError, ex); 
            } 
            finally 
            { 
                try 
                { 
                    if (_ocr != null) 
                        _ocr.EnableLogging(null); 
                } 
                catch (Exception ex) 
                { 
                    Logger.LogWarning(Message.RecognizerStopOcrLoggingError, ex); 
                } 
                Logger.LogVerbose(Message.Common_DebugReturn); 
                if (_config.GCCollect.Enabled) 
                { 
                    GC.Collect(0, GCCollectionMode.Forced); 
                } 
            } 
        } 
        private RecognitionResult RecognitionResultAnalisys(int recResultCode) 
        { 
            PollResult pollRes;                         // результат распознавания 
            int[][] sectionsMarks = null;               // массив отметок по всем секциям бланка 
            bool[] sectionsValidity = null;             // признаки корректности секций бюллетеня 
            var recLogAddInfoSb = new StringBuilder();  // доп. информация для записи в файл журнала распознавания 
            var stampResult = StampResult.YES;          // результат распознавания печати 
            var stampNumber = "";                       // Наиболее вероятный номер печати (полутон) 
            var stampAlternatives = new string[4];      // Альтернативы цифр номера печати по позициям (полутон) 


            if (recResultCode >= 0) 
            { 
                #region формирование массива меток 
                sectionsMarks = new int[_ocr.Results.Count][]; 
                sectionsValidity = new bool[_ocr.Results.Count]; 
                for (var sectionIndex = 0; sectionIndex < _ocr.Results.Count; sectionIndex++) 
                { 
                    var electionNum = _electionManager.SourceData.Blanks[_ocr.BulletinNumber].Sections[sectionIndex]; 
                    var currentElection = _electionManager.SourceData.GetElectionByNum(electionNum); 
                    sectionsValidity[sectionIndex] = false; 
                    pollRes = _ocr.Results[sectionIndex]; 
                    var currentMarks = new int[pollRes.Count]; 
                    var activeMarksCount = pollRes.Count; 
                    var noneAboveActive = false; 
                    for (var squareIndex = 0; squareIndex < pollRes.Count; squareIndex++) 
                    { 
                        currentMarks[squareIndex] = pollRes[squareIndex]; 
                        var currentCandidate = currentElection.Candidates[currentMarks[squareIndex]]; 
                        if (currentCandidate.Disabled) 
                        { 
                            activeMarksCount--; 
                        } 
                        else 
                        { 
                            if (currentCandidate.NoneAbove) 
                            { 
                                noneAboveActive = true; 
                            } 
                        } 
                    } 
                    sectionsMarks[sectionIndex] = currentMarks; 
                    sectionsValidity[sectionIndex] = 
                        activeMarksCount > 0 && activeMarksCount <= currentElection.MaxMarks; 
                    if (noneAboveActive) 
                        sectionsValidity[sectionIndex] = (activeMarksCount == 1); 
                    recLogAddInfoSb.AppendFormat("E{0},", sectionIndex); 
                    foreach (var squareNum in sectionsMarks[sectionIndex]) 
                    { 
                        recLogAddInfoSb.Append(squareNum); 
                        recLogAddInfoSb.Append(','); 
                    } 
                } 
                #endregion 
                #region вычисление результата распознавания печати 
                if (_config.Ocr.Stamp.TestLevel != StampTestLevel.None) 
                { 
                    if (_config.Ocr.Stamp.TestLevel == StampTestLevel.Halftone) 
                    { 
                        stampResult = Ocr.Ocr.IsStampOKGray(ref stampNumber, ref stampAlternatives); 
                        Logger.LogVerbose(Message.RecognizerHalftoneStamp, stampResult, stampNumber); 
                    } 
                    else 
                    { 
                        stampNumber = "0"; 
                        stampResult = _ocr.StampResult; 
                        Logger.LogVerbose(Message.RecognizerBinaryStamp, stampResult); 
                    } 
                } 
                #endregion 
            } 
            var recResult = CreateRecognitionResult(recResultCode, sectionsMarks, sectionsValidity, 
                stampResult, stampNumber, stampAlternatives); 
            LastRecognitionResult = recResult; 
            WriteRecognitionResultToLog(recResult, stampResult, stampNumber, recLogAddInfoSb.ToString()); 
            if (recResult.BlankType != BlankType.Bad && recResult.BlankType != BlankType.BadMode) 
            { 
                AddVotingResult(recResult); 
            } 
            else 
            { 
                var votingResult = new VotingResult( 
                    recResult.BlankType, 
                    recResult.BulletinNumber, 
                    recResult.StampNumber, 
                    recResult.BadBulletinReason.ToString(), 
                    recResult.BadStampReason, 
                    recResult.Marks, 
                    recResult.SectionsValidity); 
                _votingResultManager.SetLastVotingResult(votingResult); 
            } 
            SaveLastImage(recResult.ImageFilePrefix, recResult.ImageSavingType, null); 
            return recResult; 
        } 
        private void AddVotingResult(RecognitionResult recResult) 
        { 
            try 
            { 
                var votingResult = new VotingResult( 
                    recResult.BlankType, 
                    recResult.BulletinNumber, 
                    recResult.StampNumber, 
                    recResult.BadBulletinReason.ToString(), 
                    recResult.BadStampReason, 
                    recResult.Marks, 
                    recResult.SectionsValidity); 
                _votingResultManager.AddVotingResult( 
                    votingResult, 
                    _recognitionStartVotingMode, 
                    _scannerManager.IntSerialNumber); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.RecognizerAddVotingResultError, ex); 
            } 
        } 
        private void WriteRecognitionResultToLog( 
            RecognitionResult recResult, StampResult stampResult, string stampNumber, string additionalInfo) 
        { 
            try 
            { 
                var recLogSb = new StringBuilder(); 
                recLogSb.Append(_recognitionStartVotingMode); 
                recLogSb.Append(','); 
                recLogSb.Append(GetImageFileName(recResult.ImageFilePrefix)); 
                recLogSb.Append(','); 
                recLogSb.Append(_ocr.BulletinNumber); 
                recLogSb.Append(','); 
                recLogSb.Append(_scannerManager.SerialNumber); 
                recLogSb.Append(','); 
                recLogSb.Append(recResult.IntResultCode); 
                recLogSb.Append(','); 
                recLogSb.Append(stampNumber); 
                recLogSb.Append(','); 
                recLogSb.Append(stampResult); 
                recLogSb.Append(','); 
                recLogSb.Append(additionalInfo); 
                if (!_scannerManager.SheetProcessingSession.Closed) 
                { 
                    recLogSb.Append('\t'); 
                    recLogSb.Append(_scannerManager.SheetProcessingSession.Id); 
                } 
                _recognitionResultLogger.LogInfo(Message.RecognizerLog, recLogSb.ToString()); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.RecognizerLogError, ex); 
            } 
        } 
        private RecognitionResult CreateRecognitionResult( 
            int recResultCode, 
            int[][] marks, 
            bool[] sectionsValidity, 
            StampResult stampResult, 
            string stampNumber, 
            string[] stampNumberAlts) 
        { 
            var recResult = new RecognitionResult(recResultCode) 
            { 
                StampResult = stampResult, 
                StampNumber = stampNumber, 
                StampNumberAlts = stampNumberAlts, 
                BulletinNumber = recResultCode != (int)OcrRecognitionResult.MARK ? _ocr.BulletinNumber : -1, 
                Marks = marks, 
                SectionsValidity = sectionsValidity 
            }; 
            FindStampNumber(recResult); 
            CreateStampInfo(recResult); 
            if (recResult.ResultCode == OcrRecognitionResult.OK && recResult.StampOk) 
            { 
                if (recResult.BulletinNumber < 0 || 
                    recResult.BulletinNumber >= _electionManager.SourceData.Blanks.Length) 
                { 
                    recResult.BlankType = BlankType.Bad; 
                    recResult.ResultDescription = "Недопустимый номер бюллетеня " + recResult.BulletinNumber; 
                } 
                else 
                { 
                    var blank = _electionManager.SourceData.Blanks[recResult.BulletinNumber]; 
                    if (_electionManager.SourceData.IsVotingModeValidForBlank(blank, _recognitionStartVotingMode)) 
                    { 
                        Logger.LogVerbose(Message.RecognizerBulletinRecognized); 
                        for (int i = 0; i < recResult.Marks.Length; i++) 
                        { 
                            var curSectionMarks = recResult.Marks[i]; 
                            if (recResult.SectionsValidity[i]) 
                            { 
                                recResult.BlankType = BlankType.Valid; 
                                break; 
                            } 
                            if (curSectionMarks.Length > 0) 
                                recResult.BulletinWithExtraLabels = true; 
                            else 
                                recResult.BulletinWithoutLabels = true; 
                        } 
                        if (recResult.BlankType != BlankType.Valid) 
                        { 
                            if (recResult.BulletinWithExtraLabels) 
                                recResult.BlankType = BlankType.TooManyMarks; 
                            if (recResult.BulletinWithoutLabels) 
                                recResult.BlankType = BlankType.NoMarks; 
                        } 
                        recResult.ResultDescription = string.Format( 
                            "Бюллетень N {0} {1}. Печать: {2}. {3}. Время: {4}", 
                            recResult.BlankTypeDescription, 
                            blank.Marker, 
                            recResult.StampDescription, 
                            GetSquareDescription(recResultCode, recResult.Marks), 
                            DateTime.Now.ToString("HH:mm:ss.fff")); 
                        Logger.LogVerbose(Message.RecognizerBulletinResult, 
                            recResult.BlankTypeDescription, 
                            _recognitionStartVotingMode); 
                    } 
                    else 
                    { 
                        recResult.BlankType = BlankType.BadMode; 
                        recResult.ResultDescription = "Бюллетень не имеет текущего режима голосования"; 
                        Logger.LogVerbose(Message.RecognizerWrongModeForBulletin); 
                    } 
                } 
            } 
            else 
            { 
                recResult.BlankType = BlankType.Bad; 
                CreateBadBulletinInfo(recResult); 
                recResult.ResultDescription = string.Format("Бюллетень неустановленной формы ({0}).", 
                    recResult.BadBulletinDescription); 
                Logger.LogVerbose(Message.RecognizerNuf, recResult.ResultDescription); 
            } 
            var blankConfig = _config.Blanks.Get(recResult.BlankType, _sheetType); 
            if (recResult.BlankType == BlankType.Bad && 
                recResult.BadBulletinReason == BadBulletinReason.Stamp && 
                _config.Blanks.Get(BlankType.BadStamp) != null) 
            { 
                blankConfig = _config.Blanks.Get(BlankType.BadStamp); 
            } 
            var imageFilePrefixStringBuilder = new StringBuilder(); 
            imageFilePrefixStringBuilder.Append(blankConfig.ImageFilePrefix); 
            if (!string.IsNullOrEmpty(recResult.BadBulletinFilePrefix)) 
            { 
                imageFilePrefixStringBuilder.Append('-'); 
                imageFilePrefixStringBuilder.Append(recResult.BadBulletinFilePrefix); 
            } 
            if (_config.Ocr.Stamp.TestLevel == StampTestLevel.Halftone) 
            { 
                imageFilePrefixStringBuilder.Append('-'); 
                imageFilePrefixStringBuilder.Append(recResult.StampNumber); 
            } 
            recResult.ImageFilePrefix = imageFilePrefixStringBuilder.ToString(); 
            recResult.Marking = blankConfig.Marking; 
            if (_recognitionStartVotingMode == VotingMode.Test) 
                blankConfig = _config.Blanks.Get(BlankType.Test); 
            recResult.ImageSavingType = blankConfig.ImageSavingType; 
            return recResult; 
        } 
        private void CreateStampInfo(RecognitionResult recResult) 
        { 
            if (_config.Ocr.Stamp.TestLevel == StampTestLevel.None || 
                recResult.ResultCode != OcrRecognitionResult.OK) 
            { 
                recResult.StampReasonCode = "not-checked"; 
                recResult.StampDescription = "не проверялась"; 
                recResult.StampShortDescription = recResult.StampDescription; 
                return; 
            } 
            switch (recResult.StampResult) 
            { 
                case StampResult.YES: 
                    switch (_config.Ocr.Stamp.TestLevel) 
                    { 
                        case StampTestLevel.Found: 
                            recResult.StampReasonCode = "found"; 
                            recResult.StampDescription = "присутствует"; 
                            break; 
                        case StampTestLevel.Halftone: 
                            if (recResult.StampOk) 
                            { 
                                recResult.StampReasonCode = "recognized"; 
                                recResult.StampDescription = "распознана"; 
                            } 
                            else 
                            { 
                                recResult.StampReasonCode = "not-match-uik-number"; 
                                recResult.StampDescription = "не соответствует номеру УИК"; 
                            } 
                            break; 
                        default: 
                            recResult.StampReasonCode = "valid"; 
                            recResult.StampDescription = "действительная"; 
                            break; 
                    } 
                    break; 
                case StampResult.EMPTY: 
                    recResult.StampReasonCode = "empty"; 
                    recResult.StampDescription = "отсутствует"; 
                    break; 
                case StampResult.FAINT: 
                    recResult.StampReasonCode = "faint"; 
                    recResult.StampDescription = "слишком бледная"; 
                    break; 
                case StampResult.BADPRINT: 
                    recResult.StampReasonCode = "bad-print"; 
                    recResult.StampDescription = "неверная"; 
                    break; 
                case StampResult.CALL_ERROR: 
                    recResult.StampReasonCode = "recognizing-error"; 
                    recResult.StampDescription = "ошибка при распознавании"; 
                    recResult.StampShortDescription = "ошибка распознавания"; 
                    break; 
                case StampResult.BADLINES: 
                    recResult.StampReasonCode = "bad-lines"; 
                    recResult.StampDescription = "не обнаружены необходимые линии"; 
                    recResult.StampShortDescription = "линии"; 
                    break; 
                default: 
                    recResult.StampReasonCode = "not-recognized"; 
                    recResult.StampDescription = "не распознана"; 
                    break; 
            } 
            if (recResult.StampShortDescription == null) 
                recResult.StampShortDescription = recResult.StampDescription; 
        } 
        private static void CreateBadBulletinInfo(RecognitionResult recResult) 
        { 
            if (recResult.ResultCode == OcrRecognitionResult.OK && 
                (recResult.StampResult != StampResult.YES || !recResult.StampOk)) 
            { 
                recResult.BadBulletinFilePrefix = "BAD-STAMP"; 
                recResult.BadBulletinReason = BadBulletinReason.Stamp; 
                recResult.BadStampReason = recResult.StampReasonCode; 
                recResult.BadBulletinDescription = "Печать - " + recResult.StampDescription; 
                recResult.BadBulletinShortDescription = "Печать - " + recResult.StampShortDescription; 
                return; 
            } 
            recResult.BadBulletinReason = BadBulletinReason.Marker; 
            switch (recResult.ResultCode) 
            { 
                case OcrRecognitionResult.ERROR: 
                    recResult.BadBulletinFilePrefix = "REC-ERROR"; 
                    recResult.BadBulletinDescription = "Ошибка при возврате из функции"; 
                    recResult.BadBulletinShortDescription = "Возврат из функции"; 
                    break; 
                case OcrRecognitionResult.NUF: 
                    recResult.BadBulletinFilePrefix = "NUF"; 
                    recResult.BadBulletinDescription = "Без детализации"; 
                    recResult.BadBulletinShortDescription = recResult.BadBulletinDescription; 
                    break; 
                case OcrRecognitionResult.BRK: 
                    recResult.BadBulletinFilePrefix = "BRK"; 
                    recResult.BadBulletinDescription = "Распознавание было прервано"; 
                    recResult.BadBulletinShortDescription = "распознавание прервано"; 
                    break; 
                case OcrRecognitionResult.MARK: 
                    recResult.BadBulletinFilePrefix = "MARK"; 
                    recResult.BadBulletinDescription = "Не удалось обнаружить или узнать маркер"; 
                    recResult.BadBulletinShortDescription = "Маркер"; 
                    break; 
                case OcrRecognitionResult.SKEW: 
                    recResult.BadBulletinFilePrefix = "SKEW"; 
                    recResult.BadBulletinReason = BadBulletinReason.Lines; 
                    recResult.BadBulletinDescription = "Не удалось локализовать требуемые линии"; 
                    recResult.BadBulletinShortDescription = "Линии"; 
                    break; 
                case OcrRecognitionResult.REFP: 
                    recResult.BadBulletinFilePrefix = "REFP"; 
                    recResult.BadBulletinReason = BadBulletinReason.Refp; 
                    recResult.BadBulletinDescription = "Не удалось локализовать опорные точки"; 
                    recResult.BadBulletinShortDescription = "Опорные точки"; 
                    break; 
                case OcrRecognitionResult.FSQR: 
                    recResult.BadBulletinFilePrefix = "FSQR"; 
                    recResult.BadBulletinReason = BadBulletinReason.Squares; 
                    recResult.BadBulletinDescription = "Не удалось локализовать квадраты"; 
                    recResult.BadBulletinShortDescription = "Квадраты"; 
                    break; 
                case OcrRecognitionResult.CLRTOP: 
                    recResult.BadBulletinFilePrefix = "CLRTOP"; 
                    recResult.BadBulletinDescription = "При удалении черного сверху съели почти все"; 
                    recResult.BadBulletinShortDescription = "Черное сверху"; 
                    break; 
                case OcrRecognitionResult.CLRBOT: 
                    recResult.BadBulletinFilePrefix = "CLRBOT"; 
                    recResult.BadBulletinDescription = "При удалении черного снизу съели почти все"; 
                    recResult.BadBulletinShortDescription = "Черное снизу"; 
                    break; 
                case OcrRecognitionResult.CALL: 
                    recResult.BadBulletinFilePrefix = "CALL"; 
                    recResult.BadBulletinDescription = "Ошибка вызова функции"; 
                    recResult.BadBulletinShortDescription = "Вызов функции"; 
                    break; 
                case OcrRecognitionResult.SCTLINE: 
                    recResult.BadBulletinFilePrefix = "SCTLINE"; 
                    recResult.BadBulletinReason = BadBulletinReason.Lines; 
                    recResult.BadBulletinDescription = "Не удалось обнаружить нижнюю линию секции"; 
                    recResult.BadBulletinShortDescription = "Нижняя линия"; 
                    break; 
                case OcrRecognitionResult.BULNUM: 
                    recResult.BadBulletinFilePrefix = "BULNUM"; 
                    recResult.BadBulletinDescription = "Недопустимый номер бюллетеня"; 
                    recResult.BadBulletinShortDescription = "Номер бюллетеня"; 
                    break; 
                default: 
                    recResult.BadBulletinFilePrefix = "UNKNOWN"; 
                    recResult.BadBulletinDescription = "Описание неизвестно"; 
                    recResult.BadBulletinShortDescription = recResult.BadBulletinDescription; 
                    break; 
            } 
        } 
        public string GetSquareDescription(int recResultCode, int[][] marks) 
        { 
            var sb = new StringBuilder(); 
            if (recResultCode > 0) 
            { 
                for (int i = 0; i < marks.Length; i++) 
                { 
                    if (marks.Length > 1) 
                        sb.AppendFormat("\nСекция N {0}. ", (i + 1)); 
                    if (marks[i] != null) 
                    { 
                        if (marks[i].Length == 0) 
                            sb.Append("\n   Нет отметок"); 
                        else if (marks[i].Length > 1) 
                        { 
                            sb.Append("\n   Отметки в квадратах: "); 
                        } 
                        else if (marks[i].Length == 1) 
                        { 
                            sb.Append("\n   Отметка в квадрате "); 
                        } 
                        for (int j = 0; j < marks[i].Length; j++) 
                        { 
                            if (j > 0) 
                                sb.Append(", "); 
                            sb.Append(marks[i][j] + 1); 
                        } 
                        sb.Append(". "); 
                    } 
                } 
                if (sb.Length > 0) 
                    sb.Length -= 1; 
            } 
            var res = sb.ToString(); 
            return res; 
        } 
        private void FindStampNumber(RecognitionResult recResult) 
        { 
            if (_config.Ocr.Stamp.TestLevel == StampTestLevel.None) 
            { 
                recResult.StampOk = true; 
                return; 
            } 
            if (recResult.ResultCode < 0) 
            { 
                recResult.StampOk = false; 
                return; 
            } 
            if (_config.Ocr.Stamp.TestLevel != StampTestLevel.Halftone && recResult.StampResult == StampResult.YES) 
            { 
                recResult.StampOk = true; 
                return; 
            } 
            Logger.LogVerbose(Message.RecognizerStampNumber, recResult.StampNumber, 
                recResult.StampNumberAlts[0], recResult.StampNumberAlts[1], 
                recResult.StampNumberAlts[2], recResult.StampNumberAlts[3]); 
            if (recResult.StampResult != StampResult.YES && recResult.StampResult != StampResult.FAINT) 
            { 
                recResult.StampOk = false; 
                return; 
            } 
            int iStampNumber; 
            if (!int.TryParse(recResult.StampNumber, out iStampNumber)) 
            { 
                recResult.StampOk = false; 
                return; 
            } 
            if (iStampNumber == _electionManager.SourceData.Uik) 
            { 
                recResult.StampOk = true; 
                return; 
            } 
            var uikStr = _electionManager.SourceData.Uik.ToString("0000"); 
            if (СheckAlternatives(uikStr, recResult.StampNumberAlts)) 
            { 
                recResult.StampNumber = uikStr; 
                recResult.StampOk = true; 
                return; 
            } 
            if (_electionManager.SourceData == null || !AllowSuperiorStamp) 
            { 
                recResult.StampOk = false; 
                return; 
            } 
            var blank = _electionManager.SourceData.Blanks[recResult.BulletinNumber]; 
            for (var i = 0; i < blank.Sections.Length; i++) 
            { 
                var electionNum = blank.Sections[i]; 
                var election = _electionManager.SourceData.GetElectionByNum(electionNum); 
                if (election.StampCommittees. 
                    Any(stampCommittee => stampCommittee.Num > 0 && iStampNumber == stampCommittee.Num)) 
                { 
                    recResult.StampOk = true; 
                    return; 
                } 
            } 
            recResult.StampOk = false; 
        } 
        private bool СheckAlternatives(string stampNumber, string[] stampAlterantives) 
        { 
            Logger.LogVerbose(Message.RecognizerCheckAlternatives, stampNumber); 
            for (int i = 0; i < Ocr.Ocr.STAMP_DIGIT_COUNT; i++) 
            { 
                int altCounter = i; 
                bool match = stampAlterantives[i].Any(altDigit => altDigit == stampNumber[altCounter]); 
                if (!match) 
                    return false; 
            } 
            return true; 
        } 
        private bool ReserveSpaceForSaveLastBuffer(ImageSavingType savingType) 
        { 
            try 
            { 
                long requiredSize; 
                switch (savingType) 
                { 
                    case ImageSavingType.Binary: 
                        requiredSize = _scannerManager.GetBufferSize(ImageType.Binary, BufferSize.Scanned); 
                        break; 
                    case ImageSavingType.Halftone: 
                        requiredSize = _scannerManager.GetBufferSize(ImageType.Halftone, BufferSize.Scanned); 
                        break; 
                    case ImageSavingType.All: 
                        requiredSize = _scannerManager.GetBufferSize(ImageType.Binary, BufferSize.Scanned) + 
                            _scannerManager.GetBufferSize(ImageType.Halftone, BufferSize.Scanned); 
                        break; 
                    default: 
                        throw new Exception("Неизвестный тип сохранения изображения"); 
                } 
                if (!ReserveSpaceForImage(_config.MinFreeSpaceForImageKb.Value, requiredSize)) 
                    return false; 
            } 
            catch (Exception ex) 
            { 
                Logger.LogWarning(Message.RecognizerSaveImageError, ex); 
                _scannerManager.RestoreScanningAfterError(); 
                return false; 
            } 
            return true; 
        } 
        private bool ReserveSpaceForImage(long requiredSpace) 
        { 
            int minSize = (int)(requiredSpace / FileUtils.BYTES_IN_KB) + 1; 
            return ReserveSpaceForImage(minSize, requiredSpace); 
        } 
        private bool ReserveSpaceForImage(int minSpace, long requiredSpace) 
        { 
            requiredSpace = requiredSpace / FileUtils.BYTES_IN_KB + 1; 
            if (requiredSpace > minSpace) 
                minSpace = (int)requiredSpace; 
            long availableSize; 
            var imageDirectoryPath = _fileSystemManager.GetDataDirectoryPath(FileType.ScanningImage); 
            if (_fileSystemManager.ReserveDiskSpace( 
                imageDirectoryPath, (int)requiredSpace, minSpace, out availableSize)) 
                return true; 
            Logger.LogWarning( 
                Message.RecognizerSaveImageNotEnoughFreeSpace, imageDirectoryPath, availableSize, requiredSpace); 
            return false; 
        } 
        private void SaveLastImage(string filePrefix, ImageSavingType savingType, int? errorCode) 
        { 
            Logger.LogVerbose(Message.RecognizerSaveImageCall, filePrefix, savingType); 
            if (savingType == ImageSavingType.None) 
                return; 
            try 
            { 
                if (!ReserveSpaceForSaveLastBuffer(savingType)) 
                    return; 
                Logger.LogVerbose(Message.RecognizerSavingImage); 
                var savingStartTime = DateTime.Now; 
                var filePathSb = GetImageFileName(filePrefix); 
                var imageDirectoryPath = _fileSystemManager.GetDataDirectoryPath(FileType.ScanningImage); 
                filePathSb.Insert(0, imageDirectoryPath + '/'); 
                var filePath = filePathSb.ToString(); 
                if (errorCode != null) 
                    filePathSb.Append("_E" + errorCode); 
                if (savingType == ImageSavingType.Binary || savingType == ImageSavingType.All) 
                { 
                    _scannerManager.SaveBuffer( 
                        filePath + "_B", ImageType.Binary, ScannedSide.Bottom, BufferSize.Scanned); 
                } 
                if (savingType == ImageSavingType.Halftone || savingType == ImageSavingType.All) 
                { 
                    _scannerManager.SaveBuffer( 
                        filePath + "_H", ImageType.Halftone, ScannedSide.Bottom, BufferSize.Scanned); 
                    SystemHelper.SyncFileSystem(); 
                } 
                Logger.LogVerbose( 
                    Message.RecognizerSaveImageTiming, (DateTime.Now - savingStartTime).TotalMilliseconds); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogWarning(Message.RecognizerSaveImageError, ex); 
                _scannerManager.RestoreScanningAfterError(); 
            } 
            finally 
            { 
                Logger.LogVerbose(Message.Common_DebugReturn); 
            } 
        } 
        public void SaveLastImageOnDriverError(int errorCode) 
        { 
            SaveLastImage("REVERSE_DRV", ImageSavingType.Binary, errorCode); 
        } 
        private string GetRecErrorFilePath(string scannerSerialNumber) 
        { 
            return string.Format(_recErrorFilePathFormat, DateTime.Now, _ocr.RunRecCount, scannerSerialNumber); 
        } 
        private StringBuilder GetImageFileName(string filePrefix) 
        { 
            Logger.LogVerbose(Message.Common_DebugCall); 
            var fileName = new StringBuilder(); 
            try 
            { 
                fileName.AppendFormat(_imageFileNamePrefixFormat, 
                    DateTime.Now, _ocr.RunRecCount, _scannerManager.SerialNumber); 
                if (_ocr.BulletinNumber >= 0) 
                { 
                    if (_ocr.BulletinNumber < _electionManager.SourceData.Blanks.Length) 
                    { 
                        fileName.Append("_MarkerN"); 
                        fileName.Append(_electionManager.SourceData.Blanks[_ocr.BulletinNumber].Marker); 
                    } 
                    else 
                    { 
                        fileName.Append("_BulletinN"); 
                        fileName.Append(_ocr.BulletinNumber); 
                    } 
                } 
                fileName.Append('_'); 
                fileName.Append(filePrefix); 
                if (_recognitionStartVotingMode == VotingMode.Test) 
                { 
                    fileName.Append('_'); 
                    fileName.Append(_config.Blanks.Get(BlankType.Test).ImageFilePrefix); 
                } 
                fileName.Append('_'); 
                fileName.Append(_scannerManager.BinarizationThresholdTop); 
                fileName.Append('_'); 
                fileName.Append(_scannerManager.BinarizationThresholdBottom); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogWarning(Message.RecognizerGetImageFileNameError, ex); 
            } 
            return fileName; 
        } 
        #endregion 
        #endregion 
        #region IOcrEventHandler Members 
        public int GetHalfToneBuffer(IOcr ocr, short side, int x, int y, int height, int width, MemoryBlock image) 
        { 
            if (!_config.Ocr.GrayAnalysis.Enabled) 
                return -1; 
            try 
            { 
                var res = _scannerManager.GetHalftoneBuffer( 
                    (ScannedSide)side, (short)x, (short)y, (short)width, (short)height, image); 
                if (res && _config.DebugImageSaving.Squares) 
                { 
                    var imageDirectoryPath = _fileSystemManager.GetDataDirectoryPath(FileType.ScanningImage); 
                    long requiredSize = width * height; 
                    if (ReserveSpaceForImage(requiredSize)) 
                    { 
                        var filePathSb = GetImageFileName("SQ"); 
                        filePathSb.Insert(0, imageDirectoryPath + '/'); 
                        filePathSb.AppendFormat("_S{0}_X{1}_Y{2}_W{3}_H{4}.tif", side, x, y, width, height); 
                        TiffImageHelper.SaveToFile(filePathSb.ToString(), ImageType.Halftone, image, width, height); 
                    } 
                } 
                return res ? width * height : -1; 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.RecognizerGetBufferError, ex); 
                return -1; 
            } 
        } 
        public int GetBinaryThreshold(IOcr ocr, short side) 
        { 
            int res = side == 0 
                ? _scannerManager.BinarizationThresholdTop 
                : _scannerManager.BinarizationThresholdBottom; 
            Logger.LogVerbose(Message.RecognizerBinarizationThreshold, side, res); 
            return res; 
        } 
        public void Error(IOcr ocr, int errorCode, string message) 
        { 
            Logger.LogError(Message.RecognizerOcrError, errorCode, message); 
        } 
        public void AppendToLog(IOcr ocr, string message) 
        { 
            Logger.LogVerbose(Message.RecognizerOcrDebug, message); 
        } 
        #endregion 
        #region IDisposable Members 
        public override void Dispose() 
        { 
            base.Dispose(); 
            if (_ocr != null) 
            { 
                _ocr.Dispose(); 
                _ocr = null; 
            } 
            if (_recognitionResultLogger != null) 
                Disposer.DisposeObject(_recognitionResultLogger); 
        } 
        #endregion 
    } 
}

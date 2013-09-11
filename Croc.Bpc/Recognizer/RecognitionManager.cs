using System; 

using System.Diagnostics; 

using System.IO; 

using System.Text; 

using System.Text.RegularExpressions; 

using Croc.Bpc.Common; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Bpc.Common.Images; 

using Croc.Bpc.Election; 

using Croc.Bpc.Election.Voting; 

using Croc.Bpc.FileSystem; 

using Croc.Bpc.Recognizer.Config; 

using Croc.Bpc.Recognizer.Ocr; 

using Croc.Bpc.Scanner; 

using Croc.Core; 

using Croc.Core.Configuration; 

using Croc.Core.Diagnostics; 

using Croc.Bpc.Common.Images.Tiff; 

 

 

namespace Croc.Bpc.Recognizer 

{ 

    /// <summary> 

    /// Менеджер распознавания 

    /// </summary> 

    [SubsystemConfigurationElementTypeAttribute(typeof(RecognitionManagerConfig))] 

    public class RecognitionManager : 

        Subsystem, 

        IOcrEventHandler, 

        IRecognitionManager 

    { 

        /// <summary> 

        /// Путь к бинарному файлу модели 

        /// </summary> 

        private const string MODEL_FILE_NAME = "MODEL.DAT"; 

        /// <summary> 

        /// Формат-строка для получения пути к текстовому файлу модели 

        /// </summary> 

        private const string MODEL_TEXT_FILE_NAME_FORMAT = "model.{0}.txt"; 

        /// <summary> 

        /// Путь к файлу протокола распознавания 

        /// </summary> 

        private const string OCR_LOG_NAME = "ocr.txt"; 

        /// <summary> 

        /// Дата и время начала работы 

        /// </summary> 

        private static DateTime s_workStartDate = DateTime.Now; 

        /// <summary> 

        /// Конфигурация подсистемы 

        /// </summary> 


        private RecognitionManagerConfig _config; 

        /// <summary> 

        /// Менеджер выборов 

        /// </summary> 

        private IElectionManager _electionManager; 

        /// <summary> 

        /// Менеджер сканера 

        /// </summary> 

        private IScannerManager _scannerManager; 

        /// <summary> 

        /// Менеджер файловой системы 

        /// </summary> 

        private IFileSystemManager _fileSystemManager; 

        /// <summary> 

        /// Распознавалка 

        /// </summary> 

        private IOcr _ocr; 

        /// <summary> 

        /// Путь к текстовому файлу моделью 

        /// </summary> 

        private string _modelTextFilePath; 

        /// <summary> 

        /// Формат-строка для получения пути к файлу, в который будем записывать информацию об ошибке распознавания 

        /// </summary> 

        private string _recErrorFilePathFormat; 

        /// <summary> 

        /// Формат-строка для получения префикса имени файла для сохранения изображений 

        /// </summary> 

        private string _imageFileNamePrefixFormat; 

        /// <summary> 

        /// Логгер для протоколирования процесса распознавания 

        /// </summary> 

        private ILogger _scanningLogger; 

        /// <summary> 

        /// Логгер для протоколирования результатов распознавания 

        /// </summary> 

        private ILogger _rawRecognitionResultLogger; 

		/// <summary> 

		/// Путь к логу окр 

		/// </summary> 

		private string OcrLogFilePath 

		{ 

			get 

			{ 

				return Path.Combine( 

                    _fileSystemManager.GetDataDirectoryPath(FileType.Log), 

                    OCR_LOG_NAME); 

			} 

		} 

 


 
        /// <summary> 

        /// Разрешено ли использовать печати вышестоящих комиссий 

        /// </summary> 

        private bool AllowSuperiorStamp 

        { 

            get 

            { 

                // разрешено, если текущий режим голосования - тестовый или досрочный  

                // или в настройках выставлен признак допустимости 

                return  

                    _electionManager.CurrentVotingMode == VotingMode.Test || 

                    _config.SuperiorStamp.Enabled; 

            } 

        } 

 

 

        #region Инициализация подсистемы 

 

 

        /// <summary> 

        /// Инициализация подсистемы 

        /// </summary> 

        /// <param name="config"></param> 

        public override void Init(SubsystemConfig config) 

        { 

            _config = (RecognitionManagerConfig)config; 

 

 

            // получаем ссылки на другие подсистемы 

            _electionManager = Application.GetSubsystemOrThrow<IElectionManager>(); 

            _scannerManager = Application.GetSubsystemOrThrow<IScannerManager>(); 

            _fileSystemManager = Application.GetSubsystemOrThrow<IFileSystemManager>(); 

 

 

            // создаем распознавалку 

            CreateOcr(); 

 

 

            // формируем пути к файлам 

            GenerateFilePaths(); 

 

 

            // создадим отдельный логгер для протоколирования процесса распознавания 

            _scanningLogger = Application.CreateLogger("Scanning", TraceLevel.Info); 

 

 

            // создадим отдельный логгер для протоколирования процесса распознавания 

            _rawRecognitionResultLogger = Application.CreateLogger("RecognizerLog", TraceLevel.Info); 

 


 
            // TODO: подписаться на события изменения состояния?... когда входят в состояние, 

            // в котором доступно сканирование, то переинициализируют все параметры распознавалки и 

            // задают режим подтверждения принятия бюллетеня 

 

 

            // по умолчанию, режим - распознавание бюллетеней 

            RecognitionMode = RecognitionMode.BulletinRecognition; 

        } 

 

 

        /// <summary> 

        /// Применение нового конфига 

        /// </summary> 

        /// <param name="newConfig"></param> 

        public override void ApplyNewConfig(SubsystemConfig newConfig) 

        { 

            _config = (RecognitionManagerConfig)newConfig; 

            InitRecognition(); 

        } 

 

 

        /// <summary> 

        /// Создание и первичная инициализация распознавалки 

        /// </summary> 

        private void CreateOcr() 

        { 

            // создаем распознавалку 

            _ocr = new Ocr.Ocr(); 

 

 

            _ocr.SetEventsHandler(this); 

            _ocr.StampTestLevel = _config.Ocr.Stamp.TestLevel; 

            _ocr.ModelFilePath = Path.Combine( 

                _fileSystemManager.GetDataDirectoryPath(FileType.RuntimeData),  

                MODEL_FILE_NAME); 

            _ocr.Init(); 

        } 

 

 

        /// <summary> 

        /// Формирование путей к файлам 

        /// </summary> 

        private void GenerateFilePaths() 

        { 

            var runtimeDataDirPath = _fileSystemManager.GetDataDirectoryPath(FileType.RuntimeData); 

 

 

            // путь к текстовому файлу модели 

            _modelTextFilePath = string.Format(MODEL_TEXT_FILE_NAME_FORMAT, _scannerManager.SerialNumber); 


            _modelTextFilePath = Path.Combine(runtimeDataDirPath, _modelTextFilePath); 

 

 

            // формат-строка пути файла для записи информации об ошибке распознавания, содержит: 

            // [путь к папке с данными][_параметр0:текущее время_][_параметр1_][_параметр2_]REC_ERROR.txt 

            // где временные параметры получаются из времени запуска программы 

            var sb = new StringBuilder(); 

            sb.Append(runtimeDataDirPath); 

            sb.Append("{0:ddMM_HHmm}_{1}_{2}_REC_ERROR.txt"); 

            _recErrorFilePathFormat = sb.ToString(); 

 

 

            // формат-строка префикса имени файла для сохранения изображений, содержит: 

            // [_параметр0:текущее время_][_параметр1_][_параметр2_], 

            // где временные параметры получаются из времени запуска программы 

            // TODO: подумать над заменой времени на 15-мин интервалы (а надо ли??) 

            sb.Length = 0; 

            sb.Append("{0:ddMMHHmm}_{1}_{2}_"); 

            _imageFileNamePrefixFormat = sb.ToString(); 

        } 

 

 

        #endregion 

 

 

        #region Инициализация распознавалки 

 

 

        /// <summary> 

        /// Объект синхронизации инициализации распознавалки 

        /// </summary> 

        private static object s_initRecognitionSync = new object(); 

 

 

        /// <summary> 

        /// (Пере)Инициализация распознавалки 

        /// </summary> 

        public void InitRecognition() 

        { 

            Logger.LogVerbose(Message.DebugVerbose, "call"); 

 

 

            lock (s_initRecognitionSync) 

            { 

                if (_ocr == null) 

                    throw new Exception("Модуль распознавания не создан"); 

 

 

                // если ИД еще не загружены 

                if (_electionManager.SourceData == null) 


                    // то нет смысла инициализировать распознавалку 

                    return; 

 

 

                // создадим модель 

                Model model = null; 

                try 

                { 

                    model = new Model(_config.Ocr.Marker.Type); 

                    model.Create(_electionManager.SourceData); 

                    Logger.LogVerbose(Message.DebugVerbose, "Модель создана"); 

                } 

                catch (Exception ex) 

                { 

                    throw new Exception("Ошибка создания модели", ex); 

                }                

 

 

                // очистим номер печати 

                _ocr.ClearStamps(); 

 

 

                // если режим с бинарным распознаванием печати 

                if (_config.Ocr.Stamp.TestLevel != StampTestLevel.Halftone && 

                    _config.Ocr.Stamp.TestLevel != StampTestLevel.None) 

                { 

                    // загружаем в распознавалку номер УИК 

                    _ocr.AddStamp(_electionManager.UIK); 

 

 

                    // если разрешено использовать печати вышестоящих комиссий и ИД загружены 

                    if (AllowSuperiorStamp && _electionManager.SourceData != null) 

                    { 

                        // по всем выборам 

                        foreach (var election in _electionManager.SourceData.Elections) 

                            // по всем номерам 

                            foreach (var stampCommittee in election.StampCommittees) 

                            { 

                                // если номер корректен, то добавлю 

                                if (stampCommittee.Num > 0) 

                                { 

                                    try 

                                    { 

                                        _ocr.AddStamp(stampCommittee.Num); 

                                    } 

                                    catch (Exception ex) 

                                    { 

                                        Logger.LogException(Message.Exception, ex,  

                                            "Ошибка при добавлении номера печати вышестоящей комиссии"); 

                                    } 


                                } 

                            } 

                    } 

                } 

 

 

                // инициализируем распознавалку 

                _ocr.InitRecognize(); 

 

 

                // загрузим параметры в распознавалку 

                LoadOcrParameters(); 

#if DEBUG 

                // сохраняем в отладочных целях файл модели 

                model.SaveAsText(_modelTextFilePath); 

#endif 

                // обновим информацию о снятых кандидатах 

                RefreshCanceledCandidatesInfo(); 

 

 

                // сбросим последний результат распознавания 

                _electionManager.ResetLastVotingResult(); 

            } 

        } 

 

 

        /// <summary> 

        /// Загружает параметры в распознавалку 

        /// </summary> 

        private void LoadOcrParameters() 

        { 

            // в случае полутонового распознавания отключаем старый вариант распознавания, 

            // так как будет вызываться совсем другая функция 

            _ocr.StampTestLevel = _config.Ocr.Stamp.TestLevel == StampTestLevel.Halftone 

                ? StampTestLevel.None : _config.Ocr.Stamp.TestLevel; 

 

 

            _ocr.InlineRecognitionLevel = _config.Ocr.InlineRecognize.Level; 

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

            _ocr.MaxMarkerHgh = _config.Ocr.Marker.Digital.Width.Max; 

            _ocr.MinMarkerRio = _config.Ocr.Marker.Digital.Rio.Min; 

            _ocr.MaxMarkerRio = _config.Ocr.Marker.Digital.Rio.Max; 

 

 

            _ocr.BlankTestStart = _config.Ocr.InlineRecognize.StartAtLine; 

            _ocr.BlankTestStop = _config.Ocr.InlineRecognize.StopAtLine; 

 

 

            _ocr.MinCheckArea = _config.Ocr.MinCheckArea.Value; 

            _ocr.SeekBottomRightLine = _config.Ocr.SeekBottomRightLine.Enabled; 

            _ocr.MaxOnlineSkew = _config.Ocr.MaxOnlineSkew.Value; 

 

 

            _ocr.MinStandartMarkerWid = _config.Ocr.Marker.Standard.Width.Min; 

            _ocr.MaxStandartMarkerWid = _config.Ocr.Marker.Standard.Width.Max; 

            _ocr.MinStandartMarkerHgh = _config.Ocr.Marker.Standard.Height.Min; 

            _ocr.MaxStandartMarkerHgh = _config.Ocr.Marker.Standard.Height.Max; 

            _ocr.StandartMarkerZone = _config.Ocr.Marker.Standard.Zone.Value; 

 

 

            _ocr.StampLowThr = _config.Ocr.Stamp.LowThr; 

 

 

            // передаем признак необходимости пропускать мертвую зону при анализе загрязнения (0 - пропускать, -1 - не пропускать) 

            _ocr.OffsetFirstRule = _scannerManager.Version == ScannerVersion.V2003 ? 0 : -1; 

        } 

 

 

        /// <summary> 

        /// Обновляет информацию о снятых кандидатах 

        /// </summary> 

        private void RefreshCanceledCandidatesInfo() 

        { 

            // если ИД не загружены 

            if (_electionManager.SourceData == null) 

                return; 

 

 

            // пройдусь по всем бланкам 

            for (int nBlankIndex = 0; nBlankIndex < _electionManager.SourceData.Blanks.Length; nBlankIndex++) 

            { 

                var oCurBlank = _electionManager.SourceData.Blanks[nBlankIndex]; 


 
 

                // пройдусь по всем выборам на бланке 

                for (int i = 0; i < oCurBlank.Sections.Length; i++) 

                { 

                    var oCurEl = _electionManager.SourceData.GetElectionByNum(oCurBlank.Sections[i]); 

 

 

                    // пройдусь по всем кандидатам 

                    foreach (var oCurCand in oCurEl.Candidates) 

                    { 

                        // если это "против всех" 

                        if (oCurCand.NoneAbove) 

                            continue; 

 

 

                        // Нумерация квадратов - с нуля 

                        var square = oCurCand.Number - 1; 

 

 

                        // получу состояние квадрата в распознавалке 

                        int nRes; 

                        _ocr.OCR_IsSquareValid(nBlankIndex, i, square, out nRes); 

 

 

                        // если кандидат снят, а квадрат используется 

                        if (oCurCand.Disabled && nRes == 1) 

                        { 

                            // сниму квадрат 

                            _ocr.OCR_ExcludeSquare(nBlankIndex, i, square); 

                        } 

                        // если кандидат не снят, а квадрат не используется 

                        else if (!oCurCand.Disabled && nRes == 0) 

                        { 

                            // восстановлю квадрат 

                            _ocr.OCR_RestoreSquare(nBlankIndex, i, square); 

                        } 

                    } 

                } 

            } 

        } 

 

 

        #endregion 

 

 

        #region IRecognitionManager Members 

 

 

        /// <summary> 


        /// Режим распознавания 

        /// </summary> 

        public RecognitionMode RecognitionMode { get; set; } 

 

 

		/// <summary> 

		/// Нужно ли сохранять изображения, когда драйвер реверсирует лист 

		/// </summary> 

		public bool NeedSaveImageOnDriverReverse 

		{ 

			get  

			{  

				return _config.DebugImageSaving.DriverReverse;  

			} 

		} 

 

 

		/// <summary> 

		/// Разрешен ли контроль печати УИК 

		/// </summary> 

		public bool StampControlEnabled  

		{ 

			get 

			{ 

				// если режим печати none, то контроля нет 

				if (_config.Ocr.Stamp.TestLevel == StampTestLevel.None) 

					return false; 

 

 

				return true; 

			} 

			set 

			{ 

				// сохраним старое значение 

				var oldValue = _config.Ocr.Stamp.TestLevel; 

 

 

				// разрешим запретим контроль печати 

				if (value) 

					_config.Ocr.Stamp.TestLevel = StampTestLevel.Halftone; 

				else 

					_config.Ocr.Stamp.TestLevel = StampTestLevel.None; 

 

 

				// вызовем событие изменения конфигурации 

				var configUpdatedArgs = new ConfigUpdatedEventArgs 

					(Name, "StampControlEnabled", oldValue, _config.Ocr.Stamp.TestLevel); 

				RaiseConfigUpdatedEvent(configUpdatedArgs); 

			} 

		} 


 
 

        /// <summary> 

        /// Разрешено ли принятие бюллетеня 

        /// </summary> 

        private bool BulletingReceivingAllowed 

        { 

            get 

            { 

                // если режим распознавание - тестирование геометрии бюллетеня 

                if (RecognitionMode == RecognitionMode.BulletinGeometryTesting) 

                    // то можно 

                    return true; 

 

 

                return _scannerManager.SheetProcessingSession.ReceivingAllowed; 

            } 

        } 

 

 

		/// <summary> 

		/// Последний результат распознавания 

		/// TODO: метод не в интерфейсе, потому что при его внесении в интерфейс получается 

		/// перекрестная ссылка Scanner - Recognition 

		/// </summary> 

		public RecognitionResult LastRecognitionResult 

		{ 

			get; 

			private set; 

		} 

 

 

        #region Процесс распознавания 

        /// <summary> 

        /// Признак того, что в данный момент выполняется распознавание бюллетеня 

        /// </summary> 

        private volatile bool _recognitionPerformNow = false; 

        /// <summary> 

        /// Режим голосования на момент начала распознавания бюллетеня 

        /// </summary> 

        /// <remarks>используется для того, чтобы исключить запись голосов с неправильным режимом, 

        /// когда во время распознавания изменился режим голосования</remarks> 

        private VotingMode _recognitionStartVotingMode; 

        /// <summary> 

        /// Сколько раз был вызван метод OCR.NextBuffer 

        /// </summary> 

        private int _ocrNextBufferCallsCount; 

        /// <summary> 

        /// Минимальное количество строк, обработанное распознавалкой в последний раз 

        /// </summary> 


        private int _minLineCountLast; 

        /// <summary> 

        /// Признак того, что нужно пробовать определить маркер при online-распознавании 

        /// </summary> 

        private bool _tryRecornizeMarkerOnline; 

        /// <summary> 

        /// Сторона бюллетеня, определенная при online-распознавании 

        /// </summary> 

        private ScannedSide _onlineSide = ScannedSide.Undefined; 

 

 

        /// <summary> 

        /// Запустить распознавание сканируемого листа 

        /// </summary> 

        /// <param name="lineWidth">ширина линейки сканера</param> 

        public void RunRecognition(int lineWidth) 

        { 

            Logger.LogVerbose(Message.DebugVerbose, "call"); 

 

 

            if (_ocr == null) 

                return; 

 

 

            // выставим признак того, что сейчас идет распознавание 

            _recognitionPerformNow = true; 

 

 

            // запомним текущий режим голосования 

            _recognitionStartVotingMode = _electionManager.CurrentVotingMode; 

 

 

            // сбросим счетчик вызовов метода OCR.NextBuffer 

            _ocrNextBufferCallsCount = 0; 

 

 

            // обнулим минимальное количество строк, считанных сканеров в последний раз 

            _minLineCountLast = 0; 

 

 

            // выставим признак того, что нужно пробовать определить макрер в online-режиме 

            _tryRecornizeMarkerOnline = true; 

 

 

            // сбросим инф-цию о стороне бюллетеня 

            _onlineSide = ScannedSide.Undefined; 

 

 

            // установим разрешение 

            SetOcrDpi(); 


 
 

            // удалим предыдущий лог 

			if (File.Exists(OcrLogFilePath)) 

            { 

                try 

                { 

                    // выключаем логирование 

                    _ocr.EnableLogging(null); 

                    // удалим файл 

					File.Delete(OcrLogFilePath); 

                } 

                catch (Exception ex) 

                { 

                    Logger.LogException(Message.Exception, ex, "Ошибка при удалении лога OCR"); 

                } 

            } 

 

 

            // включим логирование распознавалки 

            if (_config.Ocr.LoggingEnabled.Enabled) 

            { 

				_ocr.EnableLogging(OcrLogFilePath); 

            } 

            else 

            { 

                _ocr.EnableLogging(null); 

            } 

 

 

            // запускаем распознавание 

            Logger.LogVerbose(Message.DebugVerbose, "OCR.RunRecognize call"); 

            _ocr.RunRecognize(_scannerManager.WorkBufferTop, _scannerManager.WorkBufferBottom, lineWidth, lineWidth, 0, 0); 

            Logger.LogVerbose(Message.DebugVerbose, "OCR.RunRecognize return"); 

        } 

 

 

        /// <summary> 

        /// Установить разрешение распознавалки 

        /// </summary> 

        private void SetOcrDpi() 

        { 

            Logger.LogInfo(Message.RecognizerSetDpi, _scannerManager.DpiXTop, _scannerManager.DpiYTop,  

                _scannerManager.DpiXBottom, _scannerManager.DpiYBottom); 

 

 

            _ocr.SetDpi( 

                _scannerManager.DpiXTop, _scannerManager.DpiYTop, 

                _scannerManager.DpiXBottom, _scannerManager.DpiYBottom); 

        } 


 
 

        /// <summary> 

        /// Обработать следующий буфер сканируемого листа 

        /// </summary> 

        /// <param name="str0"></param> 

        /// <param name="str1"></param> 

        /// <param name="blankMarker">маркет бланка - заполняется в случае, когда метод возвращает true</param> 

        /// <returns>true - Бюллетень распознан в режиме Online; false - не распознан</returns> 

        public bool ProcessNextBuffer(short str0, short str1, out int blankMarker) 

        { 

            try 

            { 

                Logger.LogVerbose(Message.RecognizerNextBufferCall, str0, str1); 

 

 

                blankMarker = -1; 

 

 

                // если минимальное кол-во строк не изменилось 

                if (_minLineCountLast == Math.Min(str0, str1)) 

                    // то ничего не делаем 

                    return false; 

 

 

                // корректируем количество считанных строк 

                _minLineCountLast = Math.Min(str0, str1); 

 

 

                // отладочное сохранение буфера, если включено в конфиге 

                if (_config.DebugImageSaving.NextBuffer) 

                    // некст буфера сохраним с обоих сторон для анализа 

                    SaveLastImage("NEXTBUFFER_" + _minLineCountLast, ImageSavingType.Binary, ScannedSide.Undefined, null); 

 

 

                // если метод уже был вызван максимальное кол-во раз при распознавании одного бюллетеня 

				// если NextBufferMaxCalls = 0, пропускаем все вызовы 

                if (_ocrNextBufferCallsCount >= _config.Ocr.NextBufferMaxCalls.Value 

						&& _config.Ocr.NextBufferMaxCalls.Value != 0) 

                    // то ничего не делаем 

                    return false; 

 

 

                // если режим - распознавание бюллетеня и принимать лист нельзя 

                if (RecognitionMode == RecognitionMode.BulletinRecognition && 

                    !_scannerManager.SheetProcessingSession.ReceivingAllowed) 

                { 

                    // то реверсируем лист 

                    ReverseSheet((int)LogicalReverseReason.SheetReceivingForbidden); 

                } 


 
 

                // если закончили или продолжаем выполнять реверс листа 

                if (_scannerManager.EnsureSheetReversed()) 

                    // то выходим 

                    return false; 

 

 

                // если  

                if (// режим = распознавание бюллетеней, а online-распознавание вЫключено  

                    (RecognitionMode == RecognitionMode.BulletinRecognition && !_config.Ocr.InlineRecognize.Enabled) || 

                    // или распознавалка не инициализирована 

                    _ocr == null) 

                    // то выходим 

                    return false; 

 

 

                // распознаем буфер 

                Logger.LogVerbose(Message.RecognizerOcrNextBufferCall, _minLineCountLast); 

                var nextBufferResult = _ocr.NextBuffer(_minLineCountLast); 

                Logger.LogVerbose(Message.RecognizerOcrNextBufferReturn, _minLineCountLast, nextBufferResult); 

 

 

                // увеличим счетчик вызова метода OCR.NextBuffer 

                _ocrNextBufferCallsCount++; 

 

 

                // если режим не распознавание бюллетеня 

                if (RecognitionMode != RecognitionMode.BulletinRecognition) 

                    // то выходим 

                    return false; 

 

 

                // если распознавание буфера выполнено с ошибкой 

                if (nextBufferResult < 0) 

                { 

                    //TODO: в исходном коде этих настроек уже нет... 

                    // если не запрещен реверс по этому кода распознавания 

                    //if (!Scanner.Config.GetParamBool("CurScanner/DenyRevers/Disable[@code=\"" + nRes + "\"]", false)) 

 

 

                    // реверсируем лист 

                    ReverseSheet(nextBufferResult); 

                    return false; 

                } 

 

 

                // если распознавание выполнено корректно и нужно пробовать определить маркер 

                if (nextBufferResult > 0 && _tryRecornizeMarkerOnline) 

                { 


                    // запомним сторону, на которой маркер 

                    _onlineSide = NextBufferResultToScannedSide(nextBufferResult); 

 

 

                    // больше не будем пытаться определить маркет - только 1 попытка 

                    _tryRecornizeMarkerOnline = false; 

 

 

                    // определим индекс бланка 

                    int blankIndex = _ocr.GetOnlineMarker(_config.Ocr.Marker.Type); 

                    Logger.LogVerbose(Message.RecognizerOnlineBlankIndex, blankIndex); 

 

 

                    // если индекс бланка недопустимый 

                    if (blankIndex >= _electionManager.SourceData.Blanks.Length || 

                        blankIndex == (int)OnlineMarkerResult.Impossible)  

                    { 

                        // то реверсируем лист 

                        ReverseSheet((int)LogicalReverseReason.InvalidBlankNumber); 

                    } 

                    // если ошибка (кроме случая когда еще работаем над определением маркера) 

                    else if (blankIndex < 0 && blankIndex != (int)OnlineMarkerResult.InProgress) 

                    { 

                        Logger.LogWarning(Message.RecognizerOnlineFailed, (OnlineMarkerResult)blankIndex); 

                    } 

                    // иначе (blankIndex >= 0) - удалось определить индекс бланка 

                    if (blankIndex >= 0) 

                    { 

                        Logger.LogInfo(Message.RecognizerOnlineSuccess, blankIndex); 

 

 

                        // получу описание бланка 

                        var blank = _electionManager.SourceData.Blanks[blankIndex]; 

 

 

                        // если бланк бросили в не правильном режиме 

                        if (!_electionManager.SourceData.IsVotingModeValidForBlank(blank, _recognitionStartVotingMode)) 

                        { 

                            // то реверсируем лист 

                            ReverseSheet((int)LogicalReverseReason.BlankHasNoCurrentVoteRegime); 

                        } 

                        else 

                        { 

                            Logger.LogVerbose(Message.RecognizerOnlineBulletinValid); 

                            blankMarker = blank.Marker; 

                            return true; 

                        } 

                    }                     

                } 

 


 
                return false; 

            } 

            finally 

            { 

                Logger.LogVerbose(Message.RecognizerNextBufferReturn, str0, str1); 

            } 

        } 

 

 

        /// <summary> 

        /// По коду результата работы метода NextBuffer возвращает сторону 

        /// </summary> 

        /// <param name="nextBufferResult"></param> 

        /// <returns></returns> 

        private ScannedSide NextBufferResultToScannedSide(int nextBufferResult) 

        { 

            // nextBufferResult может содержать следующие значения, которые соотв. сторонам 

            /// 1 - верхняя сторона 

            /// 2 - нижняя сторона 

            /// иначе - сторона не определена 

            if (nextBufferResult == 1) 

                return ScannedSide.Top; 

 

 

            if (nextBufferResult == 2) 

                return ScannedSide.Bottom; 

 

 

            return ScannedSide.Undefined; 

        } 

 

 

        /// <summary> 

        /// Реверсировать лист 

        /// </summary> 

        /// <param name="reasonCode">Код причины реверса</param> 

        private void ReverseSheet(int reasonCode) 

        { 

			if (_config.DebugImageSaving.Reverse) 

				SaveLastImage("REVERSE", ImageSavingType.Binary, _onlineSide, null); 

 

 

            _scannerManager.ReverseSheet(reasonCode); 

        } 

 

 

        #endregion 

 

 


        #region Завершение распознавания 

        /// <summary> 

        /// Завершить распознавание путем сброса состояния распознавалки 

        /// </summary> 

        public void ResetRecognition() 

        { 

            Logger.LogVerbose(Message.DebugVerbose, "call"); 

 

 

            // если драйвер не инициализирован или распознавание и так не выполняется 

            if (_ocr == null || !_recognitionPerformNow) 

                // то ничего не делаем 

                return; 

 

 

            try 

            { 

                _recognitionPerformNow = false; 

                _ocr.EndRecognize(MarkerType.None); 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.RecognizerException, ex, "Ошибка при сбросе состояния распознавалки"); 

            } 

        } 

 

 

        /// <summary> 

        /// Завершить распознавание отсканированного листа 

        /// </summary> 

        public void EndRecognition() 

        { 

            switch (RecognitionMode) 

            { 

                case RecognitionMode.BulletinRecognition: 

                    EndRecognizeBulletin(); 

                    break; 

 

 

                case RecognitionMode.BulletinGeometryTesting: 

                    TestBulletin(); 

                    break; 

 

 

                default: 

                    throw new Exception("Неизвестный режим работы: " + RecognitionMode); 

            } 

        } 

 

 


        /// <summary> 

        /// Тестирование геометрии бюллетеня 

        /// </summary> 

        private void TestBulletin() 

        { 

            //TODO: Тестирование геометрии бюллетеня 

            throw new NotImplementedException(); 

        } 

 

 

        /// <summary> 

        /// Завершить распознавание отсканированного бюллетеня 

        /// </summary> 

        private void EndRecognizeBulletin() 

        { 

            Logger.LogVerbose(Message.DebugVerbose, "call"); 

 

 

            // код результата распознавания 

            int recResultCode;       

            // код метода маркировки 

            // по умолчанию, на случай, если распознавалка недоступна, считаем, что это НУФ 

            short markingCode = _config.Blanks[BlankType.Bad].MarkingCode; 

            RecognitionResult recognitionResult = null; 

 

 

            try 

            { 

                // если распознавалка проинициализирована 

                if (_ocr != null) 

                { 

                    // начинаем распознавание 

                    _recognitionPerformNow = false; 

 

 

                    SetOcrDpi(); 

 

 

                    try 

                    { 

                        Logger.LogInfo( 

                            Message.DebugVerbose, "Lines = " + _scannerManager.ScannedLinesCountLast); 

 

 

                        if (_config.DebugImageSaving.NextBuffer) 

                            SaveLastImage( 

                                "LAST_NEXTBUFFER_" + _scannerManager.ScannedLinesCountLast, 

                                ImageSavingType.Binary, _onlineSide, null); 

 

 


                        // контрольный вызов NextBuffer 

                        Logger.LogVerbose(Message.DebugVerbose, "NextBuffer..."); 

                        _ocr.NextBuffer(_scannerManager.ScannedLinesCountLast); 

 

 

                        if (_config.DebugImageSaving.Presave) 

                            SaveLastImage("PRESAVE", ImageSavingType.All, _onlineSide, null); 

 

 

                        // завершаем распознавание 

                        Logger.LogVerbose(Message.DebugVerbose, "EndRecognize..."); 

                        recResultCode = _ocr.EndRecognize(_config.Ocr.Marker.Type); 

                        Logger.LogVerbose(Message.DebugVerbose, "EndRecognize done"); 

                    } 

                    catch (Exception ex) 

                    { 

                        // Выставляю код ошибки распознавания 

                        recResultCode = (int)OcrRecognitionResult.ERROR; 

 

 

                        // регистрирую в журнале 

                        Logger.LogException(Message.RecognizerException, ex, "Ошибка распознавания бюллетеня"); 

 

 

                        // Пытаюсь сохранить изображение 

                        try 

                        { 

                            SaveLastImage("REC-ERROR", ImageSavingType.All, _onlineSide, null); 

                        } 

                        catch (Exception exInner) 

                        { 

                            Logger.LogException( 

                                Message.RecognizerException, exInner, "Ошибка при сохранении изображения"); 

                        } 

 

 

                        // пытаюсь сохранить копию лога OCR 

                        try 

                        { 

                            if (_config.Ocr.LoggingEnabled.Enabled) 

                            { 

                                // Выключаю логгирование (закрываю файл OCR.txt) 

                                _ocr.EnableLogging(null); 

 

 

                                // получаю путь к файлу назначения 

                                var filePath = GetRecErrorFilePath(_scannerManager.SerialNumber); 

 

 

                                // Копирую файл 


								File.Copy(OcrLogFilePath, filePath, true); 

                            } 

                        } 

                        catch (Exception exInner) 

                        { 

                            Logger.LogException( 

                                Message.RecognizerException, exInner, "Ошибка при сохранении копии лога OCR", exInner); 

                        } 

                    } 

 

 

                    try 

                    { 

                        recognitionResult = RecognitionResultAnalisys(recResultCode); 

                        markingCode = recognitionResult.MarkingCode; 

                        Logger.LogVerbose(Message.DebugVerbose, "Результат распознавания сохранен"); 

                    } 

                    catch (Exception ex) 

                    { 

                        Logger.LogException(Message.RecognizerSaveResultError, ex); 

                        // бросам как НУФ 

                        markingCode = _config.Blanks[BlankType.Bad].MarkingCode; 

                    } 

 

 

                    // Если распознавание свалилось с ошибкой 

                    if (recResultCode == (int)OcrRecognitionResult.CALL || 

                        recResultCode == (int)OcrRecognitionResult.ERROR) 

                    { 

                        // переинициализируем распознавалку 

                        try 

                        { 

                            InitRecognition(); 

                        } 

                        catch (Exception ex) 

                        { 

                            Logger.LogException( 

                                Message.RecognizerException, ex, "Ошибка при инициализации распознавалки"); 

                        } 

                    } 

                } 

 

 

                Logger.LogVerbose(Message.DebugVerbose, "Маркируем лист методом: " + markingCode); 

 

 

                // сбасываем бюллетень в урну 

 

 

                var result = _scannerManager.DropSheet(markingCode); 


                // протоколируем 

                Logger.LogInfo(Message.RecognizerSheetDroped, result); 

 

 

                switch(result) 

                { 

                    case DropResult.Reversed: 

                        // лист был реверсирован, ничего не делаем 

                        break; 

                    default: 

                        // в остальных случаях нужно добавить НУФ в результаты, если он был 

                        if (recognitionResult != null) 

                        { 

                            if (recognitionResult.BlankType == BlankType.Bad || 

                                recognitionResult.BlankType == BlankType.BadMode) 

                            { 

                                AddVotingResult(recognitionResult); 

                            } 

                        } 

                        break; 

                } 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException( 

                    Message.RecognizerException, ex, "Ошибка при завершении распознавания бюллетеня", ex); 

            } 

            finally 

            { 

                // Выключаю логгирование 

                try 

                { 

                    _ocr.EnableLogging(null); 

                } 

                catch (Exception ex) 

                { 

                    Logger.LogException(Message.RecognizerException, ex, "Ошибка при выключении лонирования OCR"); 

                } 

 

 

                Logger.LogVerbose(Message.DebugVerbose, "return"); 

 

 

                if (_config.GCCollect.Enabled) 

                { 

                    GC.Collect(0, GCCollectionMode.Forced); 

                } 

            } 

        } 

 


 
        /// <summary> 

        /// Анализ результата распознавания 

        /// </summary> 

        /// <param name="recResultCode">код результата распознавания</param> 

        /// <returns>Результат распозавания бюллетеня</returns> 

        private RecognitionResult RecognitionResultAnalisys(int recResultCode) 

        { 

            PollResult pollRes;                         // результат распознавания 

            int[] currentMarks = null;		            // массив отметок для конкретных выборов 

            int[][] sectionsMarks = null;               // массив отметок по всем секциям бланка 

            bool[] sectionsValidity = null;             // признаки корректности секций бюллетеня 

            var recLogAddInfoSB = new StringBuilder();  // доп. информация для записи в файл журнала распознавания 

 

 

            var stampResult = StampResult.YES;          // результат распознавания печати 

            var stampNumber = "";                       // Наиболее вероятный номер печати (полутон) 

            var stampAlternatives = new string[4];      // Альтернативы цифр номера печати по позициям (полутон) 

 

 

 

 

            if (recResultCode >= 0) 

            { 

                #region формирование массива меток 

 

 

                // выделю память 

                sectionsMarks = new int[_ocr.Results.Count][]; 

                sectionsValidity = new bool[_ocr.Results.Count]; 

 

 

                // перебираем все секции в бюллетене 

                for (int sectionIndex = 0; sectionIndex < _ocr.Results.Count; sectionIndex++) 

                { 

                    // находим выборы по номеру бюллетеня и номеру секции 

                    var electionNum = _electionManager.SourceData.Blanks[_ocr.BulletinNumber].Sections[sectionIndex]; 

                    var currentElection = _electionManager.SourceData.GetElectionByNum(electionNum); 

 

 

                    // по умолчанию считаем секцию невалидной 

                    sectionsValidity[sectionIndex] = false; 

                    // текущий результат распознавания 

                    pollRes = _ocr.Results[sectionIndex]; 

 

 

                    // выделю память 

                    currentMarks = new int[pollRes.Count]; 

                    // количество отметок за вычетом снятых кандидатов 

                    int activeMarksCount = pollRes.Count; 


                    // признак того, что отмечен "против всех" 

                    bool noneAboveActive = false; 

 

 

                    // по всем отметкам 

                    for (int squareIndex = 0; squareIndex < pollRes.Count; squareIndex++) 

                    { 

                        currentMarks[squareIndex] = pollRes[squareIndex]; 

 

 

                        // проверка бюллетеня на превышение меток 

                        // Находим кандидата 

                        Candidate currentCandidate = currentElection.Candidates[currentMarks[squareIndex]]; 

 

 

                        // если кандидат снят 

                        if (currentCandidate.Disabled) 

                        { 

                            // то уменьшим кол-во "активных" отметок 

                            activeMarksCount--; 

                        } 

                        else 

                        { 

                            // если отмечен "против всех", то запомним это 

                            if (currentCandidate.NoneAbove) 

                            { 

                                noneAboveActive = true; 

                            } 

                        } 

                    } 

 

 

                    sectionsMarks[sectionIndex] = currentMarks; 

 

 

                    // бюллетень действительный, если отметки есть и их кол-во не превышает мандатность выборов 

                    sectionsValidity[sectionIndex] = 

                        activeMarksCount > 0 && activeMarksCount <= currentElection.MaxMarks; 

 

 

                    // если отмечен "против всех" 

                    if (noneAboveActive) 

                        // то этот кандидат должен быть единственным 

                        sectionsValidity[sectionIndex] = (activeMarksCount == 1); 

 

 

                    // формирую подстроку 

                    recLogAddInfoSB.AppendFormat("E{0},", sectionIndex); 

                    foreach (int squareNum in sectionsMarks[sectionIndex]) 

                    { 


                        recLogAddInfoSB.Append(squareNum); 

                        recLogAddInfoSB.Append(','); 

                    } 

                } 

 

 

                #endregion 

 

 

                #region вычисление результата распознавания печати 

 

 

                if (_config.Ocr.Stamp.TestLevel != StampTestLevel.None) 

                { 

                    // если полутоновое распознавание печати 

                    if (_config.Ocr.Stamp.TestLevel == StampTestLevel.Halftone) 

                    { 

                        stampResult = Ocr.Ocr.IsStampOKGray(ref stampNumber, ref stampAlternatives); 

                        Logger.LogVerbose(Message.RecognizerHalftoneStamp, stampResult, stampNumber); 

                    } 

                    // бинарное распознавние печати 

                    else 

                    { 

                        stampNumber = "0";    

                        stampResult = _ocr.StampResult; 

                        Logger.LogVerbose(Message.RecognizerBinaryStamp, stampResult); 

                    } 

                } 

 

 

                #endregion 

            } 

 

 

            // формируем результат распознавания 

            var recResult = CreateRecognitionResult(recResultCode, sectionsMarks, sectionsValidity,  

                stampResult, stampNumber, stampAlternatives); 

 

 

			LastRecognitionResult = recResult; 

 

 

            // записываем результат распознавания в лог 

            WriteRecognitionResultToLog(recResult, stampResult, stampNumber, recLogAddInfoSB.ToString()); 

 

 

            // добавление результата голосования - если не нуф, то добавляем сразу же 

            if (recResult.BlankType != BlankType.Bad && recResult.BlankType != BlankType.BadMode) 

            { 

                AddVotingResult(recResult); 


            } 

            else 

            { 

                // в случае НУФ установим только результат последнего листа 

                // TODO: подумать как сделать лучше 

                var votingResult = new VotingResult( 

                    recResult.BlankType, 

                    recResult.BulletinNumber, 

                    recResult.StampNumber, 

                    recResult.BadBulletinReason.ToString(), 

                    recResult.BadStampReason, 

                    recResult.Marks, 

                    recResult.SectionsValidity); 

 

 

                _electionManager.SetLastVotingResult(votingResult); 

            } 

 

 

            // сохраняем изображение 

            SaveLastImage(recResult.ImageFilePrefix, recResult.ImageSavingType, _onlineSide, null); 

 

 

            // Возвращаю результат распозавания 

            return recResult; 

        } 

 

 

        /// <summary> 

        /// Добавляет результат распознавания 

        /// </summary> 

        /// <param name="recResult">Результат распозавания</param> 

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

 

 

                _electionManager.AddVotingResult( 

                    votingResult, 

                    _recognitionStartVotingMode, 

                    _scannerManager.IntSerialNumber); 


            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.RecognizerAddVotingResultError, ex); 

            } 

        } 

 

 

        /// <summary> 

        /// Записать в журнал результат распознавания 

        /// </summary> 

        /// <param name="recResult">результат распознавания бюллетеня</param> 

        /// <param name="stampResult">результата распозавания печати</param> 

        /// <param name="stampNumber">номер печати</param> 

        /// <param name="additionalInfo">дополнительная информация</param> 

        private void WriteRecognitionResultToLog( 

            RecognitionResult recResult, StampResult stampResult, string stampNumber, string additionalInfo) 

        { 

            try 

            { 

#if DEBUG 

                // получим кол-во опущенных в сканер бюллетеней 

                VoteKey votesKey = new VoteKey() 

                { 

                    BlankType = BlankType.All, 

                    ScannerSerialNumber = _scannerManager.IntSerialNumber 

                }; 

                // +1 - т.к. данный новый бюллетень еще не учтен 

                var votesCount = _electionManager.VotingResults.VotesCount(votesKey) + 1; 

 

 

                StringBuilder marker = new StringBuilder(); 

                if (recResult.ResultCode != OcrRecognitionResult.MARK) 

                { 

                    if (recResult.BulletinNumber >= 0 && 

                        recResult.BulletinNumber < _electionManager.SourceData.Blanks.Length) 

                    { 

                        marker.Append(_electionManager.SourceData.Blanks[recResult.BulletinNumber].Marker); 

                    } 

                    else 

                    { 

                        marker.AppendFormat("0 ({0})", recResult.BulletinNumber); 

                    } 

                } 

                else 

                { 

                    marker.Append('0'); 

                } 

 

 


                _scanningLogger.LogInfo(Message.RecognitionResult,  

                    // количество бюллетеней и результат распознавания 

                    votesCount, recResult.IntResultCode, recResult.ResultCode, 

                    // маркер, тип бланка, результат распознавания печати, метки 

                    marker, recResult.BlankType, stampResult, additionalInfo); 

#endif 

 

 

                var recLogSB = new StringBuilder(); 

 

 

                // режим голосования 

                recLogSB.AppendFormat("{0},", _recognitionStartVotingMode); 

 

 

                // имя файла изображения бюллетеня 

                recLogSB.AppendFormat("{0},", GetImageFileName(recResult.ImageFilePrefix)); 

 

 

                // номер бюллетеня 

                recLogSB.AppendFormat("{0},", _ocr.BulletinNumber); 

 

 

                // серийный номер сканера 

                recLogSB.AppendFormat("{0},", _scannerManager.SerialNumber); 

 

 

                // результат распознавания бюллетеня 

                recLogSB.AppendFormat("{0},", recResult.IntResultCode); 

 

 

                // номер печати 

                recLogSB.AppendFormat("{0},", stampNumber); 

 

 

                // результат распознавания печати 

                recLogSB.AppendFormat("{0},", stampResult); 

 

 

                // дополнительная информация 

                recLogSB.Append(additionalInfo); 

 

 

                _rawRecognitionResultLogger.LogInfo(Message.RecognizerLog, recLogSB.ToString()); 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.RecognizerLog, ex, "Ошибка записи в журнал сканирования"); 

            } 

        } 


 
 

        /// <summary> 

        /// Формирование объекта, который описывает результат распознавания 

        /// TODO: нужно переделать в конструктор объекта RecognitionResult 

        /// </summary> 

        /// <param name="recResultCode">Результат распознавания</param> 

        /// <param name="marks">Отметки</param> 

        /// <param name="sectionsValidity">Признаки действительности секций</param> 

        /// <param name="stampResult">Результат распознавания печати</param> 

        /// <param name="stampNumber">Номер печати</param> 

        /// <param name="stampNumberAlts">Альтернативы цифр печати</param> 

        /// <returns></returns> 

        private RecognitionResult CreateRecognitionResult( 

            int recResultCode,  

            int[][] marks,  

            bool[] sectionsValidity,  

            StampResult stampResult,  

            string stampNumber,  

            string[] stampNumberAlts) 

        { 

            // результат распознавания 

            var recResult = new RecognitionResult(recResultCode) 

            { 

                StampResult = stampResult, 

                StampNumber = stampNumber, 

                StampNumberAlts = stampNumberAlts, 

                BulletinNumber = recResultCode != (int)OcrRecognitionResult.MARK ? _ocr.BulletinNumber : -1, 

                Marks = marks, 

                SectionsValidity = sectionsValidity 

            }; 

 

 

            // определим номер печати 

            FindStampNumber(recResult); 

            // сформирует инфу с описанием определения номера печати 

            CreateStampInfo(recResult); 

 

 

            if (recResult.ResultCode == OcrRecognitionResult.OK && recResult.StampOK) 

            { 

                // если номер бюллетеня неправильный 

                if (recResult.BulletinNumber < 0 ||  

                    recResult.BulletinNumber >= _electionManager.SourceData.Blanks.Length) 

                { 

                    recResult.BlankType = BlankType.Bad; 

                    recResult.ResultDescription = "Недопустимый номер бюллетеня " + recResult.BulletinNumber; 

                } 

                else 

                { 


                    // получу описание бланка 

                    var blank = _electionManager.SourceData.Blanks[recResult.BulletinNumber]; 

 

 

                    // если бланк бросили в правильном режиме 

                    if (_electionManager.SourceData.IsVotingModeValidForBlank(blank, _recognitionStartVotingMode)) 

                    { 

                        Logger.LogVerbose(Message.DebugVerbose, "Бюллетень распознан"); 

 

 

                        // переберем отметки в секциях бюллетеня 

                        for (int i = 0; i < recResult.Marks.Length; i++) 

                        { 

                            // массив отметок в текущей секции 

                            var curSectionMarks = recResult.Marks[i]; 

                            // выборов, которые соотв. текущей секции 

                            var curSectionElection = _electionManager.SourceData.GetElectionByNum(blank.Sections[i]); 

 

 

                            // уточняю корректность секции 

                            recResult.SectionsValidity[i] = AdjustSectionValidity( 

                                curSectionElection, curSectionMarks, recResult.SectionsValidity[i]); 

 

 

                            // если хоть один хороший 

                            if (recResult.SectionsValidity[i]) 

                            { 

                                // то принимаем весь лист 

                                recResult.BlankType = BlankType.Valid; 

                                break; 

                            } 

 

 

                            // если не приняли, то посмотрим почему 

                            if (curSectionMarks.Length > 0) 

                                // много меток 

                                recResult.BulletinWithExtraLabels = true; 

                            else 

                                // нет меток 

                                recResult.BulletinWithoutLabels = true; 

                        } 

 

 

                        // если бланк некорректен 

                        if (recResult.BlankType != BlankType.Valid) 

                        { 

                            // то уточняем причину 

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

                            GetSquareDescription(recResultCode, recResult.Marks, false), 

                            DateTime.Now.ToString("HH:mm:ss.fff")); 

 

 

                        Logger.LogVerbose(Message.RecognizerBulletinResult, 

                            recResult.BlankTypeDescription, 

                            _recognitionStartVotingMode); 

                    } 

                    else 

                    { 

                        recResult.BlankType = BlankType.BadMode; 

                        recResult.ResultDescription = "Бюллетень не имеет текущего режима голосования"; 

                        Logger.LogVerbose(Message.DebugVerbose, recResult.ResultDescription); 

                    } 

                } 

            } 

            else 

            { 

                // НУФ 

                recResult.BlankType = BlankType.Bad; 

                CreateBadBulletinInfo(recResult); 

 

 

                recResult.ResultDescription = string.Format("Бюллетень неустановленной формы ({0}).", 

                    recResult.BadBulletinDescription); 

                Logger.LogVerbose(Message.RecognizerNuf, recResult.ResultDescription); 

            } 

 

 

            // получим параметры для обработки бланка 

            var blankConfig = _config.Blanks[recResult.BlankType]; 

 

 

            if(recResult.BlankType == BlankType.Bad &&  

                recResult.BadBulletinReason == BadBulletinReason.Stamp) 

            { 

                if (_config.Blanks[BlankType.BadStamp] != null) 

                { 

                    blankConfig = _config.Blanks[BlankType.BadStamp]; 


                } 

            } 

 

 

            // вычислим префикс файла для сохранения изображения 

            var imageFilePrefixStringBuilder = new StringBuilder(); 

            imageFilePrefixStringBuilder.Append(blankConfig.ImageFilePrefix); 

 

 

            // для НУФа расширяем префикс 

            if (!string.IsNullOrEmpty(recResult.BadBulletinFilePrefix)) 

            { 

                imageFilePrefixStringBuilder.Append('-'); 

                imageFilePrefixStringBuilder.Append(recResult.BadBulletinFilePrefix); 

            } 

 

 

            // выведем еще номер печати для полутонового распознавания 

            if (_config.Ocr.Stamp.TestLevel == StampTestLevel.Halftone) 

            { 

                imageFilePrefixStringBuilder.Append('-'); 

                imageFilePrefixStringBuilder.Append(recResult.StampNumber); 

            } 

 

 

            recResult.ImageFilePrefix = imageFilePrefixStringBuilder.ToString(); 

 

 

            // Режим маркировки  

            recResult.MarkingCode = blankConfig.MarkingCode; 

 

 

            // в тестовом режиме возьмем остальное из секции для тестовых 

            if (_recognitionStartVotingMode == VotingMode.Test) 

                blankConfig = _config.Blanks[BlankType.Test]; 

 

 

            // тип сохранения изображения 

            recResult.ImageSavingType = blankConfig.ImageSavingType; 

 

 

            return recResult; 

        } 

 

 

        /// <summary> 

        /// Формирует информацию о результате распознавания печати 

        /// </summary> 

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

                            if (recResult.StampOK) 

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

 

 

        /// <summary> 

        /// Формирует информацию по НУФу 

        /// </summary> 

        private void CreateBadBulletinInfo(RecognitionResult recResult) 

        { 

            if (recResult.ResultCode == OcrRecognitionResult.OK && 

                (recResult.StampResult != StampResult.YES || !recResult.StampOK)) 

            { 

                recResult.BadBulletinFilePrefix = "BAD-STAMP"; 

                recResult.BadBulletinReason = BadBulletinReason.Stamp; 

                recResult.BadStampReason = recResult.StampReasonCode; 

                recResult.BadBulletinDescription = "Печать - " + recResult.StampDescription; 

                recResult.BadBulletinShortDescription = "Печать - " + recResult.StampShortDescription; 


                return; 

            } 

 

 

            // по умолчанию причина НУФ маркер 

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

 

 

        /// <summary> 

        ///	Формирует описание квадратов с отметками 

        /// </summary> 

        /// <param name="recResultCode">код результата распознавания</param> 

        /// <param name="marks">отметки во всех секциях</param> 

        /// <param name="getShortDescription">признак сокращенного вывода</param> 

        public string GetSquareDescription(int recResultCode, int[][] marks, bool getShortDescription) 

        { 

            var sb = new StringBuilder(); 

 

 

            if (recResultCode > 0) 

            { 

                // По всем секциям 

                for (int i = 0; i < marks.Length; i++) 

                { 

                    // если более 1 секции 

                    if (marks.Length > 1)  

                        sb.AppendFormat("\nСекция N {0}. ", (i + 1)); 

 

 

                    if (marks[i] != null) 

                    { 

                        if (marks[i].Length == 0) 

                            sb.Append("\n   Нет отметок"); 

                        else if (marks[i].Length > 1) 

                        { 

                            if (getShortDescription) 

                                sb.Append("Отметки: "); 

                            else 

                                sb.Append("\n   Отметки в квадратах: "); 

                        } 

                        else if (marks[i].Length == 1) 

                        { 

                            if (getShortDescription) 

                                sb.Append("Отметка: "); 

                            else 

                                sb.Append("\n   Отметка в квадрате "); 

                        } 

 

 

                        for (int j = 0; j < marks[i].Length; j++) 

                        { 

                            if (j > 0)  

                                sb.Append(", "); 


 
 

                            sb.Append(marks[i][j] + 1); 

                        } 

                    } 

 

 

                    sb.Append(". "); 

                } 

 

 

                if (sb.Length > 0) 

                    sb.Length -= 1; 

            } 

 

 

            var res = sb.ToString(); 

 

 

            // если нужно короткое описание 

            if (getShortDescription) 

                // то убираю все пробелы и переносы строк 

                res = Regex.Replace(res, "\\s+|\\n|\\r", ""); 

 

 

            return res; 

        } 

 

 

        /// <summary> 

        /// Уточняет корректность секции 

        /// </summary> 

        /// <param name="election">описание выборов</param> 

        /// <param name="sectionMarks">результаты распознавания по квадратам</param> 

        /// <param name="curSectionValidity">результат проверки действительности секции бюллетеня (из распознавалки) 

        /// </param> 

        /// <returns>уточненное значение корректности секции</returns> 

        private bool AdjustSectionValidity( 

            Election.Voting.Election election, int[] sectionMarks, bool curSectionValidity) 

        { 

            // если  

            if (// в выборах нет кандидата "Против всех" 

                !election.NoneAboveExists || 

                // или кол-во кандидатов не больше 1 

                sectionMarks.Length <= 1 || election.Candidates.Length <= 1 || 

                // или секция и так уже определена, как не корректная 

                !curSectionValidity) 

            { 

                // не изменяем значение корректности 

                return curSectionValidity; 


            } 

 

 

            // Кандидат "Против всех" - последний 

            if (sectionMarks[sectionMarks.Length - 1] == election.Candidates.Length - 1) 

                return false; 

 

 

            return curSectionValidity; 

        } 

 

 

        /// <summary> 

        /// Найти номер печати в ИД 

        /// </summary> 

        private void FindStampNumber(RecognitionResult recResult) 

        { 

            // если печать проверять не нужно 

            if (_config.Ocr.Stamp.TestLevel == StampTestLevel.None) 

            { 

                recResult.StampOK = true; 

                return; 

            } 

 

 

            // если распознавание прошло с ошибкой 

            if (recResult.ResultCode < 0) 

            { 

                recResult.StampOK = false; 

                return; 

            } 

 

 

            // если распознавание печати не полутоновое и результат - печать распознана 

            if (_config.Ocr.Stamp.TestLevel != StampTestLevel.Halftone && recResult.StampResult == StampResult.YES) 

            { 

                recResult.StampOK = true; 

                return; 

            } 

 

 

            // обработка для полутонового распознавания печати 

            Logger.LogVerbose(Message.RecognizerStampNumber, recResult.StampNumber,  

                recResult.StampNumberAlts[0], recResult.StampNumberAlts[1],  

                recResult.StampNumberAlts[2], recResult.StampNumberAlts[3]); 

 

 

            // если печать не распознана и даже не распознана, как бледная 

            if (recResult.StampResult != StampResult.YES && recResult.StampResult != StampResult.FAINT) 

            { 


                recResult.StampOK = false; 

                return; 

            } 

 

 

            int iStampNumber = 0; 

            // если номер печати не является числом 

            if (!int.TryParse(recResult.StampNumber, out iStampNumber)) 

            { 

                recResult.StampOK = false; 

                return; 

            } 

 

 

            // если наиболее вероятный номер - это номер УИК в ИД 

            if (iStampNumber == _electionManager.UIK) 

            { 

                recResult.StampOK = true; 

                return; 

            } 

 

 

            var uikStr = _electionManager.UIK.ToString("0000"); 

            // если наиболее вероятный номер не нашли в ИД, но нашли альтернативу 

            if (СheckAlternatives(uikStr, recResult.StampNumberAlts)) 

            { 

                // меняем распознанный номер 

                recResult.StampNumber = uikStr; 

                recResult.StampOK = true; 

                return; 

            } 

 

 

            // если нет ИД или номера вышестоящих комиссий использовать запрещено 

            if (_electionManager.SourceData == null || !AllowSuperiorStamp) 

            { 

                // дальше нет смысла искать номер 

                recResult.StampOK = false; 

                return; 

            } 

 

 

            // проверим номера вышестоящих комиссий 

            var blank = _electionManager.SourceData.Blanks[recResult.BulletinNumber]; 

 

 

            for (int i = 0; i < blank.Sections.Length; i++) 

            { 

                var electionNum = blank.Sections[i]; 

                var election = _electionManager.SourceData.GetElectionByNum(electionNum); 


 
 

                // перебираем номера вышестоящих комиссий 

                foreach (var stampCommittee in election.StampCommittees) 

                { 

                    // если номер корректен 

                    if (stampCommittee.Num > 0 && iStampNumber == stampCommittee.Num) 

                    { 

                        // номер найден 

                        recResult.StampOK = true; 

                        return; 

                    } 

                } 

            } 

 

 

            // не нашли номер 

            recResult.StampOK = false; 

        } 

 

 

        /// <summary> 

        /// Анализ номера печати по альтернативам 

        /// </summary> 

        /// <param name="stampNumber">Номер печати</param> 

        /// <param name="stampAlterantives">Альтернативы цифр номера печати по позициям</param> 

        /// <returns>true - удалось найти сопоставление</returns> 

        private bool СheckAlternatives(string stampNumber, string[] stampAlterantives) 

        { 

            Logger.LogVerbose(Message.RecognizerCheckAlternatives, stampNumber); 

 

 

            // по всем цифрам печати 

            for (int i = 0; i < Ocr.Ocr.STAMP_DIGIT_COUNT; i++) 

            { 

                bool match = false; 

                // проходим по всем альтернативам этой цифры 

                foreach (char altDigit in stampAlterantives[i]) 

                { 

                    // проверяем совпадение 

                    if (altDigit == stampNumber[i]) 

                    { 

                        match = true; 

                        break; 

                    } 

                } 

 

 

                // если не нашли ни одного совпадения для позиции 

                if (!match) 


                    return false; 

            } 

 

 

            // в эту точку попадаем только если успешны сопоставления всех цифр				 

            return true; 

        } 

 

 

		/// <summary> 

		/// Резервирование места для сохранения буфера 

		/// </summary> 

		/// <param name="savingType">тип сохраняемого изображения</param> 

		/// <param name="sideToSave">сторона</param> 

		/// <returns>успех операции</returns> 

		private bool ReserveSpaceForSaveLastBuffer(ImageSavingType savingType, ScannedSide sideToSave) 

		{ 

			try 

			{ 

				// требуемый размер 

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

 

 

				// округлим до 1024 байт сверху 

				requiredSize = 1024 * ((requiredSize + 1024 - 1) / 1024); 

 

 

				// минимально необходимый размер 

				long minSize = _config.MinFreeSpaceForImageKb.Value * 1024; 


				long availableSize; 

				// путь к директории с картинками 

				var imageDirectoryPath = _fileSystemManager.GetDataDirectoryPath(FileType.ScanningImage); 

 

 

				// если не удалось зарезервировать место на диске 

				if (!_fileSystemManager.ReserveDiskSpace( 

					FileType.ScanningImage, imageDirectoryPath, requiredSize, minSize, out availableSize)) 

				{ 

					Logger.LogWarning( 

						Message.RecognizerSaveImageNotEnoughFreeSpace, imageDirectoryPath, availableSize, requiredSize); 

 

 

					return false; 

				} 

			} 

			catch (Exception ex) 

			{ 

				Logger.LogException(Message.RecognizerException, ex, "Ошибка при сохранении изображения"); 

 

 

				// восстанавливаем сканирование 

				_scannerManager.RestoreScanningAfterError(); 

 

 

				return false; 

			} 

 

 

			return true; 

		} 

 

 

        /// <summary> 

        /// Сохраняет изображение, отсканированное на момент вызова метода 

        /// </summary> 

        /// <param name="filePrefix">префикс изображения</param> 

        /// <param name="savingType">что сохранять</param> 

        /// <param name="sideToSave">какую сторону сохранять</param> 

		/// <param name="errorCode">номер ошибки драйвера(может быть null)</param> 

        /// <returns></returns> 

        private bool SaveLastImage( 

            string filePrefix, ImageSavingType savingType, ScannedSide sideToSave, int? errorCode) 

        { 

            Logger.LogVerbose(Message.RecognizerSaveImageCall, filePrefix, savingType, sideToSave); 

 

 

            // если не надо сохранять 

            if (savingType == ImageSavingType.None) 

                return true; 


 
 

            try 

            { 

				if (!ReserveSpaceForSaveLastBuffer(savingType, sideToSave)) 

					return false; 

 

 

                Logger.LogVerbose(Message.DebugVerbose, "Сохранение изображения..."); 

                // запомним время начала сохранения 

                DateTime savingStartTime = DateTime.Now; 

 

 

                // генерирую полный путь к файлу 

                var filePathSB = GetImageFileName(filePrefix); 

				// путь к директории с картинками 

				var imageDirectoryPath = _fileSystemManager.GetDataDirectoryPath(FileType.ScanningImage); 

 

 

                // вставляю полный путь до каталога 

				filePathSB.Insert(0, imageDirectoryPath + '/'); 

 

 

                var filePath = filePathSB.ToString(); 

 

 

				// если надо добавить код ошибки 

				if (errorCode != null) 

					filePathSB.Append("_E" + errorCode); 

 

 

                // если надо сохранять бинар 

                if (savingType == ImageSavingType.Binary || savingType == ImageSavingType.All) 

                { 

                    _scannerManager.SaveBuffer(filePath + "_B", ImageType.Binary, sideToSave, BufferSize.Scanned); 

                } 

 

 

                // если надо сохранять полутон 

                if (savingType == ImageSavingType.Halftone || savingType == ImageSavingType.All) 

                { 

                    _scannerManager.SaveBuffer(filePath + "_H", ImageType.Halftone, sideToSave, BufferSize.Scanned); 

                    _fileSystemManager.Sync(); 

                } 

 

 

                Logger.LogVerbose( 

                    Message.RecognizerSaveImageTiming, (DateTime.Now - savingStartTime).TotalMilliseconds); 

 

 


                return true; 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.RecognizerException, ex, "Ошибка при сохранении изображения"); 

 

 

                // восстанавливаем сканирование 

                _scannerManager.RestoreScanningAfterError(); 

 

 

                return false; 

            } 

            finally 

            { 

                Logger.LogVerbose(Message.RecognizerSaveImageReturn, filePrefix, savingType, sideToSave); 

            } 

        } 

 

 

		/// <summary> 

		/// Сохранить бинар последнего изображения при ошибке драйвера 

		/// </summary> 

		/// <param name="errorCode">код ошибки</param> 

        public void SaveLastImageOnDriverError(int errorCode) 

		{ 

            SaveLastImage("REVERSE_DRV", ImageSavingType.Binary, _onlineSide, errorCode); 

		} 

 

 

        /// <summary> 

        /// Получить путь к файлу, для сохранения информации об ошибке распознавания 

        /// </summary> 

        /// <param name="scannerSerialNumber"></param> 

        /// <returns></returns> 

        private string GetRecErrorFilePath(string scannerSerialNumber) 

        { 

            return string.Format(_recErrorFilePathFormat, DateTime.Now, _ocr.RunRecCount, scannerSerialNumber); 

        } 

 

 

        /// <summary> 

        /// Получить имя файла для сохранения изображения 

        /// </summary> 

        /// <param name="filePrefix">префикс имени файла</param> 

        /// <returns>StringBuilder, в который уже записано имя файла</returns> 

        private StringBuilder GetImageFileName(string filePrefix) 

        { 

            Logger.LogVerbose(Message.DebugVerbose, "call"); 

 


 
            var fileName = new StringBuilder(); 

 

 

            try 

            { 

                // Используется следующий принцип формирования имени файла с изображением: 

                // день, месяц, час и минута запуска программы, число запусков функции RunRecognize,  

                // номер сканера, номер маркера (если распознан), результат распознавания,  

                // признак режима тестирования, порог бинаризации для стороны 0, 

                // порог бинаризации для стороны 1 

 

 

                fileName.AppendFormat(_imageFileNamePrefixFormat,  

                    DateTime.Now, _ocr.RunRecCount, _scannerManager.SerialNumber); 

 

 

                // если номер бюллетеня правильный 

                if (_ocr.BulletinNumber >= 0) 

                { 

                    if (_ocr.BulletinNumber < _electionManager.SourceData.Blanks.Length) 

                    { 

                        // добавим маркер 

                        fileName.Append("_MarkerN"); 

                        fileName.Append(_electionManager.SourceData.Blanks[_ocr.BulletinNumber].Marker); 

                    } 

                    else 

                    { 

                        // не можем узнать маркер 

                        fileName.Append("_BulletinN"); 

                        fileName.Append(_ocr.BulletinNumber); 

                    } 

                } 

 

 

                // добавлю префикс 

                fileName.Append('_'); 

                fileName.Append(filePrefix); 

 

 

                // если режим тестирования, то добавляю в имя файла слово "TEST" 

                if (_recognitionStartVotingMode == VotingMode.Test) 

                { 

                    fileName.Append('_'); 

                    fileName.Append(_config.Blanks[BlankType.Test].ImageFilePrefix); 

                } 

 

 

                // добавлю пороги бинаризации 

                fileName.Append('_'); 


                fileName.Append(_scannerManager.BinarizationThresholdTop); 

                fileName.Append('_'); 

                fileName.Append(_scannerManager.BinarizationThresholdBottom); 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException( 

                    Message.RecognizerException, ex, "Ошибка получения имени файла для сохранения изображения"); 

            } 

 

 

            return fileName; 

        } 

 

 

		#endregion 

 

 

        #endregion 

 

 

        #region IOcrEventHandler Members 

 

 

        /// <summary> 

        /// Получить полутон 

        /// </summary> 

        /// <param name="ocr">pOCR</param> 

        /// <param name="side">Сторона</param> 

        /// <param name="x">Координата по X</param> 

        /// <param name="y">Координата по Y</param> 

        /// <param name="height">Высота</param> 

        /// <param name="width">Ширина</param> 

        /// <param name="image">Буфер для полутона</param> 

        /// <returns>Результат выполнения</returns> 

        public int GetHalfToneBuffer(IOcr ocr, short side, int x, int y, int height, int width, MemoryBlock image) 

        { 

            if (!_config.Ocr.GrayAnalysis.Enabled) 

                return -1; 

 

 

            try 

            { 

                var res = _scannerManager.GetHalftoneBuffer( 

                    (ScannedSide)side, (short)x, (short)y, (short)width, (short)height, image); 

 

 

				// если требуется сохраним изображение 

				if (res && _config.DebugImageSaving.Squares) 

				{ 


					long avalibleSize; 

					var imageDirectoryPath = _fileSystemManager.GetDataDirectoryPath(FileType.ScanningImage); 

 

 

					// размер вычислим как ширина на высоту + округлим сверху до 1024 байт 

					long requiredSize = 1024 * ((width * height + 1024 - 1) / 1024); 

 

 

					// если удастся выделить место на диске 

					if (_fileSystemManager.ReserveDiskSpace(FileType.ScanningImage, imageDirectoryPath, 

															requiredSize, requiredSize, out avalibleSize)) 

					{ 

						var filePathSB = GetImageFileName("SQ"); 

						// вставляю полный путь до каталога 

						filePathSB.Insert(0, imageDirectoryPath + '/'); 

						filePathSB.AppendFormat("_S{0}_X{1}_Y{2}_W{3}_H{4}.tif", side, x, y, width, height); 

 

 

						TiffImageHelper.SaveToFile(filePathSB.ToString(), ImageType.Halftone, image, width, height); 

					} 

					else 

						Logger.LogError(Message.RecognizerSaveImageNotEnoughFreeSpace, imageDirectoryPath, avalibleSize, requiredSize); 

				} 

 

 

                return res ? width * height : -1; 

            } 

            catch (Exception ex) 

            { 

                Logger.LogException(Message.RecognizerException, ex, "Ошибка получения буфера"); 

                return -1; 

            } 

        } 

 

 

        /// <summary> 

        /// Получить порог бинаризации 

        /// </summary> 

        /// <param name="ocr">pOCR</param> 

        /// <param name="side">Сторона</param> 

        /// <returns>порог бинаризации</returns> 

        public int GetBinaryThreshold(IOcr ocr, short side) 

        { 

            int res = side == 0 

                ? _scannerManager.BinarizationThresholdTop 

                : _scannerManager.BinarizationThresholdBottom; 

 

 

            Logger.LogVerbose(Message.RecognizerBinarizationThreshold, side, res); 

 


 
            return res; 

        } 

 

 

        /// <summary> 

        /// Передать ошибку распознавания 

        /// </summary> 

        /// <param name="ocr">pOCR</param> 

        /// <param name="errorCode">Код ошибки</param> 

        /// <param name="message">Сообщение</param> 

        public void Error(IOcr ocr, int errorCode, string message) 

        { 

            Logger.LogError(Message.RecognizerOcrError, errorCode, message); 

        } 

 

 

        /// <summary> 

        /// Передать отладочное сообщение 

        /// </summary> 

        /// <param name="ocr">pOCR</param> 

        /// <param name="message">Сообщение</param> 

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

        } 

 

 

        #endregion 

    } 

}



using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Bpc.Scanner; 

using Croc.Workflow.ComponentModel; 

using Croc.Bpc.Election.Voting; 

using Croc.Core.Extensions; 

using Croc.Bpc.Common.Diagnostics; 

using System.Threading; 

 

 

namespace Croc.Bpc.Workflow.Activities 

{ 

    /// <summary> 

    /// Базовое действие, которое включает и выключает сканирование и  

    /// обрабатывает события от менеджера сканера во время сканирования 

    /// </summary> 

    [Serializable] 

    public abstract class ScanningActivity : BpcCompositeActivity 

    { 

        /// <summary> 

        /// Инициализация 

        /// </summary> 

        /// <param name="context"></param> 

        protected override void Initialize(WorkflowExecutionContext context) 

        { 

            base.Initialize(context); 

 

 

            // подписываемся на события поступления и обработки листа 

            _scannerManager.NewSheetReceived += new EventHandler<SheetEventArgs>(ScannerManager_NewSheetReceived); 

            _scannerManager.SheetProcessed += new EventHandler<SheetEventArgs>(ScannerManager_SheetProcessed); 

        } 

 

 

        /// <summary> 

        /// Деинициализация 

        /// </summary> 

        /// <param name="context"></param> 

        protected override void Uninitialize(WorkflowExecutionContext context) 

        { 

            // останавливаем сканирование 

            StopScanning(context, null); 

 

 

            // отписываемся от событий 

            _scannerManager.NewSheetReceived -= new EventHandler<SheetEventArgs>(ScannerManager_NewSheetReceived); 

            _scannerManager.SheetProcessed -= new EventHandler<SheetEventArgs>(ScannerManager_SheetProcessed); 

 


 
            base.Uninitialize(context); 

        } 

 

 

        #region Включение/выключение сканирования 

 

 

        /// <summary> 

        /// Режим работы ламп во время нахождения в данном состоянии 

        /// </summary> 

        protected abstract ScannerLampsRegime LampsRegime { get; } 

 

 

        /// <summary> 

        /// Кол-во бюллетеней, которое нужно отображать 

        /// </summary> 

        public abstract int ReceivedBulletinsCount { get; } 

 

 

        /// <summary> 

        /// Кол-во бюллетеней, принятых в текущем режиме голосования больше нуля? 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey ReceivedBulletinsCountInCurrentModeMoreThenZero( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            var key = new VoteKey() 

            { 

                BlankType = BlankType.All, 

                VotingMode = _electionManager.CurrentVotingMode, 

                ScannerSerialNumber = _scannerManager.IntSerialNumber 

            }; 

 

 

            var count = _electionManager.VotingResults.VotesCount(key); 

            return count > 0 ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// Включение сканирования и зеленой лампочки 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey StartScanning( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 


        { 

            // включаем сканирование 

            _scannerManager.StartScanning(); 

            // установим режим работы ламп 

            _scannerManager.SetLampsRegime(LampsRegime); 

 

 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Выключение сканирования и зеленой лампочки 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey StopScanning( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // выключаем сканирование 

            _scannerManager.StopScanning(); 

            // выключим лампы 

            _scannerManager.SetLampsRegime(ScannerLampsRegime.BothOff); 

 

 

            return context.DefaultNextActivityKey; 

        } 

 

 

        #endregion 

 

 

        #region Обработка листа 

 

 

        /// <summary> 

        /// Признак того, что принятие бюллетеня разрешено 

        /// </summary> 

        private bool _bulletinReceivingAllowed = false; 

        /// <summary> 

        /// Результат голосования для последнего поступившего бюллетеня 

        /// </summary> 

        protected VotingResult _lastVotingResult; 

        /// <summary> 

        /// Ошибка при обработке последнего поступившего бюллетеня 

        /// </summary> 

        protected SheetProcessingError _lastError; 

        /// <summary> 

        /// Результат сброса последнего листа 


        /// </summary> 

        protected DropResult _lastDropResult; 

 

 

        /// <summary> 

        /// Событие "Лист обработан" 

        /// </summary> 

        [NonSerialized] 

        private ManualResetEvent _sheetProcessed = new ManualResetEvent(false); 

        /// <summary> 

        /// Событие "Лист обработан" 

        /// </summary> 

        public WaitHandle SheetProcessed 

        { 

            get 

            { 

                return _sheetProcessed; 

            } 

        } 

        /// <summary> 

        /// Имя действия-обработчика события "Поступил новый лист" 

        /// </summary> 

        protected abstract string NewSheetReceivedHandlerActivityName { get; } 

        /// <summary> 

        /// Можно ли принять бюллетень? 

        /// </summary> 

        /// <returns></returns> 

        protected abstract bool CanReceiveBulletin(); 

 

 

        /// <summary> 

        /// Обработка событие "Поступил новый лист" 

        /// </summary> 

        /// <param name="sender"></param> 

        /// <param name="e"></param> 

        private void ScannerManager_NewSheetReceived(object sender, SheetEventArgs e) 

        { 

            _logger.LogVerbose(Message.DebugVerbose, "call"); 

 

 

            // если есть разрешение на принятие или принимать бюллетень можно 

            if (_bulletinReceivingAllowed || CanReceiveBulletin()) 

            { 

                // сбросим разрешение на принятие, т.к. оно одноразовое (выдается на 1 бюллетень) 

                _bulletinReceivingAllowed = false; 

                // разрешим принятие бюллетеня 

                e.SheetProcessingSession.ReceivingAllowed = true; 

            } 

            else 

            { 


                // запретим принятие бюллетеня 

                e.SheetProcessingSession.ReceivingAllowed = false; 

            } 

 

 

            // сбросим событие "Лист обработан" 

            _sheetProcessed.Reset(); 

            // переходим к действию, в котором начнется обработка события "Поступил новый лист" 

            _workflowManager.GoToActivity(NewSheetReceivedHandlerActivityName, true); 

        } 

 

 

        /// <summary> 

        /// Подтверждение принятия бюллетеня  

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey ConfirmBulletinReceiving( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _bulletinReceivingAllowed = true; 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Обработка события "Лист обработан" 

        /// </summary> 

        /// <param name="sender"></param> 

        /// <param name="e"></param> 

        private void ScannerManager_SheetProcessed(object sender, SheetEventArgs e) 

        { 

            // сохранение результат распознавания и ошибку 

            _lastVotingResult = e.SheetProcessingSession.VotingResult; 

            _lastError = e.SheetProcessingSession.Error; 

            _lastDropResult = e.SheetProcessingSession.DropResult; 

 

 

            // если случилась ошибка подождем немного, так как (если ошибка сразу за NewSheet, например выдернули лист) 

            // NewSheetReceivedHandlerActivityName могло еще не начать ожидать события _sheetProcessed 

            // тогда ЖДИТЕ зависнет на индикаторе 

            if (_lastError != null) 

                Thread.Sleep(100); 

 

 

            // взводим событие "Лист обработан" 

            _sheetProcessed.Set(); 

        } 

 


 
        /// <summary> 

        /// Анализ результата распознавания бюллетеня  

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey CheckRecognitionResult( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            if (_lastError != null) 

            { 

                if (_lastError.Code == (int)LogicalReverseReason.SheetReceivingForbidden) 

                    return BpcNextActivityKeys_RecognitionResult.BulletinReceivingForbidden; 

 

 

                if (_lastError.IsReverseReason) 

                    return BpcNextActivityKeys_RecognitionResult.BulletinReversed; 

 

 

                return BpcNextActivityKeys_RecognitionResult.Error; 

            } 

 

 

            // если результат сброса реверс 

            if (_lastDropResult == DropResult.Reversed) 

                return BpcNextActivityKeys_RecognitionResult.BulletinReversed; 

 

 

            switch (_lastVotingResult.BlankType) 

            { 

                case BlankType.Valid: 

                    return BpcNextActivityKeys_RecognitionResult.ValidBulletin; 

 

 

                case BlankType.Bad: 

                    return BpcNextActivityKeys_RecognitionResult.BadBulletin; 

 

 

                case BlankType.TooManyMarks: 

                    return BpcNextActivityKeys_RecognitionResult.TooManyMarksBulletin; 

 

 

                case BlankType.NoMarks: 

                    return BpcNextActivityKeys_RecognitionResult.NoMarksBulletin; 

 

 

                default: 

                    _logger.LogError(Message.WorkflowUnknownBlankType, _lastVotingResult.BlankType); 

                    return BpcNextActivityKeys_RecognitionResult.BadBulletin; 


            } 

        } 

 

 

        #region Причина реверса/НУФ 

 

 

        private const string NO_REVERSE_REASON = "Причина реверса не определена"; 

 

 

        /// <summary> 

        /// Пути к звук. файлам для воспроизведения причины реверса 

        /// </summary> 

        public string[] ReverseReasonSound 

        { 

            get 

            { 

                // если не было ошибки и реверса 

                if (_lastError == null && _lastDropResult != DropResult.Reversed) 

                { 

                    _logger.LogWarning(Message.Information, NO_REVERSE_REASON); 

                    return new[] { _soundManager.StubSoundFileName }; 

                } 

 

 

                // если реверс из-за ошибки 

                if (_lastError != null) 

                    return new[] { _soundManager.GetSoundForReverseReason(_lastError.Code) }; 

 

 

                // если реверс из-за НУФ 

                if (_lastVotingResult.BlankType == BlankType.Bad) 

                    return BadBulletinReasonSounds; 

 

 

                // непонятный реверс 

                return new[] { _soundManager.StubSoundFileName }; 

            } 

        } 

 

 

        /// <summary> 

        /// Описание причины реверса 

        /// </summary> 

        public string ReverseReasonText 

        { 

            get 

            { 

                // если реверс из-за ошибки 

                if (_lastError != null) 


                    return _lastError.Description; 

 

 

                // если реверс из-за НУФ 

                if (_lastDropResult == DropResult.Reversed && _lastVotingResult.BlankType == BlankType.Bad) 

                    return BadBulletinReasonText; 

 

 

                return NO_REVERSE_REASON; 

            } 

        } 

 

 

        /// <summary> 

        /// Пути к звук. файлам для воспроизведения фразы 

        /// "Бюллетень неустановленной формы. [Причина НУФ]" 

        /// </summary> 

        public string[] BadBulletinReasonSounds 

        { 

            get 

            { 

                // если причину НУФ определить нельзя 

                if (_lastVotingResult == null) 

                    return _soundManager.GetSoundsForBadBulletinReason(null, null); 

 

 

                // если текущий режим голосования - Стационарное или Переносное голосование 

                if (_electionManager.CurrentVotingMode == VotingMode.Main || 

                    _electionManager.CurrentVotingMode == VotingMode.Portable) 

                { 

                    // почти не уточняем причину НУФ 

                    return new[]{ string.IsNullOrEmpty(_lastVotingResult.BadStampReason) 

                        ? _soundManager.BadBulletinSound // причина НУФ - не печать 

                        : _soundManager.StampNotRecognizedSound // причина НУФ - печать 

                    }; 

                } 

                else 

                { 

                    // если причина НУФ - не печать 

                    if (string.IsNullOrEmpty(_lastVotingResult.BadStampReason)) 

                        return _soundManager.GetSoundsForBadBulletinReason(_lastVotingResult.BadBulletinReason, null); 

 

 

                    // причина НУФ - печать 

                    return _soundManager.GetSoundsForBadBulletinReason( 

                        _lastVotingResult.BadBulletinReason, _lastVotingResult.BadStampReason); 

                } 

            } 

        } 

 


 
        private const string NO_DETAILED_BAD_BULLETIN_REASON = "Неустановленная форма бюллетеня"; 

        private const string NO_BAD_BULLETIN_REASON = "Причина НУФ не определена"; 

 

 

        /// <summary> 

        /// Описание причины НУФа 

        /// </summary> 

        public string BadBulletinReasonText 

        { 

            get 

            { 

                // если текущий режим голосования - Стационарное или Переносное голосование 

                if (_electionManager.CurrentVotingMode == VotingMode.Main || 

                    _electionManager.CurrentVotingMode == VotingMode.Portable) 

                    // не уточняем причину НУФ 

                    return NO_DETAILED_BAD_BULLETIN_REASON; 

 

 

                return (_lastVotingResult == null || String.IsNullOrEmpty(_lastVotingResult.BadBulletinReason)) 

                    ? NO_BAD_BULLETIN_REASON 

                    : _soundManager.GetTextForBadBulletinReason(_lastVotingResult.BadBulletinReason); 

            } 

        } 

        #endregion 

 

 

        #endregion 

    } 

}



using System; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Scanner; 
using Croc.Core.Utils.Threading; 
using Croc.Workflow.ComponentModel; 
using System.Threading; 
using Croc.Bpc.Voting; 
namespace Croc.Bpc.Workflow.Activities 
{ 
    [Serializable] 
    public abstract class ScanningActivity : BpcCompositeActivity 
    { 
        protected override void Initialize(WorkflowExecutionContext context) 
        { 
            base.Initialize(context); 
            _sheetProcessed = new EventWaitHandleEx(false, true, this); 
            _scannerManager.NewSheetReceived += ScannerManager_NewSheetReceived; 
            _scannerManager.SheetProcessed += ScannerManager_SheetProcessed; 
        } 
        protected override void Uninitialize(WorkflowExecutionContext context) 
        { 
            _scannerManager.StopScanning(); 
            _scannerManager.NewSheetReceived -= ScannerManager_NewSheetReceived; 
            _scannerManager.SheetProcessed -= ScannerManager_SheetProcessed; 
            _sheetProcessed.Dispose(); 
            base.Uninitialize(context); 
        } 
        #region Включение/выключение сканирования 
        protected abstract ScannerLampsRegime LampsRegime { get; } 
        public int ReceivedBulletinsCount 
        { 
            get 
            { 
                return _votingResultManager.VotingResults.GetCounterValue(_scannerManager.IntSerialNumber); 
            } 
        } 
        public NextActivityKey ReceivedBulletinsCountInCurrentModeMoreThenZero( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var key = new VoteKey 
            { 
                BlankType = BlankType.All, 
                VotingMode = _electionManager.CurrentVotingMode, 
                ScannerSerialNumber = _scannerManager.IntSerialNumber 
            }; 
            var count = _votingResultManager.VotingResults.VotesCount(key); 
            return count > 0 ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey StartScanning( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _scannerManager.StartScanning(LampsRegime); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey StopScanning( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _scannerManager.StopScanning(); 
            return context.DefaultNextActivityKey; 
        } 
        #endregion 
        #region Обработка листа 
        private bool _bulletinReceivingAllowed; 
        protected VotingResult _lastVotingResult; 
        protected SheetProcessingError _lastError; 
        protected DropResult _lastDropResult; 
        protected SheetType _lastSheetType; 
        [NonSerialized] 
        private EventWaitHandleEx _sheetProcessed; 
        public WaitHandle SheetProcessed 
        { 
            get 
            { 
                return _sheetProcessed; 
            } 
        } 
        protected abstract string NewSheetReceivedHandlerActivityName { get; } 
        protected abstract bool CanReceiveBulletin(); 
        private void ScannerManager_NewSheetReceived(object sender, SheetEventArgs e) 
        { 
            _logger.LogVerbose(Message.Common_DebugCall); 
            if (_bulletinReceivingAllowed || CanReceiveBulletin()) 
            { 
                _bulletinReceivingAllowed = false; 
                e.SheetProcessingSession.ReceivingAllowed = true; 
            } 
            else 
            { 
                e.SheetProcessingSession.ReceivingAllowed = false; 
            } 
            _sheetProcessed.GetAccess(this); 
            _sheetProcessed.Reset(); 
            _workflowManager.GoToActivity(NewSheetReceivedHandlerActivityName); 
        } 
        public NextActivityKey ConfirmBulletinReceiving( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _bulletinReceivingAllowed = true; 
            return context.DefaultNextActivityKey; 
        } 
        private void ScannerManager_SheetProcessed(object sender, SheetEventArgs e) 
        { 
            _lastVotingResult = e.SheetProcessingSession.VotingResult; 
            _lastError = e.SheetProcessingSession.Error; 
            _lastDropResult = e.SheetProcessingSession.DropResult; 
            _lastSheetType = e.SheetProcessingSession.SheetType; 
            _sheetProcessed.GetAccess(this); 
            _sheetProcessed.Set(); 
        } 
        public NextActivityKey CheckRecognitionResult( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (_lastError != null) 
            { 
                if (_lastError.Code == (int) LogicalReverseReason.SheetReceivingForbidden) 
                    return BpcNextActivityKeys_RecognitionResult.BulletinReceivingForbidden; 
                return _lastError.IsReverseReason 
                           ? BpcNextActivityKeys_RecognitionResult.BulletinReversed 
                           : BpcNextActivityKeys_RecognitionResult.Error; 
            } 
            if (_lastDropResult == DropResult.Reversed) 
                return BpcNextActivityKeys_RecognitionResult.BulletinReversed; 
            if (_lastDropResult == DropResult.ProbablyDropped && IsCurrentVotingModeMainOrPortable()) 
            { 
                return BpcNextActivityKeys_RecognitionResult.ProbablyDropped; 
            } 
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
        private bool IsCurrentVotingModeMainOrPortable() 
        { 
            return _electionManager.CurrentVotingMode == VotingMode.Main || 
                   _electionManager.CurrentVotingMode == VotingMode.Portable; 
        } 
        #region Причина реверса/НУФ 
        private const string NO_REVERSE_REASON = "Причина реверса не определена"; 
        private const string BADMODE_REVERSE_REASON = "Режим запрещен!"; 
        private const string INVALIDBLANKNUMBER_REVERSE_REASON = "Недопустимый тип"; 
        public string[] ReverseReasonSound 
        { 
            get 
            { 
                if (_lastError != null) 
                { 
                    switch (_lastError.Code) 
                    { 
                        case (int)LogicalReverseReason.BlankHasNoCurrentVoteRegime: 
                            return new[] { _soundManager.BadModeReverseReasonSound }; 
                        case (int)LogicalReverseReason.InvalidBlankNumber: 
                            return IsCurrentVotingModeMainOrPortable() 
                                ? _soundManager.GetSoundsForBadBulletinReason(null, null) 
                                : new[] { _soundManager.InvalidBlankNumberReverseReasonSound }; 
                        default: 
                            return new[] { _soundManager.GetSoundForReverseReason(_lastError.Code) }; 
                    } 
                } 
                if (_lastDropResult == DropResult.Reversed && 
                    _lastVotingResult.BlankType == BlankType.Bad) 
                { 
                    return BadBulletinReasonSounds; 
                } 
                _logger.LogWarning(Message.WorkflowReverseReasonUndefined, _lastDropResult, _lastVotingResult); 
                return new[] { _soundManager.StubSoundFileName }; 
            } 
        } 
        public string ReverseReasonText 
        { 
            get 
            { 
                if (_lastError != null) 
                { 
                    switch (_lastError.Code) 
                    { 
                        case (int)LogicalReverseReason.BlankHasNoCurrentVoteRegime: 
                            return BADMODE_REVERSE_REASON; 
                        case (int)LogicalReverseReason.InvalidBlankNumber: 
                            return IsCurrentVotingModeMainOrPortable() 
                                ? NO_DETAILED_BAD_BULLETIN_REASON 
                                : INVALIDBLANKNUMBER_REVERSE_REASON; 
                        default: 
                            return _lastError.Description; 
                    } 
                } 
                return _lastDropResult == DropResult.Reversed && 
                       _lastVotingResult.BlankType == BlankType.Bad 
                           ? BadBulletinReasonText 
                           : NO_REVERSE_REASON; 
            } 
        } 
        public string[] BadBulletinReasonSounds 
        { 
            get 
            { 
                if (_lastSheetType == SheetType.Long) 
                    return new[] {_soundManager.LongBadBulletinSound}; 
                if (_lastVotingResult == null) 
                    return _soundManager.GetSoundsForBadBulletinReason(null, null); 
                if (IsCurrentVotingModeMainOrPortable()) 
                { 
                    return new[]{ string.IsNullOrEmpty(_lastVotingResult.BadStampReason) 
                        ? _soundManager.BadBulletinSound // причина НУФ - не печать 
                        : _soundManager.StampNotRecognizedSound // причина НУФ - печать 
                    }; 
                } 
                if (string.IsNullOrEmpty(_lastVotingResult.BadStampReason)) 
                    return _soundManager.GetSoundsForBadBulletinReason(_lastVotingResult.BadBulletinReason, null); 
                return _soundManager.GetSoundsForBadBulletinReason( 
                    _lastVotingResult.BadBulletinReason, _lastVotingResult.BadStampReason); 
            } 
        } 
        private const string LONG_BAD_BULLETIN_REASON =  
            "Слишком длинный лист, не придерживайте бюллетени и опускайте их по одному"; 
        private const string NO_DETAILED_BAD_BULLETIN_REASON = "Неустановленная форма бюллетеня"; 
        private const string NO_BAD_BULLETIN_REASON = "Причина НУФ не определена"; 
        public string BadBulletinReasonText 
        { 
            get 
            { 
                if (_lastSheetType == SheetType.Long) 
                    return LONG_BAD_BULLETIN_REASON; 
                if (IsCurrentVotingModeMainOrPortable()) 
                    return NO_DETAILED_BAD_BULLETIN_REASON; 
                return (_lastVotingResult == null || String.IsNullOrEmpty(_lastVotingResult.BadBulletinReason)) 
                    ? NO_BAD_BULLETIN_REASON 
                    : GetTextForBadBulletinReason(_lastVotingResult.BadBulletinReason); 
            } 
        } 
        public static string GetTextForBadBulletinReason(string badBulletinReason) 
        { 
            switch (badBulletinReason) 
            { 
                case "Marker": 
                    return "НУФ: маркер"; 
                case "Lines": 
                    return "НУФ: линии"; 
                case "Squares": 
                    return "НУФ: квадраты"; 
                case "Stamp": 
                    return "НУФ: печать"; 
                case "Refp": 
                    return "НУФ: точки"; 
                default: 
                    return "НУФ"; 
            } 
        } 
        #endregion 
        #endregion 
    } 
}

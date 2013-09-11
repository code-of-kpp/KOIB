using System; 
using System.Collections.Generic; 
using System.Text; 
using Croc.Bpc.Scanner; 
using Croc.Bpc.Voting; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.Testing 
{ 
    [Serializable] 
    public class TestingActivity : ScanningActivity 
    { 
        public NextActivityKey SetVotingModeToTest( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _electionManager.CurrentVotingMode = VotingMode.Test; 
            _votingResultManager.VotingResults.SetCounterValueKeys( 
                new[] 
                    { 
                        new VoteKey 
                            { 
                                VotingMode = VotingMode.Test, 
                                BlankType = BlankType.All 
                            } 
                    }); 
            return context.DefaultNextActivityKey; 
        } 
        #region Включение/выключение сканирования 
        protected override ScannerLampsRegime LampsRegime 
        { 
            get 
            { 
                return ScannerLampsRegime.GreenOn; 
            } 
        } 
        #endregion 
        #region Обработка листа 
        protected override string NewSheetReceivedHandlerActivityName 
        { 
            get 
            { 
                return "TestingActivity.WaitSheetProcessed"; 
            } 
        } 
        protected override bool CanReceiveBulletin() 
        { 
            return !(// нельзя, если 
                _electionManager.IsElectionDay() == ElectionDayСomming.ItsElectionDay && 
                _electionManager.SourceData.ElectionMode == ElectionMode.Real && 
                _electionManager.SourceData.IsVotingModeExpired(VotingMode.Test)); 
        } 
        public int StampNumber 
        { 
            get 
            { 
                int stampNumber; 
                if (_lastVotingResult == null || !int.TryParse(_lastVotingResult.StampNumber, out stampNumber)) 
                    return 0; 
                return stampNumber; 
            } 
        } 
        #endregion 
        #region Подготовка параметров для воспроизведения отмеченных позиций 
        public string PhraseTextFormat 
        { 
            get; 
            private set; 
        } 
        public string PhraseFormat 
        { 
            get; 
            private set; 
        } 
        public string[] PhraseSounds 
        { 
            get; 
            private set; 
        } 
        public object[] PhraseParams 
        { 
            get; 
            private set; 
        } 
        public NextActivityKey PrepareParamsForSaySelectedMarks( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (_lastVotingResult == null || 
                _lastVotingResult.SectionsMarks == null) 
            { 
                PhraseTextFormat = ""; 
                PhraseFormat = ""; 
                PhraseSounds = new string[] { }; 
                PhraseParams = new object[] { }; 
                return context.DefaultNextActivityKey; 
            } 


            var textFormatSb = new StringBuilder(parameters.GetParamValue("Text", "")); 
            var phraseFormatSb = new StringBuilder(); 
            var phraseParams = new List<object>(); 
            var sounds = new List<string>(); 
            var firstSound = parameters.GetParamValue<string>("Sound", null); 
            if (firstSound != null) 
            { 
                phraseFormatSb.Append("{s0}"); 
                sounds.Add(firstSound); 
            } 
            if (_lastVotingResult.SectionsMarks.Length == 1) 
            { 
                GetPositionsInSection(_lastVotingResult.SectionsMarks[0], false, 
                    textFormatSb, phraseFormatSb, sounds, phraseParams); 
            } 
            else 
            { 
                int sectionNum = 1; 
                foreach (var section in _lastVotingResult.SectionsMarks) 
                { 
                    textFormatSb.AppendFormat("Секция {{p{0}}}: ", phraseParams.Count); 
                    phraseFormatSb.AppendFormat("{{s{0}}}{{p{1}}}", sounds.Count, phraseParams.Count); 
                    sounds.Add("section"); 
                    phraseParams.Add(sectionNum++); 
                    GetPositionsInSection(section, true, textFormatSb, phraseFormatSb, sounds, phraseParams); 
                    textFormatSb.Append("; "); 
                } 
                if (textFormatSb.Length > 0) 
                    textFormatSb.Length -= 2; 
            } 
            PhraseTextFormat = textFormatSb.ToString(); 
            PhraseFormat = phraseFormatSb.ToString(); 
            PhraseSounds = sounds.ToArray(); 
            PhraseParams = phraseParams.ToArray(); 
            return context.DefaultNextActivityKey; 
        } 
        private static void GetPositionsInSection( 
            int[] marksInSection, bool addPositionsWord, 
            StringBuilder textFormatSb, StringBuilder phraseFormatSb,  
            List<string> sounds, List<object> phraseParams) 
        { 
            if (marksInSection.Length == 0) 
            { 
                textFormatSb.Append("нет отметок"); 
                phraseFormatSb.AppendFormat("{{s{0}}}", sounds.Count); 
                sounds.Add("no_marks"); 
                return; 
            } 
            if (addPositionsWord) 
            { 
                textFormatSb.Append("позиции "); 
                phraseFormatSb.AppendFormat("{{s{0}}}", sounds.Count); 
                sounds.Add("positions"); 
            } 
            foreach (var markPos in marksInSection) 
            { 
                textFormatSb.AppendFormat("{{p{0}}},", phraseParams.Count); 
                phraseFormatSb.AppendFormat("{{p{0}}}", phraseParams.Count); 
                phraseParams.Add(markPos + 1); 
            } 
            if (textFormatSb.Length > 0) 
                textFormatSb.Length -= 1; 
        } 
        #endregion 
    } 
}

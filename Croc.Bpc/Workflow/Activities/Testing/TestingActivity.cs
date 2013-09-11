using System; 

using System.Collections.Generic; 

using System.Text; 

using Croc.Bpc.Election.Voting; 

using Croc.Bpc.Scanner; 

using Croc.Workflow.ComponentModel; 

using Croc.Core.Extensions; 

using Croc.Bpc.Common.Diagnostics; 

 

 

namespace Croc.Bpc.Workflow.Activities.Testing 

{ 

    /// <summary> 

    /// Режим тестирования 

    /// </summary> 

    [Serializable] 

    public class TestingActivity : ScanningActivity 

    {         

        /// <summary> 

        /// Установка режима голосования = Тестирование 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey SetVotingModeToTest( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _electionManager.CurrentVotingMode = VotingMode.Test; 

            return context.DefaultNextActivityKey; 

        } 

 

 

        #region Включение/выключение сканирования 

 

 

        /// <summary> 

        /// Режим работы ламп во время нахождения в данном состоянии 

        /// </summary> 

        protected override ScannerLampsRegime LampsRegime 

        { 

            get 

            { 

                return ScannerLampsRegime.GreenOn; 

            } 

        } 

 

 

        /// <summary> 

        /// Кол-во бюллетеней, принятых в тестовом режиме 

        /// </summary> 


        public override int ReceivedBulletinsCount 

        { 

            get 

            { 

                var key = new VoteKey() 

                { 

                    VotingMode = VotingMode.Test, 

                    BlankType = BlankType.All, 

                    ScannerSerialNumber = _scannerManager.IntSerialNumber 

                }; 

 

 

                return _electionManager.VotingResults.VotesCount(key); 

            } 

        } 

 

 

        #endregion 

 

 

        #region Проверка даты и времени голосования 

 

 

        /// <summary> 

        /// Кол-во дней, которое осталось до начала голосования 

        /// </summary> 

        public int DaysToVotingStart 

        { 

            get 

            { 

                var date = _electionManager.SourceData.ElectionDate.Date - 

                    _electionManager.SourceData.LocalTimeNow.Date; 

 

 

                return date.Days; 

            } 

        } 

 

 

        /// <summary> 

        /// Кол-во времени (часов + минут), которое осталось до начала голосования 

        /// без учета текущего дня (т.е. предполагается, что сейчас день голосования) 

        /// </summary> 

        public TimeSpan TimeToVotingStart 

        { 

            get 

            { 

                var now = _electionManager.SourceData.LocalTimeNow; 

                var nowTime = now - now.Date; 

 


 
                var timeToStart = _electionManager.SourceData.GetVotingModeStartTime(VotingMode.Main) - nowTime; 

 

 

                if (timeToStart < TimeSpan.Zero) 

                    return TimeSpan.Zero; 

 

 

                return timeToStart; 

            } 

        } 

 

 

        /// <summary> 

        /// Проверяет дату голосования 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey CheckVotingDate( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            var daysRemaining = DaysToVotingStart; 

 

 

            if (daysRemaining == 0) 

                return BpcNextActivityKeys_VotingTime.ElectionDayNow; 

 

 

            if (daysRemaining > 0) 

                return BpcNextActivityKeys_VotingTime.ElectionDayHasNotCome; 

 

 

            return BpcNextActivityKeys_VotingTime.ElectionDayPassed; 

        } 

 

 

        /// <summary> 

        /// Проверяет время голосования 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey CheckVotingTime( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // если сейчас не день голосования 

            if (!_electionManager.SourceData.IsElectionDay) 

                return BpcNextActivityKeys_VotingTime.NotVotingTime; 

 


 
            if (TimeToVotingStart.Ticks > 0) 

                return BpcNextActivityKeys_VotingTime.SomeTimeToVotingStart; 

 

 

            if (_electionManager.SourceData.IsVotingModeTime(VotingMode.Main)) 

                return BpcNextActivityKeys_VotingTime.VotingTimeNow; 

 

 

            return BpcNextActivityKeys_VotingTime.NotVotingTime; 

        } 

 

 

        #endregion 

 

 

        #region Обработка листа 

 

 

        /// <summary> 

        /// Имя действия-обработчика события "Поступил новый лист" 

        /// </summary> 

        protected override string NewSheetReceivedHandlerActivityName 

        { 

            get 

            { 

				return "TestingActivity.WaitSheetProcessed"; 

            } 

        } 

 

 

        /// <summary> 

        /// Можно ли принять бюллетень? 

        /// </summary> 

        /// <returns></returns> 

        protected override bool CanReceiveBulletin() 

        { 

            return !(// нельзя, если 

                // сейчас День выборов И 

                _electionManager.SourceData.IsElectionDay && 

                // режим выборов == Боевой И 

                _electionManager.SourceData.ElectionMode == ElectionMode.Real && 

                // время Тестового режима голосования истекло 

                _electionManager.SourceData.IsVotingModeExpired(VotingMode.Test)); 

        } 

 

 

        /// <summary> 

        /// Номер печати 

        /// </summary> 


        public int StampNumber 

        { 

            get 

            { 

                int stampNumber = 0; 

                if (_lastVotingResult == null || !int.TryParse(_lastVotingResult.StampNumber, out stampNumber)) 

                    return 0; 

 

 

                return stampNumber; 

            } 

        } 

 

 

        #endregion 

 

 

        #region Подготовка параметров для воспроизведения отмеченных позиций 

 

 

        /// <summary> 

        /// Формат-строка текста 

        /// </summary> 

        public string PhraseTextFormat 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Формат-строка фразы 

        /// </summary> 

        public string PhraseFormat 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Пути к звук. файлам 

        /// </summary> 

        public string[] PhraseSounds 

        { 

            get; 

            private set; 

        } 

 

 


        /// <summary> 

        /// Параметры фразы 

        /// </summary> 

        public object[] PhraseParams 

        { 

            get; 

            private set; 

        } 

 

 

        /// <summary> 

        /// Подготавливает параметры фразы для действий, который 

        /// воспроизводят номера отмеченных позиций 

        /// </summary> 

        /// <remarks> 

        /// Параметры действия: 

        ///     Text        - задает текст, который будет вставлен в начало результативной формат-строки текста 

        ///     Sound       - задает путь к звук. файлу, который будет вставлен в начало результативной фразы 

        ///  

        /// Результативная фраза будет иметь вид: 

        ///     - если несколько секций:  "[Text] Секция 1: позиции: 1, 2..., Секция 2: позиции: 1, 2..." 

        ///     - если 1 секция:          "[Text] 1, 2..." 

        /// </remarks> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey PrepareParamsForSaySelectedMarks( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // если нет результата голосования или отметок 

            if (_lastVotingResult == null || 

                _lastVotingResult.SectionsMarks == null) 

            { 

                PhraseTextFormat = ""; 

                PhraseFormat = ""; 

                PhraseSounds = new string[] { }; 

                PhraseParams = new object[] { }; 

 

 

                return context.DefaultNextActivityKey; 

            } 

 

 

            var textFormatSB = new StringBuilder(parameters.GetParamValue("Text", "")); 

            var phraseFormatSB = new StringBuilder(); 

            var phraseParams = new List<object>(); 

            var sounds = new List<string>(); 

 

 

            var firstSound = parameters.GetParamValue<string>("Sound", null); 


            if (firstSound != null) 

            { 

                phraseFormatSB.Append("{s0}"); 

                sounds.Add(firstSound); 

            } 

 

 

            // если только 1 секция 

            if (_lastVotingResult.SectionsMarks.Length == 1) 

            { 

                GetPositionsInSection(_lastVotingResult.SectionsMarks[0], false, 

                    textFormatSB, phraseFormatSB, sounds, phraseParams); 

            } 

            // если несколько секций 

            else 

            { 

                int sectionNum = 1; 

                foreach (var section in _lastVotingResult.SectionsMarks) 

                { 

                    textFormatSB.AppendFormat("Секция {{p{0}}}: ", phraseParams.Count); 

                    phraseFormatSB.AppendFormat("{{s{0}}}{{p{1}}}", sounds.Count, phraseParams.Count); 

                    sounds.Add("section"); 

                    phraseParams.Add(sectionNum++); 

 

 

                    GetPositionsInSection(section, true, textFormatSB, phraseFormatSB, sounds, phraseParams); 

 

 

                    textFormatSB.Append("; "); 

                } 

 

 

                // отрежем последний разделитель "; " 

                if (textFormatSB.Length > 0) 

                    textFormatSB.Length -= 2; 

            } 

 

 

            PhraseTextFormat = textFormatSB.ToString(); 

            PhraseFormat = phraseFormatSB.ToString(); 

            PhraseSounds = sounds.ToArray(); 

            PhraseParams = phraseParams.ToArray(); 

 

 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Формирует часть фразы для проговаривания позиций в 1 секции 


        /// </summary> 

        /// <param name="marksInSection"></param> 

        /// <param name="addPositionsWord"></param> 

        /// <param name="textFormatSB"></param> 

        /// <param name="phraseFormatSB"></param> 

        /// <param name="sounds"></param> 

        /// <param name="phraseParams"></param> 

        private void GetPositionsInSection( 

            int[] marksInSection, bool addPositionsWord, 

            StringBuilder textFormatSB, StringBuilder phraseFormatSB, List<string> sounds, List<object> phraseParams) 

        { 

            if (marksInSection.Length == 0) 

            { 

                textFormatSB.Append("нет отметок"); 

                phraseFormatSB.AppendFormat("{{s{0}}}", sounds.Count); 

                sounds.Add("no_marks"); 

                return; 

            } 

 

 

            if (addPositionsWord) 

            { 

                textFormatSB.Append("позиции "); 

                phraseFormatSB.AppendFormat("{{s{0}}}", sounds.Count); 

                sounds.Add("positions"); 

            } 

 

 

            // пройдемся по отмеченным позициям 

            foreach (var markPos in marksInSection) 

            { 

                textFormatSB.AppendFormat("{{p{0}}},", phraseParams.Count); 

                phraseFormatSB.AppendFormat("{{p{0}}}", phraseParams.Count); 

                phraseParams.Add(markPos + 1); 

            } 

 

 

            // отрежем последний разделитель "," 

            if (textFormatSB.Length > 0) 

                textFormatSB.Length -= 1; 

        } 

 

 

        #endregion 

    } 

}



using System; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Bpc.Election.Voting; 

using Croc.Bpc.Scanner; 

using Croc.Workflow.ComponentModel; 

using Croc.Core.Extensions; 

 

 

 

 

namespace Croc.Bpc.Workflow.Activities.Voting 

{ 

    /// <summary> 

    /// Голосование в стационарном режиме 

    /// </summary> 

    [Serializable] 

    public class MainVotingActivity : ScanningActivity 

    { 

        /// <summary> 

        /// Установка режима голосования = Стационарный 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey SetVotingModeToMain( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            if (_electionManager.CurrentVotingMode != VotingMode.Main) 

                _electionManager.CurrentVotingMode = VotingMode.Main; 

 

 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Проверяет, можно ли перейти в Переносной режим голосования 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey CanGoToPortableVotingMode( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // переходить нельзя, если: 

            return 

                // сейчас День выборов И 

                _electionManager.SourceData.IsElectionDay && 

                // режим выборов == Боевой И 

                _electionManager.SourceData.ElectionMode == ElectionMode.Real && 


                // время Стационарного режима голосования еще не истекло 

                !_electionManager.SourceData.IsVotingModeExpired(VotingMode.Main) 

 

 

                ? BpcNextActivityKeys.No : BpcNextActivityKeys.Yes; 

        } 

 

 

        /// <summary> 

        /// Время начала Переносного режима голосования 

        /// </summary> 

        public TimeSpan PortableVotingModeStartTime 

        { 

            get 

            { 

                return _electionManager.SourceData.GetVotingModeStartTime(VotingMode.Portable); 

            } 

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

        /// Кол-во бюллетеней, принятых в стационарном и переносном режимах 

        /// </summary> 

        public override int ReceivedBulletinsCount 

        { 

            get 

            { 

                // режим голосования не задаем, чтобы учесть 

                // и стационарный и переносной режимы  

                // (а результаты тестового режима должны были почиститься при переходе в стационарный) 

                var key = new VoteKey() 

                { 

                    BlankType = BlankType.All, 

                    ScannerSerialNumber = _scannerManager.IntSerialNumber 

                }; 


 
 

                return _electionManager.VotingResults.VotesCount(key); 

            } 

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

				return "MainVotingActivity.WaitSheetProcessed"; 

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

                // время Стационарного режима голосования истекло 

                _electionManager.SourceData.IsVotingModeExpired(VotingMode.Main)); 

        } 

 

 

        #endregion 

    } 

}



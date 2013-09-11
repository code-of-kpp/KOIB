using System; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Bpc.Election.Voting; 

using Croc.Bpc.Scanner; 

using Croc.Workflow.ComponentModel; 

using Croc.Core.Extensions; 

 

 

 

 

namespace Croc.Bpc.Workflow.Activities.Voting 

{ 

    /// <summary> 

    /// Голосование в переносном режиме 

    /// </summary> 

    [Serializable] 

    public class PortableVotingActivity : ScanningActivity 

    { 

        /// <summary> 

        /// Установка режима голосования = Переносной 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey SetVotingModeToPortable( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            if (_electionManager.CurrentVotingMode != VotingMode.Portable) 

                _electionManager.CurrentVotingMode = VotingMode.Portable; 

 

 

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

                return ScannerLampsRegime.GreenBlinking; 

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

				return "PortableVotingActivity.WaitSheetProcessed"; 

            } 

        } 

 

 

        /// <summary> 

        /// Можно ли принять бюллетень? 

        /// </summary> 

        /// <returns></returns> 

        protected override bool CanReceiveBulletin() 

        { 

            return true; 

        } 

 

 


        #endregion 

    } 

}



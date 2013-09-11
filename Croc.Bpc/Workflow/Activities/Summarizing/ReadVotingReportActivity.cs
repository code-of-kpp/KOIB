using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Workflow.ComponentModel; 

using Croc.Core.Diagnostics; 

using Croc.Bpc.Election.Voting; 

 

 

namespace Croc.Bpc.Workflow.Activities.Summarizing 

{ 

    /// <summary> 

    /// Зачитывание протокола голосования 

    /// </summary> 

    [Serializable] 

    public class ReadVotingReportActivity : ElectionParametrizedActivity 

    { 

        /// <summary> 

        /// Общее кол-во бюллетеней по текущим выборам 

        /// </summary> 

        public int TotalBulletinCount 

        { 

            get 

            { 

                var key = new VoteKey() 

                { 

                    BlankType = BlankType.AllButBad, 

                    BlankId = BlankId 

                }; 

 

 

                return _electionManager.VotingResults.VotesCount(key); 

            } 

        } 

 

 

        /// <summary> 

        /// Кол-во валидных бюллетеней по текущим выборам 

        /// </summary> 

        public int ValidBulletinCount 

        { 

            get 

            { 

                var key = new VoteKey() 

                { 

                    BlankType = BlankType.Valid, 

                    BlankId = BlankId 

                }; 

 


 
                return _electionManager.VotingResults.VotesCount(key); 

            } 

        } 

 

 

        /// <summary> 

        /// Кол-во не валидных бюллетеней по текущим выборам 

        /// </summary> 

        public int NotValidBulletinCount 

        { 

            get 

            { 

                var key = new VoteKey() 

                { 

                    BlankType = BlankType.NotValid, 

                    BlankId = BlankId 

                }; 

 

 

                return _electionManager.VotingResults.VotesCount(key); 

            } 

        } 

 

 

        /// <summary> 

        /// Кол-во бюллетеней, обработанных в стационарном режиме 

        /// </summary> 

        public int ProcessedInMainBulletinCount 

        { 

            get 

            { 

                var key = new VoteKey() 

                { 

                    VotingMode = VotingMode.Main, 

                    BlankType = BlankType.AllButBad, 

                    BlankId = BlankId 

                }; 

 

 

                return _electionManager.VotingResults.VotesCount(key); 

            } 

        } 

 

 

        /// <summary> 

        /// Кол-во бюллетеней, обработанных в переносном режиме 

        /// </summary> 

        public int ProcessedInPortableBulletinCount 

        { 


            get 

            { 

                var key = new VoteKey() 

                { 

                    VotingMode = VotingMode.Portable, 

                    BlankType = BlankType.AllButBad, 

                    BlankId = BlankId 

                }; 

 

 

                return _electionManager.VotingResults.VotesCount(key); 

            } 

        } 

    } 

}



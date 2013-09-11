using System; 

using System.Collections.Specialized; 

using Croc.Bpc.Election.Voting; 

 

 

namespace Croc.Bpc.Workflow.Activities.Testing 

{ 

    /// <summary> 

    /// Зачитывание протокола тестирования 

    /// </summary> 

    [Serializable] 

    public class ReadTestingReportActivity : ElectionEnumeratorActivity 

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

                    VotingMode = VotingMode.Test, 

                    BlankType = BlankType.AllButBad, 

                    BlankId = _currentBlankId 

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

                    VotingMode = VotingMode.Test, 

                    BlankType = BlankType.Valid, 

                    BlankId = _currentBlankId 

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

                    VotingMode = VotingMode.Test, 

                    BlankType = BlankType.NotValid, 

                    BlankId = _currentBlankId 

                }; 

 

 

                return _electionManager.VotingResults.VotesCount(key); 

            } 

        } 

 

 

		/// <summary> 

		/// Параметры для печати тестового протокола 

		/// </summary> 

		public ListDictionary TestResultsPrintParameters 

		{ 

			get  

			{ 

				var parameters = new ListDictionary(); 

				// параметр для печати тестового протокола 

				parameters.Add("test", true); 

				return parameters; 

			} 

		} 

 

 

        /// <summary> 

        /// Кол-во НУФ по текущим выборам 

        /// </summary> 

        public int BadBulletinCount 

        { 

            get 

            { 

                var key = new VoteKey() 

                { 

                    VotingMode = VotingMode.Test, 

                    BlankType = BlankType.Bad, 

                    BlankId = _currentBlankId 


                }; 

 

 

                return _electionManager.VotingResults.VotesCount(key); 

            } 

        } 

    } 

}



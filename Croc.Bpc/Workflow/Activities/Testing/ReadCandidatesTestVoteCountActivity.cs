using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Workflow.ComponentModel; 

using Croc.Core.Diagnostics; 

using Croc.Bpc.Election.Voting; 

 

 

namespace Croc.Bpc.Workflow.Activities.Testing 

{ 

    /// <summary> 

    /// Зачитывание количества голосов по каждому кандидату (позиции) для заданных выборов в режиме Тестирования 

    /// </summary> 

    [Serializable] 

    public class ReadCandidatesTestVoteCountActivity : CandidateEnumeratorActivity 

    { 

        /// <summary> 

        /// Кол-во голосов, отданные за текущего кандидата 

        /// </summary> 

        public int CandidateVoteCount 

        { 

            get 

            { 

                var key = new VoteKey() 

                { 

                    VotingMode = VotingMode.Test, 

                    CandidateId = _currentCandidate.Id, 

                    ElectionNum = Election.ElectionId 

                }; 

 

 

                return _electionManager.VotingResults.VotesCount(key); 

            } 

        } 

 

 

        /// <summary> 

        /// Проверяет снят ли текущий кандидат 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey IsCandidateCanceled( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _currentCandidate.Disabled ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

    } 


}



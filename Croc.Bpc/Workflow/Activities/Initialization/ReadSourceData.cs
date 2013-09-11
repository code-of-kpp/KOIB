using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Workflow.ComponentModel; 

using Croc.Core.Diagnostics; 

using Croc.Bpc.Election; 

using Croc.Bpc.Election.Voting; 

using System.Collections.Specialized; 

 

 

namespace Croc.Bpc.Workflow.Activities.Initialization 

{ 

    [Serializable] 

    public class ReadSourceDataActivity : ElectionEnumeratorActivity 

    { 

        /// <summary> 

        /// Номер УИК 

        /// </summary> 

        public int UIK 

        { 

            get 

            { 

                return _electionManager.UIK; 

            } 

        } 

        /// <summary> 

        /// Номера вышестоящих комиссий для текущих выборов 

        /// </summary> 

        public object[] StampCommittees 

        { 

            get 

            { 

                var res = new List<object>(_currentElection.StampCommittees.Length); 

                foreach (var item in _currentElection.StampCommittees) 

                    res.Add(item.Num); 

 

 

                return res.ToArray(); 

            } 

        } 

        /// <summary> 

        /// Дата выборов 

        /// </summary> 

        public DateTime ElectionDate 

        { 

            get 

            { 

                return _electionManager.SourceData.ElectionDate; 

            } 


        } 

        /// <summary> 

        /// Время начала стац. голосования 

        /// </summary> 

        public DateTime MainVotingStartTime 

        { 

            get 

            { 

                return _electionManager.SourceData.MainVotingStartTime; 

            } 

        } 

        /// <summary> 

        /// Время окончания стац. голосования 

        /// </summary> 

        public DateTime MainVotingEndTime 

        { 

            get 

            { 

                return _electionManager.SourceData.MainVotingEndTime; 

            } 

        } 

        /// <summary> 

        /// Кол-во бюллетеней в ИД 

        /// </summary> 

        public int BulletinCount 

        { 

            get 

            { 

                return _electionManager.SourceData.Elections.Length; 

            } 

        } 

        /// <summary> 

        /// Кол-во мандатов для текущего бюллетеня 

        /// </summary> 

        public int CurrentBulletinMandateCount 

        { 

            get 

            { 

                return _currentElection.MaxMarks; 

            } 

        } 

        /// <summary> 

        /// Кол-во кандидатов в текущем бюллетене 

        /// </summary> 

        public int CurrentBulletinCandidateCount 

        { 

            get 

            { 

                return _currentElection.Candidates.Length; 

            } 


        } 

        /// <summary> 

        /// Номера снятых кандидатов 

        /// </summary> 

        public object[] DisabledCandidateNumbers 

        { 

            get 

            { 

                var res = new List<object>(); 

                for (int i = 0; i < _currentElection.Candidates.Length; i++) 

                    if (_currentElection.Candidates[i].DisabledInSourceData) 

                        res.Add(i + 1); 

 

 

                return res.ToArray(); 

            } 

        } 

 

 

        /// <summary> 

        /// Параметры печали ИД 

        /// </summary> 

        public ListDictionary SourceDataReportParameters 

        { 

            get 

            { 

                var list = new ListDictionary(); 

                // необходимо добавить ИД и УИК, так как на подчиненном сканере их нет 

                // а печатать можно и с него 

                list.Add("SourceData", _electionManager.SourceData); 

                list.Add("UIK", UIK); 

 

 

                return list; 

            } 

        } 

 

 

 

 

        /// <summary> 

        /// Проверяет, есть ли вышестоящие комиссии 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey StampCommitteesExists( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _currentElection.StampCommittees.Length > 0 ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 


        } 

 

 

        /// <summary> 

        /// Проверяет, кол-во мандатов (макс. кол-во отметок) > 1 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey MaxMarksMoreThenOne( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _currentElection.MaxMarks > 1 ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// Проверяет, есть ли снятые позиции в текущем бюллетене 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey DisabledCandidatesExists( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return DisabledCandidateNumbers.Length > 0 ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// Проверяет, существует ли позиция "Против всех" 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey NoneAboveExists( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _currentElection.NoneAboveExists ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

    } 

}



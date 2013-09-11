using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Workflow.ComponentModel; 

 

 

namespace Croc.Bpc.Workflow.Activities 

{ 

    /// <summary> 

    /// Действие, которое реализует механизм перебора кандидатов (позиций) 

    /// </summary> 

    [Serializable] 

    public class CandidateEnumeratorActivity : ElectionParametrizedActivity 

    { 

        /// <summary> 

        /// Индекс текущего кандидата 

        /// </summary> 

        protected int _currentCandidateIndex; 

        /// <summary> 

        /// Текущий кандидат 

        /// </summary> 

        [NonSerialized] 

        protected Election.Voting.Candidate _currentCandidate; 

        /// <summary> 

        /// Номер текущего кандидата 

        /// </summary> 

        public int CurrentCandidateNumber 

        { 

            get 

            { 

                return _currentCandidateIndex + 1; 

            } 

        } 

 

 

        /// <summary> 

        /// Инициализация 

        /// </summary> 

        /// <param name="context"></param> 

        protected override void Initialize(WorkflowExecutionContext context) 

        { 

            base.Initialize(context); 

 

 

            // если это не восстановление 

            if (!context.Restoring) 

            { 

                _currentCandidateIndex = -1; 

            } 


            // иначе, если восстановление и индекс текущего кандидата в пределах массива кандидатов 

            else if (0 <= _currentCandidateIndex && 

                _currentCandidateIndex < Election.Candidates.Length) 

            { 

                _currentCandidate = Election.Candidates[_currentCandidateIndex]; 

            } 

        } 

 

 

        /// <summary> 

        /// Передвигает итератор к след. кандидату 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey MoveNextCandidate( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            if (++_currentCandidateIndex < Election.Candidates.Length) 

            { 

                _currentCandidate = Election.Candidates[_currentCandidateIndex]; 

                return BpcNextActivityKeys.Yes; 

            } 

            else 

                return BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// Передвигает итератор к предыдущему кандидату 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey MovePreviousCandidate( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // т.к. в начале след. итерации цикла индекс будет передвинут на 1 вперед, то 

            // 1) если текущий кандидат не первый в списке, то нужно передвинуть индекс на 2 пункта назад 

            // 2) иначе - на 1 пункт назад 

            if (_currentCandidateIndex > 0) 

                _currentCandidateIndex -= 2; 

            else 

                _currentCandidateIndex -= 1; 

 

 

            return context.DefaultNextActivityKey; 

        } 

 

 


        /// <summary> 

        /// Передвигает итератор к кандидату, за которым следует первый кандидат 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey ResetCandidateEnumerator( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _currentCandidateIndex = -1; 

            return context.DefaultNextActivityKey; 

        } 

    } 

}



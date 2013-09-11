using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Workflow.ComponentModel; 

 

 

namespace Croc.Bpc.Workflow.Activities 

{ 

    /// <summary> 

    /// Действие, которое реализует механизм перебора выборов (бюллетеней) 

    /// </summary> 

    [Serializable] 

    public class ElectionEnumeratorActivity : BpcCompositeActivity 

    { 

        /// <summary> 

        /// Текущие выборы 

        /// </summary> 

        [NonSerialized] 

        protected Election.Voting.Election _currentElection; 

        /// <summary> 

        /// ИД бланка, который соотв. текущим выборам 

        /// </summary> 

        [NonSerialized] 

        protected string _currentBlankId; 

        /// <summary> 

        /// Индекс текущих выборов 

        /// </summary> 

        public int CurrentElectionIndex 

        { 

            get; 

            protected set; 

        } 

        /// <summary> 

        /// Номер текущих выборов 

        /// </summary> 

        public int CurrentElectionNumber 

        { 

            get 

            { 

                return CurrentElectionIndex + 1; 

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

                CurrentElectionIndex = -1; 

            } 

            // иначе, если восстановление и индекс текущих выборов в пределах массива выборов 

            else if (0 <= CurrentElectionIndex && 

                CurrentElectionIndex < _electionManager.SourceData.Elections.Length) 

            { 

                _currentElection = _electionManager.SourceData.Elections[CurrentElectionIndex]; 

                _currentBlankId = _electionManager.SourceData.GetBlankIdByElectionNumber(_currentElection.ElectionId); 

            } 

        } 

 

 

        /// <summary> 

        /// Передвигает итератор к следующим выборам в ИД 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey MoveNextElection( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            if (++CurrentElectionIndex < _electionManager.SourceData.Elections.Length) 

            { 

                _currentElection = _electionManager.SourceData.Elections[CurrentElectionIndex]; 

                _currentBlankId = _electionManager.SourceData.GetBlankIdByElectionNumber(_currentElection.ElectionId); 

                return BpcNextActivityKeys.Yes; 

            } 

            else 

                return BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// Передвигает итератор к предыдущим выборам в ИД 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey MovePreviousElection( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // т.к. в начале след. итерации цикла индекс будет передвинут на 1 вперед, то 

            // 1) если текущие выборы не первые в списке, то нужно передвинуть индекс на 2 пункта назад 


            // 2) иначе - на 1 пункт назад 

            if (CurrentElectionIndex > 0) 

                CurrentElectionIndex -= 2; 

            else 

                CurrentElectionIndex -= 1; 

 

 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Передвигает итератор к выборам, за которыми следуют первые выборы 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey ResetElectionEnumerator( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            CurrentElectionIndex = -1; 

            return context.DefaultNextActivityKey; 

        } 

    } 

}



using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Workflow.ComponentModel; 

using Croc.Core.Diagnostics; 

using Croc.Bpc.Election; 

 

 

namespace Croc.Bpc.Workflow.Activities.Cancelation 

{ 

    [Serializable] 

    public class CancelCandidatesActivity : CandidateEnumeratorActivity 

    { 

        /// <summary> 

        /// Проверка статуса кандидата 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey CheckCandidateStatus( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            if (_currentCandidate.DisabledLocally) 

                return BpcNextActivityKeys_Cancelation.CanceledLocally; 

 

 

            if (_currentCandidate.DisabledInSourceData) 

                return BpcNextActivityKeys_Cancelation.CanceledInSD; 

 

 

            return BpcNextActivityKeys_Cancelation.NotCanceled; 

        } 

 

 

        /// <summary> 

        /// Минимально необходимое кол-во позиций для заданных выборов 

        /// </summary> 

        /// <remarks> 

        /// вычисляется по формуле: 

        /// 1) Если выборы одномандатные = 2 

        /// 2) иначе (многомандатные) = (Кол-во мандатов для выборов) + (Есть позиция «Против всех» ? 1 : 0) 

        /// </remarks> 

        public int MinRequiredNotCanceledCandidates 

        { 

            get 

            { 

                return (Election.MaxMarks == 1) 

                    ? 2  

                    : Election.MaxMarks + (Election.NoneAboveExists ? 1 : 0); 


            } 

        } 

 

 

        /// <summary> 

        /// Проверяет, что кол-во еще не снятых позиций больше минимально необходимого кол-ва 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey NotCanceledCandidatesMoreThenMinRequired( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            var notCanceledCandidateCount = Election.Candidates.Count(cand => !cand.Disabled); 

 

 

            return notCanceledCandidateCount > MinRequiredNotCanceledCandidates 

                ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// Проверяет, является ли текущий кандидат позицией "Против всех" 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey IsNoneAboveCandidate( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            return _currentCandidate.NoneAbove ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

        /// <summary> 

        /// Снятие кандидата 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey CancelCandidate( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _currentCandidate.DisabledLocally = true; 

            return context.DefaultNextActivityKey; 

        } 

 

 

        /// <summary> 

        /// Восстановление кандидата 


        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey RestoreCandidate( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            _currentCandidate.DisabledLocally = false; 

            return context.DefaultNextActivityKey; 

        } 

    } 

}



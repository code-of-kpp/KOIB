using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Core.Diagnostics; 

using Croc.Workflow.ComponentModel; 

using Croc.Bpc.Election; 

 

 

namespace Croc.Bpc.Workflow.Activities.Cancelation 

{ 

    [Serializable] 

    public class SayCanceledCandidatesInElectionActivity : BpcCompositeActivity 

    { 

        /// <summary> 

        /// Индекс выборов 

        /// </summary> 

        public int ElectionIndex 

        { 

            get; 

            set; 

        } 

        /// <summary> 

        /// Номер выборов 

        /// </summary> 

        public int ElectionNumber 

        { 

            get 

            { 

                return ElectionIndex + 1; 

            } 

        } 

        /// <summary> 

        /// Параметры фразы для воспроизведения номеров снятых кандидатов 

        /// </summary> 

        public object[] SayCanceledCandidatesPhraseParameters 

        { 

            get 

            { 

                var paramList = new List<object>(); 

                paramList.Add(ElectionNumber); 

 

 

                var election = _electionManager.SourceData.Elections[ElectionIndex]; 

                for (int i = 0; i < election.Candidates.Length; i++) 

                    if (election.Candidates[i].Disabled) 

                        paramList.Add(i + 1); 

 

 

                return paramList.ToArray(); 


            } 

        } 

 

 

        /// <summary> 

        /// Есть снятые кандидаты? 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey HasCanceledCandidates( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            var election = _electionManager.SourceData.Elections[ElectionIndex]; 

            var canceledCandidatesCount = election.Candidates.Count(cand => cand.Disabled); 

 

 

            return canceledCandidatesCount == 0 ? BpcNextActivityKeys.No : BpcNextActivityKeys.Yes; 

        } 

    } 

}



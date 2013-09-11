using System; 
using System.Collections.Generic; 
using System.Linq; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.Cancelation 
{ 
    [Serializable] 
    public class SayCanceledCandidatesInElectionActivity : BpcCompositeActivity 
    { 
        public int ElectionIndex 
        { 
            get; 
            set; 
        } 
        public int ElectionNumber 
        { 
            get 
            { 
                return ElectionIndex + 1; 
            } 
        } 
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
        public NextActivityKey HasCanceledCandidates( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var election = _electionManager.SourceData.Elections[ElectionIndex]; 
            var canceledCandidatesCount = election.Candidates.Count(cand => cand.Disabled); 
            return canceledCandidatesCount == 0 ? BpcNextActivityKeys.No : BpcNextActivityKeys.Yes; 
        } 
    } 
}

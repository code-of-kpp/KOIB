using System; 
using System.Linq; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.Cancelation 
{ 
    [Serializable] 
    public class CancelCandidatesActivity : CandidateEnumeratorActivity 
    { 
        public NextActivityKey CheckCandidateStatus( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (_currentCandidate.DisabledLocally) 
                return BpcNextActivityKeys_Cancelation.CanceledLocally; 
            if (_currentCandidate.DisabledInSourceData) 
                return BpcNextActivityKeys_Cancelation.CanceledInSD; 
            return BpcNextActivityKeys_Cancelation.NotCanceled; 
        } 
        public int MinRequiredNotCanceledCandidates 
        { 
            get 
            { 
                return (Election.MaxMarks == 1) 
                           ? 2 
                           : Election.MaxMarks + (Election.NoneAboveExists ? 1 : 0); 
            } 
        } 
        public NextActivityKey NotCanceledCandidatesMoreThenMinRequired( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var notCanceledCandidateCount = Election.Candidates.Count(cand => !cand.Disabled); 
            return notCanceledCandidateCount > MinRequiredNotCanceledCandidates 
                ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey IsNoneAboveCandidate( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _currentCandidate.NoneAbove ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey CancelCandidate( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _currentCandidate.Disabled = true; 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey CheckCanRestoreCanceledInSD( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (_electionManager.СanRestoreCandidateCanseledInSd) 
                return BpcNextActivityKeys.Yes; 
            return BpcNextActivityKeys.No; 
        } 
        public NextActivityKey RestoreCandidate( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _currentCandidate.Disabled = false; 
            return context.DefaultNextActivityKey; 
        } 
    } 
}

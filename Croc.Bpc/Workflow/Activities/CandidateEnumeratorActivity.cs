using System; 
using Croc.Bpc.Voting; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities 
{ 
    [Serializable] 
    public class CandidateEnumeratorActivity : ElectionParametrizedActivity 
    { 
        protected int _currentCandidateIndex; 
        [NonSerialized] 
        protected Candidate _currentCandidate; 
        public int CurrentCandidateNumber 
        { 
            get 
            { 
                return _currentCandidateIndex + 1; 
            } 
        } 
        protected override void Initialize(WorkflowExecutionContext context) 
        { 
            base.Initialize(context); 
            if (!context.Restoring) 
            { 
                _currentCandidateIndex = -1; 
            } 
            else if (0 <= _currentCandidateIndex && 
                _currentCandidateIndex < Election.Candidates.Length) 
            { 
                _currentCandidate = Election.Candidates[_currentCandidateIndex]; 
            } 
        } 
        public NextActivityKey MoveNextCandidate( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (++_currentCandidateIndex < Election.Candidates.Length) 
            { 
                _currentCandidate = Election.Candidates[_currentCandidateIndex]; 
                return BpcNextActivityKeys.Yes; 
            } 
            return BpcNextActivityKeys.No; 
        } 
        public NextActivityKey MovePreviousCandidate( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (_currentCandidateIndex > 0) 
                _currentCandidateIndex -= 2; 
            else 
                _currentCandidateIndex -= 1; 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey ResetCandidateEnumerator( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _currentCandidateIndex = -1; 
            return context.DefaultNextActivityKey; 
        } 
    } 
}

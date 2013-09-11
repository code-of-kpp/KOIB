using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using Croc.Bpc.Voting; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities 
{ 
    [Serializable] 
    public class ElectionEnumeratorActivity : BpcCompositeActivity 
    { 
        [NonSerialized] 
        protected Election _currentElection; 
        [NonSerialized] 
        protected string _currentBlankId; 
        public int CurrentElectionIndex 
        { 
            get; 
            protected set; 
        } 
        public int CurrentElectionNumber 
        { 
            get 
            { 
                return CurrentElectionIndex + 1; 
            } 
        } 
        protected override void Initialize(WorkflowExecutionContext context) 
        { 
            base.Initialize(context); 
            if (!context.Restoring) 
            { 
                CurrentElectionIndex = -1; 
            } 
            else if (0 <= CurrentElectionIndex && 
                CurrentElectionIndex < _electionManager.SourceData.Elections.Length) 
            { 
                _currentElection = _electionManager.SourceData.Elections[CurrentElectionIndex]; 
                _currentBlankId = _electionManager.SourceData.GetBlankIdByElectionNumber(_currentElection.ElectionId); 
            } 
        } 
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
        public NextActivityKey MovePreviousElection( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (CurrentElectionIndex > 0) 
                CurrentElectionIndex -= 2; 
            else 
                CurrentElectionIndex -= 1; 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey ResetElectionEnumerator( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            CurrentElectionIndex = -1; 
            return context.DefaultNextActivityKey; 
        } 
    } 
}

using System; 
using System.Collections.Generic; 
using System.Collections.Specialized; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities.Initialization 
{ 
    [Serializable] 
    public class ReadSourceDataActivity : ElectionEnumeratorActivity 
    { 
        public int Uik 
        { 
            get 
            { 
                return _electionManager.SourceData.Uik; 
            } 
        } 
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
        public DateTime ElectionDate 
        { 
            get 
            { 
                return _electionManager.SourceData.ElectionDate; 
            } 
        } 
        public DateTime MainVotingStartTime 
        { 
            get 
            { 
                return _electionManager.SourceData.MainVotingStartTime; 
            } 
        } 
        public DateTime MainVotingEndTime 
        { 
            get 
            { 
                return _electionManager.SourceData.MainVotingEndTime; 
            } 
        } 
        public int BulletinCount 
        { 
            get 
            { 
                return _electionManager.SourceData.Blanks.Length; 
            } 
        } 
        public int ElectionCount 
        { 
            get 
            { 
                return _electionManager.SourceData.Elections.Length; 
            } 
        } 
        public int CurrentBulletinMandateCount 
        { 
            get 
            { 
                return _currentElection.MaxMarks; 
            } 
        } 
        public int CurrentBulletinCandidateCount 
        { 
            get 
            { 
                return _currentElection.Candidates.Length; 
            } 
        } 
        public object[] DisabledCandidateNumbers 
        { 
            get 
            { 
                var res = new List<object>(); 
                for (var i = 0; i < _currentElection.Candidates.Length; i++) 
                    if (_currentElection.Candidates[i].DisabledInSourceData) 
                        res.Add(i + 1); 
                return res.ToArray(); 
            } 
        } 
        public ListDictionary SourceDataReportParameters 
        { 
            get 
            { 
                var list = new ListDictionary(); 
                list.Add("SourceData", _electionManager.SourceData); 
                list.Add("UIK", Uik); 
                return list; 
            } 
        } 
        public NextActivityKey IsBulletinCountLessThenElectionCount( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return BulletinCount < ElectionCount 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey StampCommitteesExists( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _currentElection.StampCommittees.Length > 0 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey DisabledCandidatesExists( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return DisabledCandidateNumbers.Length > 0 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey NoneAboveExists( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return _currentElection.NoneAboveExists 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
    } 
}

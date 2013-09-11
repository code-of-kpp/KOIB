using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using Croc.Bpc.Voting; 
namespace Croc.Bpc.Workflow.Activities 
{ 
    [Serializable] 
    public class ElectionParametrizedActivity : BpcCompositeActivity 
    { 
        private int _electionIndex; 
        public int ElectionIndex 
        { 
            get 
            { 
                return _electionIndex; 
            } 
            set 
            { 
                _electionIndex = value; 
                _election = _electionManager.SourceData.Elections[_electionIndex]; 
                _blankId = _electionManager.SourceData.GetBlankIdByElectionNumber(_election.ElectionId); 
            } 
        } 
        private Election _election; 
        protected Election Election 
        { 
            get 
            { 
                return _election; 
            } 
        } 
        private string _blankId; 
        public string BlankId 
        { 
            get 
            { 
                return _blankId; 
            } 
        } 
        public int ElectionNumber 
        { 
            get 
            { 
                return _electionIndex + 1; 
            } 
        } 
    } 
}

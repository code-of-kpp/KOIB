using System; 
using System.Collections; 
using System.Text; 
using Croc.Bpc.Voting; 
namespace Croc.Bpc.Election 
{ 
    [Serializable] 
    public class SourceDataChangesCache 
    { 
        private readonly Hashtable _electionChangesCacheDict = new Hashtable(); 
        private ElectionChangesCache GetElectionChangesCache(string electionId) 
        { 
            if (!_electionChangesCacheDict.ContainsKey(electionId)) 
                _electionChangesCacheDict[electionId] = new ElectionChangesCache(); 
            return (ElectionChangesCache)_electionChangesCacheDict[electionId]; 
        } 
        public bool IsEmpty 
        { 
            get { return _electionChangesCacheDict.Count == 0; } 
        } 
        public void StoreLineValue(Line line) 
        { 
            if (line == null) 
                return; 
            var ecc = GetElectionChangesCache(line.Election.ElectionId); 
            ecc.LineValues[line.FullKey] = line.Value; 
        } 
        public void StoreCandidateDisabling(Candidate candidate) 
        { 
            if (candidate == null) 
                return; 
            var ecc = GetElectionChangesCache(candidate.Election.ElectionId); 
            ecc.CandidateDisabling[candidate.Id] = candidate.Disabled; 
        } 
        public void ApplyChanges(SourceData targetSourceData) 
        { 
            if (targetSourceData == null) 
                return; 
            foreach (var election in targetSourceData.Elections) 
            { 
                var ecc = (ElectionChangesCache)_electionChangesCacheDict[election.ElectionId]; 
                if (ecc == null) 
                    continue; 
                if (ecc.LineValues.Count > 0) 
                { 
                    foreach (var line in election.Protocol.Lines) 
                    { 
                        var value = ecc.LineValues[line.FullKey]; 
                        if (value == null) 
                            continue; 
                        line.Value = (int)value; 
                    } 
                } 
                if (ecc.CandidateDisabling.Count > 0) 
                { 
                    foreach (var candidate in election.Candidates) 
                    { 
                        var disabled = ecc.CandidateDisabling[candidate.Id]; 
                        if (disabled == null) 
                            continue; 
                        candidate.Disabled = (bool)disabled; 
                    } 
                } 
            } 
        } 
        public override bool Equals(object obj) 
        { 
            var other = obj as SourceDataChangesCache; 
            if (other == null) 
                return false; 
            return string.CompareOrdinal(ToString(), other.ToString()) == 0; 
        } 
        public override string ToString() 
        { 
            var sb = new StringBuilder(_electionChangesCacheDict.Keys.Count * 256); 
            foreach (var eccKey in _electionChangesCacheDict.Keys) 
            { 
                var ecc = (ElectionChangesCache)_electionChangesCacheDict[eccKey]; 
                sb.Append(eccKey); 
                sb.Append(':'); 
                foreach (var lvKey in ecc.LineValues.Keys) 
                { 
                    sb.Append(lvKey); 
                    sb.Append('='); 
                    sb.Append(ecc.LineValues[lvKey]); 
                    sb.Append('|'); 
                } 
                sb.Append(','); 
                foreach (var cdKey in ecc.CandidateDisabling.Keys) 
                { 
                    sb.Append(cdKey); 
                    sb.Append('='); 
                    sb.Append(ecc.CandidateDisabling[cdKey]);  
                    sb.Append('|'); 
                } 
                sb.Append(';'); 
            } 
            return sb.ToString(); 
        } 
        [Serializable] 
        private class ElectionChangesCache 
        { 
            public readonly Hashtable LineValues = new Hashtable(); 
            public readonly Hashtable CandidateDisabling = new Hashtable(); 
        } 
    } 
}

using System; 
using System.Collections; 
using System.Collections.Generic; 
using System.Linq; 
using Croc.Bpc.Scanner; 
using Croc.Bpc.Diagnostics; 
using Croc.Core; 
using Croc.Core.Diagnostics; 
namespace Croc.Bpc.Voting 
{ 
    [Serializable] 
    public class VotingResults 
    { 
        private static readonly object s_votesSync = new object(); 
        private readonly Hashtable _votes = new Hashtable(); 
        [NonSerialized] 
        private ILogger _logger; 
        [NonSerialized] 
        private bool _addBadBlankToCounterValue; 
        [NonSerialized] 
        private VoteKey[] _counterKeys; 
        public void Init(ILogger logger, bool addBadBlankToCounterValue) 
        { 
            CodeContract.Requires(logger != null); 
            _logger = logger; 
            _addBadBlankToCounterValue = addBadBlankToCounterValue; 
        } 
        public void SetNewVotesValue(VoteKey voteKey, int value) 
        { 
            _votes[voteKey] = value; 
            _logger.LogInfo( 
                Message.VotingResult_SetVotesValue,  
                voteKey.ToString(),  
                0, value,  
                GetCounterValue(voteKey.ScannerSerialNumber)); 
        } 
        private void UpdateVotesValue(VoteKey voteKey, int newValue) 
        { 
            var currentValue = (int) _votes[voteKey]; 
            if (currentValue >= newValue) 
            { 
                _logger.LogInfo( 
                    Message.VotingResult_TryToSetIncorrectVotesValue, 
                    voteKey.ToString(),  
                    currentValue, newValue, 
                    GetCounterValue(voteKey.ScannerSerialNumber)); 
                return; 
            } 
            _votes[voteKey] = newValue; 
            _logger.LogInfo( 
                Message.VotingResult_SetVotesValue,  
                voteKey.ToString(),  
                currentValue, newValue, 
                GetCounterValue(voteKey.ScannerSerialNumber)); 
        } 
        public void ClearTestData() 
        { 
            lock (s_votesSync) 
            { 
                var keys = new VoteKey[_votes.Keys.Count]; 
                _votes.Keys.CopyTo(keys, 0); 
                foreach (var key in keys.Where(key => key.VotingMode == VotingMode.Test)) 
                { 
                    _votes.Remove(key); 
                    _logger.LogInfo(Message.VotingResult_ClearTestData); 
                } 
            } 
        } 
        public void AddVote(VoteKey voteKey) 
        { 
            lock (s_votesSync) 
            { 
                if (_votes.ContainsKey(voteKey)) 
                    UpdateVotesValue(voteKey, (int)_votes[voteKey] + 1); 
                else 
                    SetNewVotesValue(voteKey, 1); 
            } 
        } 
        public enum MergeResult 
        { 
            NotNeed, 
            OtherContainMoreVotes, 
            OurContainMoreVotes, 
        } 
        public MergeResult Merge(VotingResults otherVotingResults) 
        { 
            lock (s_votesSync) 
            { 
                var otherVotesMore = false; 
                var ourVotesMore = false; 
                foreach (VoteKey key in otherVotingResults._votes.Keys) 
                { 
                    if (!_votes.ContainsKey(key)) 
                    { 
                        SetNewVotesValue(key, (int) otherVotingResults._votes[key]); 
                        otherVotesMore = true; 
                    } 
                    else if ((int)_votes[key] < (int)otherVotingResults._votes[key]) 
                    { 
                        UpdateVotesValue(key, (int)otherVotingResults._votes[key]); 
                        otherVotesMore = true; 
                    } 
                } 
                foreach (VoteKey key in _votes.Keys) 
                { 
                    if (!otherVotingResults._votes.ContainsKey(key) || 
                        (int)otherVotingResults._votes[key] < (int)_votes[key]) 
                    { 
                        ourVotesMore = true; 
                    } 
                } 
                return ourVotesMore 
                           ? MergeResult.OurContainMoreVotes 
                           : (otherVotesMore ? MergeResult.OtherContainMoreVotes : MergeResult.NotNeed); 
            } 
        } 
        #region public-методы для получения кол-ва голосов 
        public void SetCounterValueKeys(VoteKey[] keys) 
        { 
            CodeContract.Requires(keys != null && keys.Length > 0); 
            lock (s_votesSync) 
            { 
                _counterKeys = keys; 
                if (!_addBadBlankToCounterValue) 
                { 
                    foreach (var key in 
                        _counterKeys.Where(key => key.BlankType.HasValue && key.BlankType.Value == BlankType.All)) 
                    { 
                        key.BlankType = BlankType.AllButBad; 
                    } 
                } 
            } 
        } 
        public int GetCounterValue(int? scannerSerialNumber) 
        { 
            lock (s_votesSync) 
            { 
                if (_counterKeys == null) 
                           return 0; 
                var counterValue = 0; 
                foreach (var key in _counterKeys) 
                { 
                    key.ScannerSerialNumber = scannerSerialNumber; 
                    counterValue += VotesCount(key); 
                } 
                return counterValue; 
            } 
        } 
        public int TotalBulletinCount 
        { 
            get 
            { 
                var key = new VoteKey { BlankType = BlankType.All }; 
                return VotesCount(key); 
            } 
        } 
        public int GetBulletinCountForScanner(int scannerSerialNumber) 
        { 
            var key = new VoteKey 
            { 
                ScannerSerialNumber = scannerSerialNumber, 
                BlankType = BlankType.All 
            }; 
            return VotesCount(key); 
        } 
        public int VotesCount(VoteKey mask) 
        { 
            lock (s_votesSync) 
            { 
                return (from VoteKey key in _votes.Keys where key.CheckMask(mask) select (int) _votes[key]).Sum(); 
            } 
        } 
        public int GetTotalVotesCount(string electionId) 
        { 
            var key = new VoteKey 
            { 
                ElectionNum = electionId 
            }; 
            return VotesCount(key); 
        } 
        public int GetAboveCandidateVotesCount(Election election) 
        { 
            if (!election.NoneAboveExists) 
                return 0; 
            var key = new VoteKey 
            { 
                ElectionNum = election.ElectionId, 
                CandidateId = election.NoneAboveCandidate.Id 
            }; 
            return VotesCount(key); 
        } 
        public List<ScannerInfo> GetScannerInfos() 
        { 
            lock (s_votesSync) 
            { 
                var resultList = new List<ScannerInfo>(); 
                foreach (VoteKey key in _votes.Keys) 
                { 
                    if (key.ScannerSerialNumber == null) 
                        continue; 
                    var scannerInfo = new ScannerInfo(key.ScannerSerialNumber.ToString(), null); 
                    if (!resultList.Contains(scannerInfo)) 
                        resultList.Add(scannerInfo); 
                } 
                return resultList; 
            } 
        } 
        #endregion 
    } 
}

using System; 
using System.Text; 
namespace Croc.Bpc.Voting 
{ 
    [Serializable] 
    public class VoteKey 
    { 
        private int _scannerSerialNumber; 
        private bool _scannerSerialNumberHasValue; 
        public VoteKey() 
        { } 
        public VoteKey(BlankType? type, 
            VotingMode? mode, 
            int? scannerSerial, 
            string candidate, 
            string election, 
            string blankId) 
        { 
            BlankType = type; 
            VotingMode = mode; 
            ScannerSerialNumber = scannerSerial; 
            CandidateId = candidate; 
            ElectionNum = election; 
            BlankId = blankId; 
        } 
        public int? ScannerSerialNumber 
        { 
            get 
            { 
                return _scannerSerialNumberHasValue ? (int?)_scannerSerialNumber : null; 
            } 
            set 
            { 
                if (value.HasValue) 
                { 
                    _scannerSerialNumber = value.Value; 
                    _scannerSerialNumberHasValue = true; 
                } 
                else 
                    _scannerSerialNumberHasValue = false; 
            } 
        } 
        private VotingMode? _votingMode; 
        private bool _votingModeHasValue; 
        public VotingMode? VotingMode 
        { 
            get 
            { 
                return _votingModeHasValue ? _votingMode : null; 
            } 
            set 
            { 
                if (value.HasValue) 
                { 
                    _votingMode = value.Value; 
                    _votingModeHasValue = true; 
                } 
                else 
                    _votingModeHasValue = false; 
            } 
        } 
        public string CandidateId { get; set; } 
        public string ElectionNum { get; set; } 
        public string BlankId { get; set; } 
        private BlankType? _blankType; 
        private bool _blankTypeHasValue; 
        public BlankType? BlankType 
        { 
            get 
            { 
                return _blankTypeHasValue ? _blankType : null; 
            } 
            set 
            { 
                if (value.HasValue) 
                { 
                    _blankType = value.Value; 
                    _blankTypeHasValue = true; 
                } 
                else 
                    _blankTypeHasValue = false; 
            } 
        } 
        public override bool Equals(object obj) 
        { 
            var other = obj as VoteKey; 
            if (other == null || 
                other.BlankType.HasValue && other.BlankType.Value != BlankType || 
                other.CandidateId != null && other.CandidateId.CompareTo(CandidateId) != 0 || 
                other.ElectionNum != null && other.ElectionNum.CompareTo(ElectionNum) != 0 || 
                other.BlankId != null && other.BlankId.CompareTo(BlankId) != 0 || 
                other.ScannerSerialNumber.HasValue && other.ScannerSerialNumber != ScannerSerialNumber || 
                other.VotingMode.HasValue && other.VotingMode.Value != VotingMode) 
                return false; 
            return true; 
        } 
        public override int GetHashCode() 
        { 
            int result = 0; 
            if (BlankType.HasValue) 
            { 
                result += (int)BlankType.Value; 
            } 
            if (VotingMode.HasValue) 
            { 
                result += ((int)VotingMode.Value) << 2; 
            } 
            if (ScannerSerialNumber.HasValue) 
            { 
                result += (ScannerSerialNumber.Value % 16) << 5; 
            } 
            if (CandidateId != null) 
            { 
                result ^= CandidateId.GetHashCode(); 
            } 
            if (BlankId != null) 
            { 
                result ^= BlankId.GetHashCode(); 
            } 
            if (ElectionNum != null) 
            { 
                result ^= ElectionNum.GetHashCode(); 
            } 
            return result; 
        } 
        public bool CheckMask(VoteKey mask) 
        { 
            if (mask.BlankType.HasValue) 
            { 
                if (!BlankType.HasValue) 
                    return false; 
                switch (mask.BlankType.Value) 
                { 
                    case Voting.BlankType.AllButBad: 
                        if (BlankType.Value == Voting.BlankType.Bad || 
                            BlankType.Value == Voting.BlankType.BadMode) 
                            return false; 
                        break; 
                    case Voting.BlankType.NotValid: 
                        if (BlankType.Value == Voting.BlankType.Valid || 
                            BlankType.Value == Voting.BlankType.Bad || 
                            BlankType.Value == Voting.BlankType.BadMode) 
                            return false; 
                        break; 
                    default: 
                        if (// нужен бланк конкретного типа 
                            mask.BlankType.Value != Voting.BlankType.All && 
                            mask.BlankType.Value != BlankType.Value) 
                            return false; 
                        break; 
                } 
            } 
            if (mask.CandidateId != null && mask.CandidateId != CandidateId || 
                mask.ElectionNum != null && mask.ElectionNum != ElectionNum || 
                mask.BlankId != null && mask.BlankId != BlankId || 
                mask.ScannerSerialNumber.HasValue && mask.ScannerSerialNumber.Value != ScannerSerialNumber || 
                mask.VotingMode.HasValue && mask.VotingMode.Value != VotingMode) 
                return false; 
            return true; 
        } 
        public override string ToString() 
        { 
            var sb = new StringBuilder(64); 
            sb.Append("BT="); 
            if (BlankType.HasValue) 
                sb.Append((int)BlankType.Value); 
            sb.Append(';'); 
            sb.Append("CI="); 
            if (CandidateId != null) 
                sb.Append(CandidateId); 
            sb.Append(';'); 
            sb.Append("EL="); 
            if (ElectionNum != null) 
                sb.Append(ElectionNum); 
            sb.Append(';'); 
            sb.Append("BI="); 
            if (BlankId != null) 
                sb.Append(BlankId); 
            sb.Append(';'); 
            sb.Append("SN="); 
            if (ScannerSerialNumber.HasValue) 
                sb.Append(ScannerSerialNumber.Value); 
            sb.Append(';'); 
            sb.Append("VM="); 
            if (VotingMode.HasValue) 
                sb.Append((int)VotingMode.Value); 
            return sb.ToString(); 
        } 
    } 
}

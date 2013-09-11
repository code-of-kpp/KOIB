using System; 
using System.Collections; 
using System.Linq; 
using System.Xml.Serialization; 
using System.Text; 
using Croc.Core; 
using Croc.Bpc.Diagnostics; 
namespace Croc.Bpc.Voting 
{ 
    [Serializable, XmlType("Election")] 
    public class Election 
    { 
        [XmlAttribute("disabled")] 
        public bool Disabled; 
        [XmlElement("ParentComittee")] 
        public ParentComittee ParentComittee; 
        [XmlArray("StampCommittees")] 
        [XmlArrayItem("StampCommittee", IsNullable = false)] 
        public StampCommittee[] StampCommittees = new StampCommittee[0]; //Инициализируем пустым массивом 
        private ProtocolTemplate _protocol; 
        [XmlElement("Protocol")] 
        public ProtocolTemplate Protocol 
        { 
            get 
            { 
                return _protocol; 
            } 
            set 
            { 
                _protocol = value; 
                _protocol.SetElection(this); 
            } 
        } 
        [XmlArray("Candidates")] 
        public Candidate[] Candidates = new Candidate[0]; //Инициализируем пустым массивом 
        [XmlArray("Modes")] 
        [XmlArrayItem("Mode")] 
        public VotingMode[] VotingModes = new VotingMode[0]; //Инициализируем пустым массивом 
        [XmlAttribute("name")] 
        public string Name; 
        [XmlAttribute("maxMarks")] 
        public int MaxMarks; 
        [XmlAttribute("num")] 
        public string ExternalNumber; 
        [XmlAttribute("id")] 
        public string Id; 
        [XmlIgnore] 
        public string ElectionId 
        { 
            get 
            { 
                if(string.IsNullOrEmpty(Id)) 
                    return ExternalNumber; 
                return Id; 
            } 
        } 
        [XmlIgnore] 
        public bool NoneAboveExists 
        { 
            get 
            { 
                foreach (Candidate oCand in Candidates) 
                    if (oCand.NoneAbove) //нашли 
                        return true; 
                return false; //не нашли 
            } 
        } 
        [XmlIgnore] 
        public Candidate NoneAboveCandidate 
        { 
            get 
            { 
                foreach (Candidate oCand in Candidates) 
                    if (oCand.NoneAbove) return oCand; 
                return null; 
            } 
        } 
        [XmlIgnore] 
        public string Modes 
        { 
            get 
            { 
                return SourceData.GetModeNames(VotingModes); 
            } 
        } 
        [XmlIgnore] 
        internal SourceData SourceData 
        { 
            get; 
            set; 
        } 
        public int GetRealCandidateLength() 
        { 
            int againstAll = Candidates.Count(с => с.NoneAbove); 
            return Candidates.Length - againstAll; 
        } 
        private string _failedKs = String.Empty; 
        private readonly ArrayList _failedKsList = new ArrayList(); 
        private bool _ksTurnedOff; 
        [XmlIgnore] 
        public string KsFailedNumbers 
        { 
            get 
            { 
                return _failedKs; 
            } 
            set 
            { 
                _failedKs = value; 
            } 
        } 
        [XmlIgnore] 
        public ArrayList FailedControlRelations 
        { 
            get 
            { 
                return _failedKsList; 
            } 
        } 
        [XmlIgnore] 
        public bool KsTurnedOff 
        { 
            get 
            { 
                return _ksTurnedOff; 
            } 
            set 
            { 
                _ksTurnedOff = value; 
            } 
        } 
        public bool IsControlRelationsSatisfied() 
        { 
            return AllChecksDone() != CheckResult.Failed; 
        } 
        public CheckResult AllChecksDone() 
        { 
            FailedControlRelations.Clear(); 
            CheckResult done = CheckResult.NotDefined; 
            KsFailedNumbers = ""; 
            KsTurnedOff = true; 


            if (Protocol.Checks != null) 
            { 
                done = CheckResult.OK; 
                int ksNumber = 0; 
                foreach (CheckExpression check in Protocol.Checks) 
                { 
                    try 
                    { 
                        if (!check.Check(Protocol)) 
                        { 
                            if (check.Enabled) KsTurnedOff = false; 
                            done = MakeKsError(check.Mild, ksNumber, done, null); 
                        } 
                    } 
                    catch (Exception ex) 
                    { 
                        done = MakeKsError(check.Mild, ksNumber, done, ex.Message); 
                    } 
                    ksNumber++; 
                } 
                if (_failedKs.Length != 0) 
                { 
                    _failedKs = _failedKs.TrimEnd(','); 
                    CoreApplication.Instance.Logger.LogInfo(Message.Election_ChecksFailed, _failedKs); 
                } 
            } 
            return done; 
        } 
        private CheckResult MakeKsError(bool mildKs, int i, CheckResult done, string additionalMessage) 
        { 
            _failedKs += (i + 1) +  
                (!string.IsNullOrEmpty(additionalMessage) ? "(" + additionalMessage + ")" : "") + ","; 
            _failedKsList.Add(i); 
            if(done != CheckResult.Failed && mildKs) 
            { 
                done = CheckResult.LogicalFailed; 
            } 
            else  
            { 
                done = CheckResult.Failed; 
            } 
            return done; 
        } 
        public override string ToString() 
        { 
            StringBuilder sText = new StringBuilder(); 
            sText.Append("["); 
            sText.Append("isDisabled=" + Disabled + ";"); 
            sText.Append("ParentComittee" + ParentComittee + ";"); 
            sText.Append("StampCommittees=["); 
            foreach(StampCommittee oCommittie in StampCommittees) 
            { 
                sText.Append(oCommittie + ";"); 
            } 
            sText.Append("];"); 
            sText.Append("Protocol=" + Protocol + ";"); 
            sText.Append("Candidates=["); 
            foreach(Candidate oCand in Candidates) 
            { 
                sText.Append(oCand + ";"); 
            } 
            sText.Append("],"); 
            sText.Append("Modes=["); 
            foreach(VotingMode mode in VotingModes) 
            { 
                sText.Append(mode + ";"); 
            } 
            sText.Append("];"); 
            sText.Append("Name=" + Name + ";"); 
            sText.Append("MaxMarks=" + MaxMarks + ";"); 
            sText.Append("Num=" + ExternalNumber + ";"); 
            sText.Append("Id=" + Id + ";"); 
            sText.Append("]"); 
            return sText.ToString(); 
        } 
    } 
}

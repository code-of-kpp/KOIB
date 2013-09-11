using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Reflection; 
using System.Text; 
using System.Xml.Serialization; 
using Croc.Bpc.Utils; 
namespace Croc.Bpc.Voting 
{ 
    [Serializable, XmlType("SourceData", Namespace = XMLNS)] 
    public class SourceData 
    { 
        public const string XMLNS = "http://localhost/Schemas/SIB2003/SourceData"; 
        public const string SHEMA_PATH = @"Data/Schemas/SourceData.xsd"; 
        private string _hashCode = string.Empty; 
        public string HashCode 
        { 
            get 
            { 
                return _hashCode; 
            } 
        } 
        private int _uik; 
        public int Uik 
        { 
            get 
            { 
                return _uik; 
            } 
        } 
        #region Сериализуемые поля 
        #region xml-атрибуты 
        [XmlAttribute("id")] 
        public Guid Id; 
        [XmlAttribute("version")] 
        public int Version; 
        [XmlAttribute("DateTime", DataType = "date")] 
        public DateTime ElectionDate; 
        [XmlAttribute("localTimeZone")] 
        public string LocalTimeZoneString; 
        [XmlAttribute("Mode")] 
        public ElectionMode ElectionMode; 
        [XmlAttribute("FileSuffix")] 
        public string RealModeFileSuffix; 
        [XmlAttribute("TrainingFileSuffix")] 
        public string TrainingModeFileSuffix; 
        [XmlAttribute("isGasVrn")] 
        public bool IsGasVrn; 
        #endregion 
        #region xml-элементы 
        [XmlArray("Elections")] 
        public Election[] Elections; 
        [XmlArray("Blanks")] 
        public Blank[] Blanks; 
        [XmlArray("CommitteeMembers")] 
        public CommitteeMember[] CommitteeMembers = new CommitteeMember[0]; //Инициализируем пустым массивом 
        [XmlArray("Targets")] 
        public TargetCommittee[] Targets; 
        [XmlArray("Modes")] 
        [XmlArrayItem("Mode")] 
        public VotingMode[] VotingModes = new VotingMode[0]; //Инициализируем пустым массивом 
        [XmlArray("ModeTimeTable")] 
        [XmlArrayItem("ModeTime")] 
        public ModeTime[] VotingModeTimes = new ModeTime[0]; //Инициализируем пустым массивом 
        #endregion 
        #endregion 
        public void Init(int uik) 
        { 
            if (Id == Guid.Empty) 
                throw new InvalidOperationException("Не задан идентификатор"); 
            _uik = uik; 
            _hashCode = string.Format("{0}_{1}_{2}", Id, _uik, ElectionDate.ToString("ddMMyyyy")); 
            foreach (var blank in Blanks) 
                blank.SourceData = this; 
            foreach (var election in Elections) 
            { 
                election.SourceData = this; 
                foreach (var line in election.Protocol.Lines) 
                    line.Election = election; 
                foreach (var candidate in election.Candidates) 
                    candidate.Election = election; 
            } 
        } 
        [XmlIgnore] 
        [NonSerialized] 
        private TimeZoneInfo _localTimeZone; 
        [XmlIgnore] 
        public TimeZoneInfo LocalTimeZone 
        { 
            get 
            { 
                if (_localTimeZone == null) 
                { 
                    if (string.IsNullOrEmpty(LocalTimeZoneString)) 
                    { 
                        _localTimeZone = TimeZoneInfo.Local; 
                    } 
                    else 
                    { 
                        try 
                        { 
                            var elems = LocalTimeZoneString.Split(';'); 
                            _localTimeZone = TimeZoneInfo.CreateCustomTimeZone( 
                                elems[0], 
                                TimeSpan.FromMinutes(int.Parse(elems[1])), 
                                elems[2], 
                                elems[3], 
                                null, 
                                new TimeZoneInfo.AdjustmentRule[] { }, 
                                true); 
                        } 
                        catch (Exception ex) 
                        { 
                            throw new Exception("Некорректно задан часовой пояс в ИД", ex); 
                        } 
                    } 
                } 
                return _localTimeZone; 
            } 
        } 
        [XmlIgnore] 
        public DateTime LocalTimeNow 
        { 
            get 
            { 
                var utc = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now); 
                return TimeZoneInfo.ConvertTimeFromUtc(utc, LocalTimeZone); 
            } 
        } 
        [XmlIgnore] 
        private string NullableFileSuffix 
        { 
            get 
            { 
                return ElectionMode == ElectionMode.Real 
                    ? RealModeFileSuffix 
                    : (TrainingModeFileSuffix ?? RealModeFileSuffix); 
            } 
        } 
        [XmlIgnore] 
        public bool IsReal 
        { 
            get 
            { 
                return 
                    ElectionMode == ElectionMode.Real || 
                    (ElectionMode == ElectionMode.None && ElectionDate.Date <= DateTime.Now.Date); 
            } 
        } 
        [XmlIgnore] 
        public string FileSuffix 
        { 
            get 
            { 
                return NullableFileSuffix ?? ""; 
            } 
        } 
        public string GetParentComitteeStamps() 
        { 
            var stampList = new List<String>(); 
            foreach (var election in Elections) 
                foreach (var stampCommittee in election.StampCommittees) 
                    if (stampCommittee.Num > 0) 
                    { 
                        string number = stampCommittee.Num.ToString("0000"); 
                        if (!stampList.Contains(number)) 
                            stampList.Add(number); 
                    } 
            var sb = new StringBuilder(); 
            foreach (var stamp in stampList.OrderBy(s => s)) 
            { 
                sb.Append(stamp); 
                sb.Append(", "); 
            } 
            if (sb.Length > 0) 
                sb.Length -= 2; 
            return sb.ToString(); 
        } 
        public DateTime MainVotingStartTime 
        { 
            get 
            { 
                var startTime = GetVotingModeStartTime(VotingMode.Main); 
                return new DateTime(startTime.Ticks); 
            } 
        } 
        public DateTime MainVotingEndTime 
        { 
            get 
            { 
                var endTime = GetVotingModeStartTime(VotingMode.Portable); 
                return new DateTime(endTime.Ticks); 
            } 
        } 
        [XmlIgnore] 
        public string Modes 
        { 
            get 
            { 
                return GetModeNames(VotingModes); 
            } 
        } 
        public static string GetModeNames(VotingMode[] modes) 
        { 
            var sb = new StringBuilder(); 
            foreach (var mode in modes) 
            { 
                switch (mode) 
                { 
                    case VotingMode.Main: 
                        sb.Append("Стационарный"); 
                        break; 
                    case VotingMode.Portable: 
                        sb.Append("Переносной"); 
                        break; 
                    default: 
                        sb.Append(mode); 
                        break; 
                } 
                sb.Append(", "); 
            } 
            if (sb.Length > 0) 
                sb.Length -= 2; 
            return sb.ToString(); 
        } 
        public const string UNDEFINED_ID = "-1"; 
        public string GetBlankIdByElectionNumber(string electionNum) 
        { 
            foreach (var blank in Blanks) 
            { 
                if (blank.Sections.Any(elNum => string.CompareOrdinal(electionNum, elNum) == 0)) 
                    return blank.Id; 
            } 
            return UNDEFINED_ID; 
        } 
        public int GetBlankMarkerByElectionNumber(string electionNum) 
        { 
            foreach (var blank in Blanks) 
            { 
                if (blank.Sections.Any(elNum => string.CompareOrdinal(electionNum, elNum) == 0)) 
                    return blank.Marker; 
            } 
            return -1; 
        } 
        public Election GetElectionByNum(string electionNum) 
        { 
            return Elections.FirstOrDefault( 
                election => string.CompareOrdinal(electionNum, election.ElectionId) == 0); 
        } 
        public Blank GetBlankByNumber(int number) 
        { 
            return Blanks.FirstOrDefault(blank => number == blank.Marker); 
        } 
        public bool HasChecks() 
        { 
            return Elections.Any(election => election.Protocol.Checks.Length > 0); 
        } 
        public bool HasElections() 
        { 
            return Elections.Any(election => !election.Disabled); 
        } 
        public int GetElectionIndex(Election election) 
        { 
            if (election == null) 
                return -1; 
            for (var i = 0; i < Elections.Length; ++i) 
                if (string.CompareOrdinal(Elections[i].ElectionId, election.ElectionId) == 0) 
                    return i; 
            return -1; 
        } 
        public string GetCommitteeMemberInitialByType(CommitteeMemberType type) 
        { 
            var member = CommitteeMembers.FirstOrDefault(m => m.Type == type); 
            if (member == null) 
                return null; 
            var name = member.LastName + " " + member.FirstName + " " + member.Patronymic; 
            return string.IsNullOrEmpty(name.Trim()) 
                       ? null 
                       : name; 
        } 
        #region Режимы голосования 
        [XmlIgnore] 
        public VotingMode FirstVotingMode 
        { 
            get 
            { 
                return VotingModes[0]; 
            } 
        } 
        public VotingMode GetNextVotingMode(VotingMode mode) 
        { 
            if (mode == VotingMode.Test) 
            { 
                return VotingModes[0]; 
            } 
            for (int i = 0; i < VotingModes.Length - 1; i++) 
            { 
                if (VotingModes[i] == mode) 
                { 
                    return VotingModes[i + 1]; 
                } 
            } 
            return VotingMode.Results; 
        } 
        public VotingMode GetPreviousVotingMode(VotingMode mode) 
        { 
            if (mode == VotingMode.Results) 
            { 
                return VotingModes[VotingModes.Length - 1]; 
            } 
            for (var i = VotingModes.Length - 1; i > 0; i--) 
            { 
                if (VotingModes[i] == mode) 
                { 
                    return VotingModes[i - 1]; 
                } 
            } 
            return VotingMode.Test; 
        } 
        public bool IsVotingModeExpired(VotingMode mode) 
        { 
            for (var i = 0; i < VotingModeTimes.Length; ++i) 
            { 
                var mt = VotingModeTimes[i]; 
                if (mt.mode == mode) 
                { 
                    if (++i == VotingModeTimes.Length) 
                        return false; 
                    mt = VotingModeTimes[i]; 
                } 
                else if (mt.mode < mode) 
                { 
                    continue; 
                } 
                var now = LocalTimeNow; 
                return mt.hour * 60 + mt.minute <= now.Hour * 60 + now.Minute; 
            } 
            return false; 
        } 
        public bool IsVotingModeTime(VotingMode mode) 
        { 
            for (var i = 0; i < VotingModeTimes.Length; ++i) 
            { 
                var modeTime = VotingModeTimes[i]; 
                if (modeTime.mode != mode) 
                    continue; 
                var nextModeTime = ++i < VotingModeTimes.Length 
                                       ? VotingModeTimes[i] 
                                       : ModeTime.MaxValue; 
                var now = LocalTimeNow; 
                var nowTotalMinutes = now.Hour * 60 + now.Minute; 
                return modeTime.TotalMinutes <= nowTotalMinutes && nowTotalMinutes < nextModeTime.TotalMinutes; 
            } 
            return false; 
        } 
        public TimeSpan GetTimeToModeStart(VotingMode mode) 
        { 
            var now = LocalTimeNow; 
            var nowTime = now - now.Date; 
            var timeToStart = GetVotingModeStartTime(mode) - nowTime; 
            return timeToStart < TimeSpan.Zero 
                       ? TimeSpan.Zero 
                       : timeToStart; 
        } 
        public TimeSpan GetVotingModeStartTime(VotingMode mode) 
        { 
            var modeTime = VotingModeTimes.FirstOrDefault(item => item.mode == mode); 
            if (modeTime != null) 
                return TimeSpan.FromHours(modeTime.hour) + TimeSpan.FromMinutes(modeTime.minute); 
            throw new ApplicationException("Не найдено время начала режима голосования " + mode); 
        } 
        public TimeSpan GetVotingStartRemainingTime() 
        { 
            var votingStartDateTime = ElectionDate + GetVotingModeStartTime(VotingMode.Main); 
            var remainingTime = votingStartDateTime - LocalTimeNow; 
            return remainingTime < TimeSpan.Zero 
                       ? TimeSpan.Zero 
                       : remainingTime; 
        } 
        public TimeSpan GetVotingEndRemainingTime() 
        { 
            var votingEndDateTime = ElectionDate + GetVotingModeStartTime(VotingMode.Portable); 
            var remainingTime = votingEndDateTime - LocalTimeNow; 
            return remainingTime < TimeSpan.Zero 
                       ? TimeSpan.Zero 
                       : remainingTime; 
        } 
        public bool IsVotingModeValidForBlank(Blank blank, VotingMode votingMode) 
        { 
            if (votingMode == VotingMode.Test) 
                return true; 
            return blank.Sections 
                .Select(electionNum => GetElectionByNum(electionNum)) 
                .SelectMany(election => election.VotingModes) 
                .Any(mode => mode == votingMode); 
        } 
        public bool VotingModeExists(VotingMode eMode) 
        { 
            return Elections.Any(election => election.VotingModes.Any(votingMode => votingMode == eMode)); 
        } 
        #endregion 
        #region Контрольные соотношения 
        #region Динамическая сборка для вычисления строк протокола 
        [XmlIgnore] 
        [NonSerialized] 
        private Assembly _protocolLinesAssembly; 
        public Assembly CompileAutoLinesAndChecksAssembly() 
        { 
            const string ASSEMBLY_TEXT = 
                @"using Croc.Core; 
                using Croc.Bpc.Voting;  
                namespace Croc.Bpc.DynamicTypes { "; 
            const string ASSEMBLY_TEXT_END = @"};"; 
            var assemblyText = new StringBuilder(8129); 
            assemblyText.Append(ASSEMBLY_TEXT); 
            assemblyText.Append(GetManagersClassForAssembly()); 
            foreach (Election oElection in Elections) 
            { 
                string sClassText; 
                foreach (Line oLine in oElection.Protocol.Lines) 
                { 
                    sClassText = oLine.BuildCheckTypeText(this); 
                    if (sClassText != null) 
                        assemblyText.Append(sClassText); 
                } 
                foreach (CheckExpression oCheck in oElection.Protocol.Checks) 
                { 
                    sClassText = oCheck.BuildCheckTypeText(oElection.Protocol); 
                    if (sClassText != null) 
                        assemblyText.Append(sClassText); 
                } 
            } 
            assemblyText.Append(ASSEMBLY_TEXT_END); 
            Assembly oAssembly = DynamicAssemblyHelper.Compile( 
                DynamicAssemblyHelper.SplitStringByLength(assemblyText.ToString()) 
                , new[] { "Croc.Core", "Croc.Bpc.Election" }); 
            return oAssembly; 
        } 
        public void BindAutoLinesAndChecksCountMethods() 
        { 
            if (_protocolLinesAssembly == null) 
                _protocolLinesAssembly = CompileAutoLinesAndChecksAssembly(); 
            foreach (Election oElection in Elections) 
            { 
                BindAutoLinesAndChecksCountMethods(oElection); 
            } 
        } 
        public void BindAutoLinesAndChecksCountMethods(Election oElection) 
        { 
            if (_protocolLinesAssembly == null) 
                _protocolLinesAssembly = CompileAutoLinesAndChecksAssembly(); 
            foreach (Line oLine in oElection.Protocol.Lines) 
                oLine.BindMethod(_protocolLinesAssembly); 
            foreach (CheckExpression oCheck in oElection.Protocol.Checks) 
                oCheck.BindMethod(_protocolLinesAssembly); 
        } 
        private static string GetManagersClassForAssembly() 
        { 
            return @"internal static class Managers 
            { 
                private static IElectionManager s_electionManager; 
                public static IElectionManager ElectionManager 
                { 
                    get 
                    { 
                        if (s_electionManager == null) 
                            s_electionManager = CoreApplication.Instance.GetSubsystemOrThrow<IElectionManager>(); 
                        return s_electionManager; 
                    } 
                } 
                private static IVotingResultManager s_votingResultManager; 
                public static IVotingResultManager VotingResultManager 
                { 
                    get 
                    { 
                        if (s_votingResultManager == null) 
                            s_votingResultManager = CoreApplication.Instance.GetSubsystemOrThrow<IVotingResultManager>(); 
                        return s_votingResultManager; 
                    } 
                } 
            }"; 
        } 
        #endregion 
        public void ExecuteChecks() 
        { 
            foreach (var election in Elections) 
                election.AllChecksDone(); 
        } 
        #endregion 
        #region Object methods overrides 
        public override bool Equals(object obj) 
        { 
            var otherSd = obj as SourceData; 
            return otherSd != null && string.CompareOrdinal(_hashCode, otherSd._hashCode) == 0; 
        } 
        public override int GetHashCode() 
        { 
            return _hashCode.GetHashCode(); 
        } 
        public override string ToString() 
        { 
            var text = new StringBuilder(1024); 
            text.Append('['); 
            text.Append("version="); 
            text.Append(Version); 
            text.Append(';'); 
            text.Append("uniqueID="); 
            text.Append(Id); 
            text.Append(';'); 
            text.Append("Blanks=["); 
            foreach (var blank in Blanks) 
                text.Append(blank); 
            text.Append("];"); 
            text.Append("Targets=["); 
            foreach (var target in Targets) 
                text.Append(target); 
            text.Append("];"); 
            text.Append("DateTime="); 
            text.Append(ElectionDate); 
            text.Append(';'); 
            text.Append("Mode="); 
            text.Append(ElectionMode); 
            text.Append(';'); 
            text.Append("TrainingFileSuffix="); 
            text.Append(TrainingModeFileSuffix); 
            text.Append(';'); 
            text.Append("FileSuffix="); 
            text.Append(RealModeFileSuffix); 
            text.Append(';'); 
            text.Append("Modes=["); 
            foreach (var mode in VotingModes) 
            { 
                text.Append(mode); 
                text.Append(','); 
            } 
            text.Append("];"); 
            text.Append("ModeTimes=["); 
            foreach (var mt in VotingModeTimes) 
            { 
                text.Append(mt); 
                text.Append(','); 
            } 
            text.Append("];"); 
            text.Append(']'); 
            return text.ToString(); 
        } 
        #endregion 
    } 
}

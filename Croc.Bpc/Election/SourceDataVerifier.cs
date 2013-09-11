using System; 
using System.Linq; 
using System.Collections; 
using System.Collections.Generic; 
using System.Text; 
using Croc.Bpc.Election.Config; 
using Croc.Bpc.RegExpressions; 
using Croc.Bpc.Voting; 
using Croc.Core; 
namespace Croc.Bpc.Election 
{ 
    internal class SourceDataVerifier 
    { 
        private readonly SourceData _sourceData; 
        private readonly VotingModeTimeConfigCollection _defaultVotingModeTimes; 
        public SourceDataVerifier(SourceData sourceData, VotingModeTimeConfigCollection defaultVotingModeTimes) 
        { 
            CodeContract.Requires(sourceData != null); 
            _sourceData = sourceData; 
            _defaultVotingModeTimes = defaultVotingModeTimes; 
        } 
        public void Repair() 
        { 
            RepairVotingModes(); 
            RepairVotingModeTimes(); 
            RepairAdditionalNumberForAllLines(); 
        } 
        public void Verify() 
        { 
            CheckBlanks(); 
            CheckSuffix(); 
            CheckVotingModes(); 
            CheckVotingModeTimes(); 
            СheckProtocol(); 
            TestCompile(); 
        } 
        #region Восстановления 
        private void RepairVotingModes() 
        { 
            if (_sourceData.VotingModes.Length <= 0) 
                _sourceData.VotingModes = new[] { VotingMode.Main, VotingMode.Portable }; 
            var modes = new List<VotingMode>(); 
            foreach (var votingMode in _sourceData.VotingModes) 
            { 
                if (!_sourceData.VotingModeExists(votingMode)) 
                    continue; 
                if (!modes.Contains(votingMode)) 
                    modes.Add(votingMode); 
            } 
            _sourceData.VotingModes = modes.ToArray(); 
        } 
        private void RepairVotingModeTimes() 
        { 
            var newModeTimes = new List<ModeTime>(); 
            foreach (var mode in _sourceData.VotingModes) 
            { 
                var timeFound = false; 
                foreach (var mt in _sourceData.VotingModeTimes) 
                { 
                    if (mt.mode != mode) continue; 
                    newModeTimes.Add(mt); 
                    timeFound = true; 
                    break; 
                } 
                if (timeFound) 
                    continue; 
                newModeTimes.Add(_defaultVotingModeTimes.GetModeTime(mode)); 
            } 
            if (newModeTimes.Count == 1 && newModeTimes[0].mode == VotingMode.Main) 
            { 
                var portableModeTime = 
                    _sourceData.VotingModeTimes.FirstOrDefault(vmTime => vmTime.mode == VotingMode.Portable) 
                    ?? 
                    _defaultVotingModeTimes.GetModeTime(VotingMode.Portable); 
                newModeTimes.Add(portableModeTime); 
            } 
            _sourceData.VotingModeTimes = newModeTimes.ToArray(); 
        } 
        private static readonly Dictionary<char, char> s_letterReplacementDict = 
            new Dictionary<char, char> 
                { 
                    {'a', 'а'}, 
                    {'p', 'р'}, 
                    {'c', 'с'}, 
                    {'b', 'б'}, 
                    {'y', 'у'}, 
                    {'h', 'н'}, 
                    {'k', 'к'}, 
                    {'x', 'х'}, 
                }; 
        private void RepairAdditionalNumberForAllLines() 
        { 
            foreach (var line in _sourceData.Elections.SelectMany(election => election.Protocol.Lines)) 
            { 
                if (string.IsNullOrEmpty(line.AdditionalNum)) 
                    continue; 
                var addNum = line.AdditionalNum[0]; 
                if ('а' <= addNum && addNum <= 'я') 
                    return; 
                if (s_letterReplacementDict.ContainsKey(addNum)) 
                { 
                    line.AdditionalNum = s_letterReplacementDict[addNum].ToString(); 
                } 
            } 
        } 
        #endregion 
        #region Проверки 
        private void TestCompile() 
        { 
            try 
            { 
                _sourceData.CompileAutoLinesAndChecksAssembly(); 
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced); 
            } 
            catch (Exception ex) 
            { 
                throw new SourceDataVerifierException("Ошибка при компиляции КС: " + ex.Message); 
            } 
        } 
        private void CheckBlanks() 
        { 
            var electionIdToBlankIdMap = new Dictionary<string, string>(); 
            var electionVotingModes = new Hashtable(); 
            foreach (var blank in _sourceData.Blanks) 
            { 
                if (blank.Sections.Length == 0) 
                    throw new SourceDataVerifierException(String.Format("Бланк {0} не содержит секций", blank.Id)); 
                electionVotingModes.Clear(); 
                foreach (var section in blank.Sections) 
                { 
                    var electionOnSect = _sourceData.GetElectionByNum(section); 
                    if (electionOnSect == null) 
                        throw new SourceDataVerifierException( 
                            String.Format("Бланк {0} ссылается на несуществующие выборы {1}", 
                                          blank.Id, section)); 
                    if (electionIdToBlankIdMap.ContainsKey(electionOnSect.ElectionId)) 
                        throw new SourceDataVerifierException( 
                            String.Format("Выборы {0} размещены на двух бланках: {1}, {2}", 
                                          electionOnSect.ElectionId, blank.Id, 
                                          electionIdToBlankIdMap[electionOnSect.ElectionId])); 
                    electionIdToBlankIdMap[electionOnSect.ElectionId] = blank.Id; 
                    foreach (var mode in electionOnSect.VotingModes) 
                    { 
                        if (electionVotingModes.Contains(mode)) 
                            electionVotingModes[mode] = (int)electionVotingModes[mode] + 1; 
                        else 
                            electionVotingModes.Add(mode, 1); 
                    } 
                } 
                foreach (var mode in electionVotingModes.Keys) 
                { 
                    if ((int)electionVotingModes[mode] != blank.Sections.Length) 
                    { 
                        throw new SourceDataVerifierException( 
                            String.Format("Не все выборы на бланке {0} содержат режим {1}", 
                            blank.Id, mode)); 
                    } 
                } 
            } 
            foreach (var election in _sourceData.Elections) 
            { 
                if (string.CompareOrdinal( 
                    _sourceData.GetBlankIdByElectionNumber(election.ElectionId), 
                    SourceData.UNDEFINED_ID) == 0) 
                { 
                    throw new SourceDataVerifierException( 
                        String.Format("Для выборов {0} не определен бланк", election.ElectionId)); 
                } 
            } 
        } 
        private void CheckSuffix() 
        { 
            if (!CheckSuffix(_sourceData.RealModeFileSuffix)) 
                throw new SourceDataVerifierException( 
                    "В суффиксе имени файла \"боевого\" режима разрешены только символы 0-9, a-z, A-Z, \"_\", \"-\""); 
            if (!CheckSuffix(_sourceData.TrainingModeFileSuffix)) 
                throw new SourceDataVerifierException( 
                    "В суффиксе имени файла тренировочного режима разрешены только символы 0-9, a-z, A-Z, \"_\", \"-\""); 
        } 
        private static bool CheckSuffix(string suffix) 
        { 
            return string.IsNullOrEmpty(suffix) || new SourceDataModeFileSuffixRegex().IsMatch(suffix); 
        } 
        private void CheckVotingModes() 
        { 
            if (_sourceData.VotingModes.Length <= 0) 
                throw new Exception("Не удалось сформировать список доступных режимов выборов по исходным данным"); 
            for (var vm = VotingMode.Main; vm <= VotingMode.Portable; vm++) 
            { 
                if (_sourceData.VotingModeExists(vm) && !_sourceData.VotingModes.Contains(vm)) 
                    throw new SourceDataVerifierException( 
                        String.Format("Режим {0} содержится в выборах, но не сожержится в режимах ИД", vm)); 
            } 
        } 
        private void CheckVotingModeTimes() 
        { 
            if (_sourceData.VotingModeTimes.Length > 1) 
            { 
                for (var i = 1; i < _sourceData.VotingModeTimes.Length; i++) 
                { 
                    var previous = _sourceData.VotingModeTimes[i - 1]; 
                    var current = _sourceData.VotingModeTimes[i]; 
                    if (previous.hour > current.hour || 
                        (previous.hour == current.hour && previous.minute > current.minute)) 
                    { 
                        throw new SourceDataVerifierException( 
                            string.Format("Время начала режима {0} не может быть раньше, чем время начала режима {1}", 
                                          _sourceData.VotingModes[i], _sourceData.VotingModes[i - 1])); 
                    } 
                } 
            } 
        } 
        private void СheckProtocol() 
        { 
            foreach (var election in _sourceData.Elections) 
            { 
                if (election.Protocol.Texts == null) 
                    continue; 
                var countFinal = 0; 
                var countUnfinal = 0; 
                var error = false; 
                var errText = new StringBuilder(); 
                errText.Append("В исходных данных для выборов \"" + election.Name + "\" были обнаружены ошибки:\r\n"); 
                var lines = new Hashtable(); 
                var dupplicateLines = new Hashtable(); 
                var candidates = new Hashtable(); 
                var dupplicateCand = new Hashtable(); 
                var lineCount = election.Protocol.Lines.Length; 
                foreach (var cand in election.Candidates) 
                { 
                    try 
                    { 
                        if (!candidates.ContainsKey(cand.Id)) 
                            candidates.Add(cand.Id, cand); 
                        else 
                            dupplicateCand.Add(cand.Id, cand); 
                    } 
                    catch 
                    { 
                        continue; 
                    } 
                } 
                foreach (var line in election.Protocol.Lines) 
                { 
                    try 
                    { 
                        if (!CheckLineAdditionalNumber(line)) 
                        { 
                            errText.Append(String.Format( 
                                    "Строка {0} содержит неверный дополнительный номер {1}\r\n", 
                                    line.Id, 
                                    line.AdditionalNum)); 
                            error = true; 
                        } 
                        if (line.Id != null) 
                        { 
                            if (!lines.ContainsKey(line.Id)) 
                                lines.Add(line.Id, line); 
                            else 
                                dupplicateLines.Add(line.Id, line); 
                        } 
                    } 
                    catch 
                    { 
                        continue; 
                    } 
                } 
                foreach (var text in election.Protocol.Texts) 
                { 
                    if (text.Final) 
                        countFinal++; 
                    else 
                        countUnfinal++; 
                    var candidatesView = new Hashtable(); 
                    var linesView = new Hashtable(); 
                    var dupplicateCandView = new Hashtable(); 
                    var dupplicateLinesView = new Hashtable(); 
                    var badNumbers = new Hashtable(); 
                    foreach (var line in text.VoteLines) 
                    { 
                        try 
                        { 
                            if (line.Type == VoteLineType.Vote) 
                            { 
                                if (!candidatesView.ContainsKey(line.ID)) 
                                { 
                                    candidatesView.Add(line.ID, line); 
                                } 
                                else 
                                { 
                                    dupplicateCandView.Add(line.ID, line); 
                                } 
                            } 
                            else 
                            { 
                                try 
                                { 
                                    int.Parse(line.ID); 
                                } 
                                catch 
                                { 
                                    badNumbers.Add(line.ID, line); 
                                    continue; 
                                } 
                                if (!linesView.ContainsKey(line.ID)) 
                                { 
                                    linesView.Add(line.ID, line); 
                                } 
                                else 
                                { 
                                    dupplicateLinesView.Add(line.ID, line); 
                                } 
                            } 
                        } 
                        catch 
                        { 
                            continue; 
                        } 
                    } 
                    if (countFinal > 1) 
                    { 
                        errText.Append("Найдено более одного элемента Text с установленным атрибутом Final\r\n"); 
                        error = true; 
                    } 
                    if (countUnfinal > 1) 
                    { 
                        errText.Append("Найдено более одного элемента Text со сброшенным атрибутом Final\r\n"); 
                        error = true; 
                    } 
                    if (linesView.Count > lineCount) 
                    { 
                        errText.Append( 
                            string.Format( 
                                "Количество описаний отображения строк {0} больше чем количество строк в протоколе {1} \r\n", 
                                linesView.Count, lineCount)); 
                        error = true; 
                    } 
                    if (badNumbers.Count > 0) 
                    { 
                        errText.Append("Недопустимые идентификаторы строк протокола:\r\n"); 
                        foreach (var id in badNumbers.Keys) 
                            errText.Append(id + "\r\n"); 
                        error = true; 
                    } 
                    foreach (var id in candidatesView.Keys) 
                    { 
                        if (!candidates.ContainsKey(id)) 
                        { 
                            errText.Append("Не обнаружен кандидат с идентификатором: " + id + "\r\n"); 
                            error = true; 
                        } 
                    } 
                    foreach (var id in linesView.Keys) 
                    { 
                        if (!lines.ContainsKey(id) &&  
                            string.CompareOrdinal((string)id, VoteTextLine.TOTAL_RECEIVED_VOTETEXTLINE_ID) != 0) 
                        { 
                            errText.Append("Не обнаружена строка протокола с идентификатором: " + id + "\r\n"); 
                            error = true; 
                        } 
                    } 
                    if (dupplicateCandView.Count > 0) 
                    { 
                        errText.Append("Обнаружены дублирующиеся идентификаторы описаний отображения кандидатов:\r\n"); 
                        foreach (var id in dupplicateCandView.Keys) 
                        { 
                            errText.Append(id + "\r\n"); 
                        } 
                        error = true; 
                    } 
                    if (dupplicateLinesView.Count > 0) 
                    { 
                        errText.Append("Обнаружены дублирующиеся идентификаторы описаний отображения строк протокола:\r\n"); 
                        foreach (var id in dupplicateLinesView.Keys) 
                            errText.Append(id + "\r\n"); 
                        error = true; 
                    } 
                } 
                if (dupplicateCand.Count > 0) 
                { 
                    errText.Append("Обнаружены дублирующиеся идентификаторы кандидатов:\r\n"); 
                    foreach (var id in dupplicateCand.Keys) 
                        errText.Append(id + "\r\n"); 
                    error = true; 
                } 
                if (dupplicateLines.Count > 0) 
                { 
                    errText.Append("Обнаружены дублирующиеся идентификаторы строк протокола:\r\n"); 
                    foreach (string id in dupplicateLines.Keys) 
                        errText.AppendLine(id); 
                    error = true; 
                } 
                if (error) 
                    throw new SourceDataVerifierException(errText.ToString()); 
            } 
        } 
        private static bool CheckLineAdditionalNumber(Line line) 
        { 
            if (String.IsNullOrEmpty(line.AdditionalNum)) 
                return true; 
            line.AdditionalNum = line.AdditionalNum.ToLower().Trim(); 
            if (line.AdditionalNum.ToCharArray().Length > 1) 
                return false; 
            var addNum = line.AdditionalNum[0]; 
            return 'а' <= addNum && addNum <= 'я'; 
        } 
        #endregion 
    } 
}

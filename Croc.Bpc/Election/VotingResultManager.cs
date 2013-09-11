using System; 
using System.Collections.Generic; 
using System.IO; 
using System.Runtime.Serialization.Formatters.Binary; 
using System.Threading; 
using System.Xml; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Election.Config; 
using Croc.Bpc.FileSystem; 
using Croc.Bpc.Scanner; 
using Croc.Bpc.Utils; 
using Croc.Bpc.Voting; 
using Croc.Core; 
using Croc.Core.Configuration; 
using Croc.Core.Diagnostics; 
using Croc.Core.Utils.IO; 
namespace Croc.Bpc.Election 
{ 
    [SubsystemConfigurationElementTypeAttribute(typeof(VotingResultManagerConfig))] 
    public sealed class VotingResultManager : StateSubsystem, IVotingResultManager 
    { 
        private VotingResultManagerConfig _config; 
        private IFileSystemManager _fileSystemManager; 
        private IElectionManager _electionManager; 
        private IScannersInfo _scannersInfo; 
        #region Инициализация 
        public override void Init(SubsystemConfig config) 
        { 
            _config = (VotingResultManagerConfig)config; 
            _fileSystemManager = Application.GetSubsystemOrThrow<IFileSystemManager>(); 
            _electionManager = Application.GetSubsystemOrThrow<IElectionManager>(); 
            _scannersInfo = Application.FindSubsystemImplementsInterfaceOrThrow<IScannersInfo>(); 
        } 
        public override void ApplyNewConfig(SubsystemConfig newConfig) 
        { 
            _config = (VotingResultManagerConfig)newConfig; 
        } 
        #endregion 
        #region IVotingResultManager members 
        public bool PackResultsEnabled 
        { 
            get 
            { 
                return _config.PackResults.Enabled; 
            } 
        } 
        public bool AddBadBlankToCounterValue 
        { 
            get 
            { 
                return _config.AddBadBlankToCounterValue.Enabled; 
            } 
        } 
        #region Результаты голосования 
        private static readonly object s_votingResultSync = new object(); 
        public VotingResults VotingResults 
        { 
            get; 
            private set; 
        } 
        public VotingResult LastVotingResult 
        { 
            get; 
            private set; 
        } 
        public void ResetLastVotingResult() 
        { 
            lock (s_votingResultSync) 
            { 
                LastVotingResult = VotingResult.Empty; 
            } 
        } 
        public void SetLastVotingResult(VotingResult votingResult) 
        { 
            lock (s_votingResultSync) 
            { 
                LastVotingResult = votingResult; 
            } 
        } 
        public void AddVotingResult( 
            VotingResult votingResult, 
            VotingMode votingMode, 
            int scannerSerialNumber) 
        { 
            Logger.LogVerbose(Message.Common_DebugCall); 
            lock (s_votingResultSync) 
            { 
                SetLastVotingResult(votingResult); 
                List<KeyValuePair<VoteKey, bool>> voteKeysForAdding = null; 
                while (true) 
                { 
                    try 
                    { 
                        if (voteKeysForAdding == null) 
                            voteKeysForAdding = GetVoteKeysForAdding(LastVotingResult, votingMode, scannerSerialNumber); 
                        for (var keyIndex = 0; keyIndex < voteKeysForAdding.Count; keyIndex++) 
                        { 
                            var item = voteKeysForAdding[keyIndex]; 
                            if (item.Value) 
                                continue; 
                            var votesCount = 0; 
                            try 
                            { 
                                votesCount = VotingResults.VotesCount(item.Key); 
                                VotingResults.AddVote(item.Key); 
                            } 
                            catch (ThreadAbortException) 
                            { 
                                Logger.LogWarning(Message.VotingResult_AddVotingResultAbort); 
                                Thread.ResetAbort(); 
                            } 
                            catch (Exception ex) 
                            { 
                                Logger.LogError(Message.VotingResult_AddVotingResultError, ex); 
                            } 
                            finally 
                            { 
                                if (VotingResults.VotesCount(item.Key) != votesCount + 1) 
                                    keyIndex--; 
                                else 
                                    voteKeysForAdding[keyIndex] = new KeyValuePair<VoteKey, bool>(item.Key, true); 
                            } 
                        } 
                        break; 
                    } 
                    catch (ThreadAbortException) 
                    { 
                        Logger.LogWarning(Message.VotingResult_AddVotingResultAbort); 
                        Thread.ResetAbort(); 
                    } 
                    catch (Exception ex) 
                    { 
                        Logger.LogError(Message.VotingResult_AddVotingResultError, ex); 
                    } 
                } 
            } 
            RaiseStateChanged(); 
        } 
        private List<KeyValuePair<VoteKey, bool>> GetVoteKeysForAdding( 
            VotingResult votingResult, 
            VotingMode votingMode, 
            int scannerSerialNumber) 
        { 
            Logger.LogVerbose(Message.Common_DebugCall); 
            var keys = new List<KeyValuePair<VoteKey, bool>>(); 
            var sourceData = _electionManager.SourceData; 
            var blank = (0 <= votingResult.BulletinNumber && votingResult.BulletinNumber < sourceData.Blanks.Length) 
                            ? sourceData.Blanks[votingResult.BulletinNumber] 
                            : null; 
            var bulletinVote = new VoteKey 
            { 
                ScannerSerialNumber = scannerSerialNumber, 
                VotingMode = votingMode, 
                BlankId = (blank != null ? blank.Id : votingResult.BulletinNumber.ToString()), 
                BlankType = votingResult.BlankType, 
            }; 
            if (bulletinVote.BlankType == BlankType.Valid) 
            { 
                if (votingResult.SectionsMarks == null || votingResult.SectionsValidity == null) 
                { 
                    bulletinVote.BlankType = BlankType.NoMarks; 
                } 
                else 
                { 
                    var invalid = true; 
                    for (var sectionIndex = 0; 
                         sectionIndex <= votingResult.SectionsMarks.GetUpperBound(0); 
                         sectionIndex++) 
                    { 
                        if (votingResult.SectionsValidity[sectionIndex] && 
                            votingResult.SectionsMarks[sectionIndex] != null && 
                            votingResult.SectionsMarks[sectionIndex].Length > 0) 
                        { 
                            invalid = false; 
                            break; 
                        } 
                    } 
                    if (invalid) 
                        bulletinVote.BlankType = BlankType.NoMarks; 
                } 
            } 
            if (bulletinVote.BlankType == BlankType.Valid) 
            { 
                bool isBulletinAdded = false; 
                for (var sectionIndex = 0; 
                     sectionIndex <= votingResult.SectionsMarks.GetUpperBound(0); 
                     sectionIndex++) 
                { 
                    if (!votingResult.SectionsValidity[sectionIndex]) 
                        continue; 
                    for (var markIndex = 0; markIndex < votingResult.SectionsMarks[sectionIndex].Length; markIndex++) 
                    { 
                        VoteKey candidateVote; 
                        if (!isBulletinAdded && markIndex == 0) 
                        { 
                            candidateVote = bulletinVote; 
                            isBulletinAdded = true; 
                        } 
                        else 
                            candidateVote = new VoteKey 
                            { 
                                ScannerSerialNumber = scannerSerialNumber, 
                                BlankId = blank.Id, 
                                VotingMode = votingMode 
                            }; 
                        candidateVote.ElectionNum = blank.Sections[sectionIndex]; 
                        candidateVote.CandidateId = sourceData.GetElectionByNum(blank.Sections[sectionIndex]). 
                            Candidates[votingResult.SectionsMarks[sectionIndex][markIndex]].Id; 
                        keys.Add(new KeyValuePair<VoteKey, bool>(candidateVote, false)); 
                    } 
                } 
            } 
            else 
            { 
                keys.Add(new KeyValuePair<VoteKey, bool>(bulletinVote, false)); 
            } 
            return keys; 
        } 
        #endregion 
        #region Сохранение результатов голосования 
        private const string PRERESULTS_DIRNAME = "preresult"; 
        private bool _isPreliminaryProtocol; 
        private string _votingResultProtocolData; 
        private string _votingResultProtocolFileName; 
        private string _votingResultProtocolFilePath; 
        private int _votingResultProtocolVersion; 
        private bool PackResults 
        { 
            get 
            { 
                return _electionManager.SourceData.Id != Guid.Empty && 
                    _config.PackResults.Enabled; 
            } 
        } 
        #region Формирование данных и имени файла протокола 
        public const string VOTINGRESULTPROTOCOL_XMLNS = "http://localhost/Schemas/xib.xsd"; 
        private string GenerateVotingResultProtocolFileName(DateTime timeStamp, int? electionNumber) 
        { 
            var electionNumberStr = electionNumber.HasValue && _config.IncludeElectionNumberToResultsFileName.Enabled 
                                        ? "_" + electionNumber 
                                        : string.Empty; 
            return string.Format("{0}Result-{1}-{2}{3}{4}.xml", 
                                 (_isPreliminaryProtocol ? "Pre" : string.Empty), 
                                 _electionManager.SourceData.Uik, 
                                 timeStamp.ToString("dd.MM.yyyy HH.mm.ss"), 
                                 _electionManager.SourceData.FileSuffix, 
                                 electionNumberStr) 
                .Replace(':', '.'); 
        } 
        private string GenerateVotingResultProtocolFilePath(string protocolDirPath) 
        { 
            if (_isPreliminaryProtocol) 
                protocolDirPath = Path.Combine(protocolDirPath, PRERESULTS_DIRNAME); 
            FileUtils.EnsureDirExists(protocolDirPath); 
            return Path.Combine(protocolDirPath, _votingResultProtocolFileName); 
        } 
        private string GenerateVotingResultProtocolData(DateTime timeStamp, Voting.Election election) 
        { 
            using (var stringWriter = new StringWriter()) 
            { 
                using (var xmlWriter = new XmlTextWriter(stringWriter)) 
                { 
                    xmlWriter.WriteStartElement("Xib", VOTINGRESULTPROTOCOL_XMLNS); 
                    xmlWriter.WriteAttributeString("uik", _electionManager.SourceData.Uik.ToString()); 
                    xmlWriter.WriteAttributeString("isGasVrn",  
                        XmlConvert.ToString(_electionManager.SourceData.IsGasVrn)); 
                    xmlWriter.WriteAttributeString( 
                        "ts", XmlConvert.ToString(timeStamp, XmlDateTimeSerializationMode.Local)); 
                    xmlWriter.WriteAttributeString( 
                        "version", XmlConvert.ToString(GetNextVotingResultProtocolVersion())); 
                    WriteScannersInfo(xmlWriter, election); 
                    WriteProtocolBody(xmlWriter, election); 
                    xmlWriter.WriteElementString("Check", String.Empty); 
                    xmlWriter.WriteEndElement(); 
                } 
                stringWriter.Flush(); 
                return stringWriter.ToString(); 
            } 
        } 
        private int GetNextVotingResultProtocolVersion() 
        { 
            _votingResultProtocolVersion++; 
            RaiseStateChanged(); 
            return _votingResultProtocolVersion; 
        } 
        private void WriteScannersInfo(XmlTextWriter xmlWriter, Voting.Election election) 
        { 
            var scannerArr = _scannersInfo.GetScannerInfos(); 
            foreach (var scanner in scannerArr) 
            { 
                var intScannerSerialNumber = Convert.ToInt32(scanner.SerialNumber); 
                xmlWriter.WriteStartElement("Scanner"); 
                xmlWriter.WriteAttributeString("n", scanner.SerialNumber); 
                var mask = new VoteKey 
                { 
                    ScannerSerialNumber = intScannerSerialNumber, 
                    BlankType = BlankType.Bad, 
                }; 
                var nufCount = VotingResults.VotesCount(mask); 
                xmlWriter.WriteAttributeString("nuf", XmlConvert.ToString(nufCount)); 
                if (election != null) 
                    WriteAttendInfo(xmlWriter, intScannerSerialNumber, election); 
                else 
                { 
                    foreach (var el in _electionManager.SourceData.Elections) 
                        WriteAttendInfo(xmlWriter, intScannerSerialNumber, el); 
                } 
                xmlWriter.WriteEndElement(); 
            } 
        } 
        private void WriteAttendInfo(XmlTextWriter xmlWriter, int scannerSerialNumber, Voting.Election election) 
        { 
            foreach (var blank in _electionManager.SourceData.Blanks) 
            { 
                if (election != null &&  
                    _electionManager.SourceData.GetBlankIdByElectionNumber(election.ElectionId) != blank.Id) 
                    continue; 
                var mask = new VoteKey 
                { 
                    ScannerSerialNumber = scannerSerialNumber, 
                    BlankId = blank.Id 
                }; 
                xmlWriter.WriteStartElement("Bulletin"); 
                xmlWriter.WriteAttributeString("id", blank.Id); 
                xmlWriter.WriteAttributeString("n", XmlConvert.ToString(blank.Marker)); 
                mask.BlankType = BlankType.Valid; 
                xmlWriter.WriteAttributeString("Valid", XmlConvert.ToString(VotingResults.VotesCount(mask))); 
                mask.BlankType = BlankType.NoMarks; 
                xmlWriter.WriteAttributeString("NoMarks", XmlConvert.ToString(VotingResults.VotesCount(mask))); 
                mask.BlankType = BlankType.TooManyMarks; 
                xmlWriter.WriteAttributeString("TooManyMarks", XmlConvert.ToString(VotingResults.VotesCount(mask))); 
                mask.BlankType = BlankType.BadMode; 
                xmlWriter.WriteAttributeString("BadMode", XmlConvert.ToString(VotingResults.VotesCount(mask))); 
                xmlWriter.WriteEndElement(); 
            } 
        } 
        private void WriteProtocolBody(XmlTextWriter xmlWriter, Voting.Election election) 
        { 
            xmlWriter.WriteStartElement("Protocol"); 
            xmlWriter.WriteAttributeString("final", XmlConvert.ToString(!_isPreliminaryProtocol)); 
            if (election != null) 
                WriteElectionInfo(xmlWriter, election); 
            else 
            { 
                foreach (var el in _electionManager.SourceData.Elections) 
                    WriteElectionInfo(xmlWriter, el); 
            } 
            xmlWriter.WriteEndElement(); 
        } 
        private void WriteElectionInfo(XmlTextWriter xmlWriter, Voting.Election election) 
        { 
            xmlWriter.WriteStartElement("Election"); 
            xmlWriter.WriteAttributeString("n", election.ExternalNumber); 
            xmlWriter.WriteAttributeString("parentid", election.ParentComittee.ParentID); 
            xmlWriter.WriteAttributeString("e-mail", election.ParentComittee.EMail); 
            if (!_isPreliminaryProtocol) 
            { 
                foreach (var line in election.Protocol.Lines) 
                { 
                    xmlWriter.WriteStartElement("Line"); 
                    xmlWriter.WriteAttributeString("n", XmlConvert.ToString(line.Num)); 
                    if (!string.IsNullOrEmpty(line.AdditionalNum)) 
                        xmlWriter.WriteAttributeString("a", line.AdditionalNum); 
                    xmlWriter.WriteString(XmlConvert.ToString(line.Value.GetValueOrDefault(0))); 
                    xmlWriter.WriteEndElement(); 
                } 
            } 
            var showDisabledCandidates = !string.IsNullOrEmpty(election.Protocol.DisabledString); 
            foreach (var candidate in election.Candidates) 
            { 
                if (candidate.Disabled && !showDisabledCandidates) 
                    continue; 
                xmlWriter.WriteStartElement("Candidate"); 
                xmlWriter.WriteAttributeString("n", candidate.Id); 
                if (candidate.Disabled) 
                    xmlWriter.WriteAttributeString("disabled", XmlConvert.ToString(true)); 
                var mask = new VoteKey 
                { 
                    ElectionNum = election.ElectionId, 
                    CandidateId = candidate.Id 
                }; 
                var votesCount = VotingResults.VotesCount(mask); 
                xmlWriter.WriteString(XmlConvert.ToString(votesCount)); 
                xmlWriter.WriteEndElement(); 
            } 
            xmlWriter.WriteEndElement(); 
        } 
        #endregion 
        public void GeneratePreliminaryVotingResultProtocol() 
        { 
            var timeStamp = DateTime.Now; 
            _isPreliminaryProtocol = true; 
            _votingResultProtocolFileName = GenerateVotingResultProtocolFileName(timeStamp, null); 
            _votingResultProtocolData = GenerateVotingResultProtocolData(timeStamp, null); 
            SaveVotingResultProtocolToLocalDir(); 
        } 
        public void GenerateVotingResultProtocol(Voting.Election election) 
        { 
            _isPreliminaryProtocol = false; 
            var timeStamp = DateTime.Now; 
            var electionIndex = _electionManager.SourceData.GetElectionIndex(election); 
            _votingResultProtocolFileName = GenerateVotingResultProtocolFileName(timeStamp, electionIndex + 1); 
            _votingResultProtocolData = GenerateVotingResultProtocolData(timeStamp, election); 
            SaveVotingResultProtocolToLocalDir(); 
        } 
        private void SaveVotingResultProtocolToLocalDir() 
        { 
            try 
            { 
                var filePath = GenerateVotingResultProtocolFilePath( 
                    _fileSystemManager.GetDataDirectoryPath(FileType.VotingResultProtocol)); 
                if (!_fileSystemManager.WriteTextToFile( 
                        filePath, 
                        FileMode.Create, 
                        _votingResultProtocolData, 
                        true)) 
                    throw new Exception("Не удалось записать данные в файл"); 
                Logger.LogInfo(Message.VotingResult_SaveVotingResultToLocalDirSucceeded, _votingResultProtocolFileName); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError( 
                    Message.VotingResult_SaveVotingResultToLocalDirFailed, ex, _votingResultProtocolFileName); 
            } 
        } 
        public bool FindFilePathToSaveVotingResultProtocol() 
        { 
            Logger.LogVerbose(Message.VotingResult_FindFilePathToSaveVotingResultProtocol); 
            try 
            { 
                var dirPath = _electionManager.FindDirPathToSaveVotingResultProtocol( 
                    _config.NeedSourceDataForSaveResults.Enabled); 
                Logger.LogVerbose(Message.VotingResult_FindFilePathToSaveVotingResultProtocolDone, dirPath); 
                if (string.IsNullOrEmpty(dirPath)) 
                    return false; 
                _votingResultProtocolFilePath = GenerateVotingResultProtocolFilePath(dirPath); 
                return true; 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.VotingResult_FindFilePathToSaveVotingResultProtocolFailed, ex); 
                return false; 
            } 
        } 
        public bool SaveVotingResultProtocol() 
        { 
            var filePath = _votingResultProtocolFilePath; 
            try 
            { 
                if (string.IsNullOrEmpty(_votingResultProtocolFilePath) || 
                    string.IsNullOrEmpty(_votingResultProtocolData)) 
                    throw new InvalidOperationException( 
                        "Путь к файлу для сохранения протокола и/или данные протокола не определены"); 
                if (PackResults) 
                { 
                    filePath = Path.ChangeExtension(_votingResultProtocolFilePath, "bin"); 
                    var arr = ZipCompressor.Compress( 
                        _votingResultProtocolData, 
                        _votingResultProtocolFileName, 
                        ZipCompressor.DEFAULT_COMPRESS_LEVEL, 
                        _electionManager.SourceData.Id.ToString("N")); 
                    if (!_fileSystemManager.SafeSerialization( 
                        arr, new BinaryFormatter(), 
                        filePath, false, false)) 
                        throw new Exception("Ошибка записи: " + filePath); 
                } 
                else 
                { 
                    if (!_fileSystemManager.WriteTextToFile( 
                        filePath, 
                        FileMode.Create, 
                        _votingResultProtocolData, 
                        false)) 
                        throw new Exception("Ошибка записи: " + filePath); 
                    SystemHelper.SyncFileSystem(); 
                } 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.VotingResult_SaveVotingResultToFlashFailed, ex); 
                return false; 
            } 
            if (_config.ResultsReserveCopyCount.Value > 0) 
            { 
                try 
                { 
                    for (var i = 0; i < _config.ResultsReserveCopyCount.Value; ++i) 
                    { 
                        File.Copy(filePath, filePath + i, true); 
                    } 
                } 
                catch (Exception ex) 
                { 
                    Logger.LogWarning(Message.VotingResult_SaveVotingResultReserveCopiesToFlashFailed, ex); 
                } 
                SystemHelper.SyncFileSystem(); 
            } 
            return true; 
        } 
        #endregion 
        #endregion 
        #region StateSubsystem overrides 
        private const int VOTINGRESULTS_STATEINDEX = 0; 
        private const int LASTVOTINGRESULT_STATEINDEX = 1; 
        private const int VOTINGRESULTPROTOCOLVERSION_STATEINDEX = 2; 
        public override object GetState() 
        { 
            return new object[] 
                       { 
                           VotingResults, 
                           LastVotingResult, 
                           _votingResultProtocolVersion 
                       }; 
        } 
        public override void RestoreState(object state) 
        { 
            var arr = (object[])state; 
            VotingResults = (VotingResults)arr[VOTINGRESULTS_STATEINDEX]; 
            VotingResults.Init(Logger, _config.AddBadBlankToCounterValue.Enabled); 
            LastVotingResult = (VotingResult)arr[LASTVOTINGRESULT_STATEINDEX]; 
            _votingResultProtocolVersion = (int)arr[VOTINGRESULTPROTOCOLVERSION_STATEINDEX]; 
        } 
        public override SubsystemStateAcceptanceResult AcceptNewState(object newState) 
        { 
            try 
            { 
                if (newState == null) 
                { 
                    Logger.LogVerbose(Message.VotingResult_NewStateRejectedBecauseIsNull); 
                    return SubsystemStateAcceptanceResult.Rejected; 
                } 
                var arr = (object[]) newState; 
                _votingResultProtocolVersion = (int) arr[VOTINGRESULTPROTOCOLVERSION_STATEINDEX]; 
                var newVotingResults = (VotingResults) arr[VOTINGRESULTS_STATEINDEX]; 
                var mergeResult = VotingResults.Merge(newVotingResults); 
                if (mergeResult == VotingResults.MergeResult.OurContainMoreVotes) 
                { 
                    Logger.LogVerbose(Message.VotingResult_NewStateAcceptedByMerge); 
                    return SubsystemStateAcceptanceResult.AcceptedByMerge; 
                } 
                Logger.LogVerbose(Message.VotingResult_NewStateAccepted); 
                return SubsystemStateAcceptanceResult.Accepted; 
            } 
            catch (Exception ex) 
            { 
                const string MSG = "Ошибка принятия нового состояния менеджера выборов"; 
                Logger.LogError(Message.VotingResult_NewStateAссeptError, ex); 
                throw new Exception(MSG, ex); 
            } 
        } 
        protected override void ResetStateInternal() 
        { 
            VotingResults = new VotingResults(); 
            VotingResults.Init(Logger, _config.AddBadBlankToCounterValue.Enabled); 
            LastVotingResult = VotingResult.Empty; 
            _votingResultProtocolVersion = 0; 
        } 
        #endregion 
    } 
}

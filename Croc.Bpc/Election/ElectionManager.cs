using System; 
using System.Collections.Generic; 
using System.IO; 
using System.Linq; 
using System.Security.Cryptography; 
using System.Text; 
using System.Threading; 
using System.Xml; 
using System.Xml.Serialization; 
using Croc.Bpc.Diagnostics; 
using Croc.Bpc.Election.Config; 
using Croc.Bpc.FileSystem; 
using Croc.Bpc.Recognizer; 
using Croc.Bpc.RegExpressions; 
using Croc.Bpc.Synchronization; 
using Croc.Bpc.Utils; 
using Croc.Bpc.Voting; 
using Croc.Core; 
using Croc.Core.Configuration; 
using Croc.Core.Extensions; 
namespace Croc.Bpc.Election 
{ 
    [SubsystemConfigurationElementTypeAttribute(typeof(ElectionManagerConfig))] 
    public class ElectionManager : StateSubsystem, IElectionManager 
    { 
        private ElectionManagerConfig _config; 
        private IVotingResultManager _votingResultManager; 
        private IRecognitionManager _recognitionManager; 
        private IFileSystemManager _fileSystemManager; 
        private ISynchronizationManager _syncManager; 
        #region Инициализация 
        public override void Init(SubsystemConfig config) 
        { 
            _config = (ElectionManagerConfig)config; 
            _votingResultManager = Application.GetSubsystemOrThrow<IVotingResultManager>(); 
            _recognitionManager = Application.GetSubsystemOrThrow<IRecognitionManager>(); 
            _fileSystemManager = Application.GetSubsystemOrThrow<IFileSystemManager>(); 
            _syncManager = Application.GetSubsystemOrThrow<ISynchronizationManager>(); 
        } 
        public override void ApplyNewConfig(SubsystemConfig newConfig) 
        { 
            _config = (ElectionManagerConfig)newConfig; 
        } 
        #endregion 
        #region IQuietMode 
        public bool QuietMode { get; set; } 
        #endregion 
        #region IElectionManager members 
        #region Общие св-ва 
        public virtual DateTime LocalTimeNow 
        { 
            get 
            { 
                return DateTime.Now; 
            } 
        } 
        private VotingMode _currentVotingMode; 
        public VotingMode CurrentVotingMode 
        { 
            get 
            { 
                return _currentVotingMode; 
            } 
            set 
            { 
                if (_currentVotingMode == value) 
                    return; 
                SetCurrentVotingMode(value); 
                RaiseStateChanged(); 
            } 
        } 
        private void SetCurrentVotingMode(VotingMode newVotingMode) 
        { 
            var args = new VotingModeChangedEventArgs( 
                _currentVotingMode, 
                newVotingMode, 
                _votingResultManager.VotingResults.VotesCount( 
                    new VoteKey 
                        { 
                            VotingMode = _currentVotingMode 
                        })); 
            _currentVotingMode = newVotingMode; 
            if (newVotingMode > VotingMode.Test) 
                _votingResultManager.VotingResults.ClearTestData(); 
            VotingModeChanged.RaiseEvent(this, args); 
        } 
        public event EventHandler<VotingModeChangedEventArgs> VotingModeChanged; 
        public bool NeedExecuteCheckExpressions 
        { 
            get 
            { 
                return _config.ExecuteCheckExpressions.Value; 
            } 
        } 
        public bool СanRestoreCandidateCanseledInSd 
        { 
            get 
            { 
                return _config.СanRestoreCandidateCanseledInSd.Value; 
            } 
        } 
        #endregion 
        #region Получение информации по исходным данным 
        public SourceData SourceData { get; private set; } 
        private SourceDataFileDescriptor _sourceDataFileDescriptor; 
        private bool _isSourceDataCorrect; 
        public bool IsSourceDataCorrect 
        { 
            get 
            { 
                return _isSourceDataCorrect; 
            } 
            set 
            { 
                _isSourceDataCorrect = value; 
                RaiseStateChanged(); 
            } 
        } 
        private SourceDataChangesCache _sourceDataChangesCache; 
        public bool HasSourceDataChanged 
        { 
            get  
            { 
                return _sourceDataChangesCache != null && !_sourceDataChangesCache.IsEmpty; 
            } 
        } 
        public ElectionDayСomming IsElectionDay() 
        { 
            return IsElectionDay(SourceData); 
        } 
        public ElectionDayСomming IsElectionDay(SourceData sourceData) 
        { 
            if (sourceData == null) 
                return ElectionDayСomming.NotComeYet; 
            var electionDay = sourceData.ElectionDate.Date; 
            var currentDay = sourceData.LocalTimeNow.Date; 
            if (currentDay < electionDay) 
                return ElectionDayСomming.NotComeYet; 
            if (currentDay == electionDay) 
                return ElectionDayСomming.ItsElectionDay; 
            var lastExtraElectionDay = electionDay.AddDays(_config.ElectionDayDuration.Value - 1).Date; 
            if (currentDay <= lastExtraElectionDay) 
                return ElectionDayСomming.ItsExtraElectionDay; 
            return ElectionDayСomming.AlreadyPassed; 
        } 
        #endregion 
        #region Загрузка исходных данных 
        private string _rootDataDirPath; 
        public bool HasSourceData() 
        { 
            return SourceData != null; 
        } 
        #region Поиск файла с исходными данными 
        public string[] GetSourceDataSearchPaths() 
        { 
            var result = new List<string>(); 
            foreach (PathConfig path in _config.DataDirectories.RootPaths) 
                result.AddRange(GetDirectoriesByWildcard(path)); 
            for (var i = 0; i < result.Count; i++) 
                result[i] = Path.Combine(result[i], _config.DataDirectories.SourceDataDirName); 
            return result.ToArray(); 
        } 
        public bool FindSourceDataFile( 
            WaitHandle stopSearchingEvent, out SourceDataFileDescriptor sdFileDescriptor) 
        { 
            const string SOURCE_DATA_FILE_NAME_MASK = "*-?*.bin"; 
            const string UIK_GROUP_SD_FILE_NAME_PATTERN = "uik"; 
            sdFileDescriptor = null; 
            var sourceDataDirName = _config.DataDirectories.SourceDataDirName; 
            var sourceDataDirNameUpper = sourceDataDirName.ToUpper(); 
            var sourceDataDirNames = new[] 
            { 
                sourceDataDirName,                                          // как в конфиге 
                sourceDataDirNameUpper,                                     // БОЛЬШИМИ буквами 
                sourceDataDirNameUpper[0] + sourceDataDirName.Substring(1)  // первая заглавная, остальные как в конфиге 
            }; 
            var tryCount = 0;   // счетчик попыток 
            while (true) 
            { 
                tryCount++; 
                foreach (var sdName in sourceDataDirNames) 
                { 
                    foreach (PathConfig item in _config.DataDirectories.RootPaths) 
                    { 
                        try 
                        { 
                            foreach (var rootDataDirPath in GetDirectoriesByWildcard(item)) 
                            { 
                                var sourceDataDirPath = Path.Combine(rootDataDirPath, sdName); 
                                try 
                                { 
                                    var sourceDataDirInfo = new DirectoryInfo(sourceDataDirPath); 
                                    if (!sourceDataDirInfo.Exists) 
                                        continue; 
                                    Logger.LogInfo(Message.Election_SearchSourceDataInDir, sourceDataDirPath); 
                                    var files = sourceDataDirInfo.GetFiles(SOURCE_DATA_FILE_NAME_MASK); 
                                    foreach (var file in files) 
                                    { 
                                        Logger.LogVerbose(Message.Election_CheckSourceDataFile, file); 
                                        var regex = new SourceDataFileNameRegex(); 
                                        var match = regex.Match(file.Name); 
                                        if (!match.Success) 
                                            continue; 
                                        sdFileDescriptor = new SourceDataFileDescriptor( 
                                            file.FullName, 
                                            file.Length, 
                                            int.Parse(match.Groups[UIK_GROUP_SD_FILE_NAME_PATTERN].Value), 
                                            _syncManager.LocalScannerSerialNumber); 
                                        _rootDataDirPath = rootDataDirPath; 
                                        return true; 
                                    } 
                                } 
                                catch (Exception ex) 
                                { 
                                    Logger.LogWarning( 
                                        Message.Election_FindSourceDataError, sourceDataDirPath, ex.Message, tryCount); 
                                } 
                            } 
                        } 
                        catch (Exception ex) 
                        { 
                            Logger.LogWarning( 
                                Message.Election_FindSourceDataError, 
                                Path.Combine(item.RootPath, item.Wildcard), 
                                ex.Message, 
                                tryCount); 
                        } 
                    } 
                } 
                if (tryCount >= _config.SourceDataFileSearch.MaxTryCount) 
                    return false; 
                if (stopSearchingEvent == null) 
                { 
                    Thread.Sleep(_config.SourceDataFileSearch.Delay); 
                } 
                else if (stopSearchingEvent.WaitOne(_config.SourceDataFileSearch.Delay)) 
                { 
                    return false; 
                } 
            } 
        } 
        private static IEnumerable<string> GetDirectoriesByWildcard(PathConfig path) 
        { 
            if (!Directory.Exists(path.RootPath)) 
                return new string[0]; 
            if (string.IsNullOrEmpty(path.Wildcard)) 
                return new[] { path.RootPath }; 
            return Directory.GetDirectories(path.RootPath, path.Wildcard); 
        } 
        public string FindDirPathToSaveVotingResultProtocol(bool needSourceDataForSaveResults) 
        { 
            SourceDataFileDescriptor sdFileDescriptor; 
            var sdSearchResult = FindSourceDataFile(null, out sdFileDescriptor); 
            if (!sdSearchResult && needSourceDataForSaveResults) 
                return null; 
            string resultRootDir = null; 
            if (sdSearchResult && needSourceDataForSaveResults) 
            { 
                if (!_sourceDataFileDescriptor.IsPointToSameFile(sdFileDescriptor)) 
                    return null; 
                resultRootDir = _rootDataDirPath; 
            } 
            else 
            { 
                if (sdSearchResult) 
                { 
                    resultRootDir = _rootDataDirPath; 
                } 
                else 
                { 
                    foreach (var directory in GetSourceDataSearchPaths()) 
                    { 
                        var containsDirPath = directory.Replace(_config.DataDirectories.SourceDataDirName, ""); 
                        if (Directory.Exists(containsDirPath)) 
                        { 
                            resultRootDir = containsDirPath; 
                            break; 
                        } 
                    } 
                    if (String.IsNullOrEmpty(resultRootDir)) 
                        return null; 
                } 
            } 
            return Path.Combine(resultRootDir, _config.DataDirectories.VotingResultDirName); 
        } 
        #endregion 
        #region Загрузка исходных данных из файла 
        public bool LoadSourceDataFromFile( 
            SourceDataFileDescriptor sdFileDescriptor, bool needVerify, out SourceData sd) 
        { 
            needVerify = !QuietMode && needVerify; 
            try 
            { 
                using (var stream = new FileStream(sdFileDescriptor.FilePath, FileMode.Open, FileAccess.Read)) 
                { 
                    var uncompressedStream = ZipCompressor.Uncompress(stream); 
                    sd = DeserializeSourceData(uncompressedStream); 
                    uncompressedStream.Close(); 
                    if (sd.Id == Guid.Empty) 
                    { 
                        if (_votingResultManager.PackResultsEnabled) 
                            throw new SourceDataVerifierException( 
                                "Исходные данные не содержат уникального идентификатора"); 
                        sd.Id = GenerateSourceDataId(sd); 
                    } 
                } 
                sd.Init(sdFileDescriptor.Uik); 
                var verifier = new SourceDataVerifier(sd, _config.DefaultVotingModeTimes); 
                Logger.LogVerbose(Message.Election_SourceDataRepairing); 
                verifier.Repair(); 
                if (needVerify) 
                { 
                    Logger.LogVerbose(Message.Election_SourceDataVerifying); 
                    verifier.Verify(); 
                } 
            } 
            catch (SourceDataVerifierException ex) 
            { 
                sd = null; 
                Logger.LogWarning(Message.Election_SourceDataIncorrect, sdFileDescriptor.FilePath, ex.Message); 
                return false; 
            } 
            catch (Exception ex) 
            { 
                sd = null; 
                Logger.LogWarning(Message.Election_SourceDataLoadFromFileFailed, ex, sdFileDescriptor.FilePath); 
                return false; 
            } 
            if (needVerify) 
            { 
                try 
                { 
                    Logger.LogVerbose(Message.Election_CheckCreateModel); 
                    _recognitionManager.CheckCreateModel(sd); 
                } 
                catch (Exception ex) 
                { 
                    sd = null; 
                    Logger.LogWarning(Message.Election_SourceDataIncorrect, sdFileDescriptor.FilePath, ex.Message); 
                    return false; 
                } 
            } 
            Logger.LogVerbose(Message.Election_SourceDataSuccessfullyLoadedFromFile, sdFileDescriptor.FilePath); 
            return true; 
        } 
        private static SourceData DeserializeSourceData(Stream uncompressedStream) 
        { 
            Stream replacedStream; 
            using (var xmlTextReader = new XmlTextReader(uncompressedStream, XmlNodeType.Document, null)) 
            { 
                replacedStream = GetReplaceXmlStream(xmlTextReader); 
                replacedStream.Position = 0; 
            } 
            using (var replaceTextReader = new XmlTextReader(replacedStream, XmlNodeType.Document, null)) 
            { 
                var settings = new XmlReaderSettings(); 
                settings.Schemas.Add(SourceData.XMLNS, SourceData.SHEMA_PATH); 
                var xmlReader = XmlReader.Create(replaceTextReader, settings); 
                var serializer = new XmlSerializer(typeof(SourceData), SourceData.XMLNS); 
                return (SourceData)serializer.Deserialize(xmlReader); 
            } 
        } 
        private static Stream GetReplaceXmlStream(XmlTextReader reader) 
        { 
            var result = new MemoryStream(); 
            var writer = new XmlTextWriter(result, Encoding.Unicode); 
            while (reader.Read()) 
            { 
                switch (reader.NodeType) 
                { 
                    case XmlNodeType.XmlDeclaration: 
                        writer.WriteStartDocument(); 
                        break; 
                    case XmlNodeType.Whitespace: 
                        writer.WriteWhitespace(reader.Value); 
                        break; 
                    case XmlNodeType.Element: 
                        writer.WriteStartElement(reader.Name); 
                        WriteXmlElementAttributes(reader, writer); 
                        if (reader.IsEmptyElement) 
                            writer.WriteEndElement(); 
                        break; 
                    case XmlNodeType.EndElement: 
                        writer.WriteEndElement(); 
                        break; 
                    case XmlNodeType.CDATA: 
                        writer.WriteCData(reader.Value); 
                        break; 
                    case XmlNodeType.Text: 
                        writer.WriteValue(reader.Value); 
                        break; 
                } 
            } 
            writer.WriteEndDocument(); 
            writer.Flush(); 
            return result; 
        } 
        private static void WriteXmlElementAttributes(XmlTextReader reader, XmlTextWriter writer) 
        { 
            const string OLD_SOURCE_DATA_XMLNS = "www.croc.ru"; 
            while (reader.MoveToNextAttribute()) 
            { 
                writer.WriteStartAttribute(reader.Name); 
                writer.WriteValue(reader.Value.Replace(OLD_SOURCE_DATA_XMLNS, "localhost")); 
                writer.WriteEndAttribute(); 
            } 
            reader.MoveToElement(); 
        } 
        private static string SerializeSourceData(SourceData sd) 
        { 
            var oSerializer = new XmlSerializer(typeof(SourceData), SourceData.XMLNS); 
            using (var memStream = new MemoryStream()) 
            { 
                var writer = new StreamWriter(memStream); 
                oSerializer.Serialize(writer, sd); 
                memStream.Seek(0, SeekOrigin.Begin); 
                var reader = new StreamReader(memStream); 
                return reader.ReadToEnd(); 
            } 
        } 
        private static Guid GenerateSourceDataId(SourceData sd) 
        { 
            var data = SerializeSourceData(sd); 
            var hash = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(data)); 
            var bytes = hash.Take(16).ToArray(); 
            return new Guid(bytes); 
        } 
        #endregion 
        #region Установка исходных данных 
        public bool SetSourceData(SourceData sourceData, SourceDataFileDescriptor sourceDataFileDescriptor) 
        { 
            CodeContract.Requires(sourceData != null); 
            CodeContract.Requires(sourceDataFileDescriptor != null); 
            try 
            { 
                SetSourceDataInternal( 
                    sourceData, 
                    sourceDataFileDescriptor, 
                    null, 
                    false); 
            } 
            catch (Exception ex) 
            { 
                Logger.LogError(Message.Election_SetSourceDataFailed, ex); 
                return false; 
            } 
            RaiseStateChanged(); 
            Logger.LogInfo( 
                Message.Election_SetSourceDataSucceeded, 
                SourceData.Uik, 
                SourceData.ElectionDate.ToString("dd.MM.yyyy"), 
                SourceData.GetVotingModeStartTime(VotingMode.Main), 
                SourceData.GetVotingModeStartTime(VotingMode.Portable)); 
            return true; 
        } 
        private void SetSourceDataInternal( 
            SourceData newSd,  
            SourceDataFileDescriptor newSdFileDescriptor, 
            SourceDataChangesCache newSdChangesCache, 
            bool newIsSdCorrect) 
        { 
            var newSdFileDescriptorDiffer = SetSourceDataFileDescriptor(newSdFileDescriptor); 
            var newSdChangesCacheDiffer = SetSourceDataChangesCache(newSdChangesCache); 
            var newSdDiffer = true; 
            if (newSd == null) 
            { 
                if (newSdFileDescriptor == null) 
                { 
                    if (SourceData == null) 
                        newSdDiffer = false; 
                } 
                else if (newSdFileDescriptorDiffer) 
                { 
                    LoadSourceDataFromFile(_sourceDataFileDescriptor, false, out newSd); 
                } 
                else 
                { 
                    newSdDiffer = false; 
                } 
            } 
            if (newSdDiffer || newSdChangesCacheDiffer) 
            { 
                if (SourceData != null) 
                { 
                    UnsubscribeFromSourceDataChanges(); 
                } 
                if (newSdDiffer) 
                    SourceData = newSd; 
                if (SourceData != null) 
                { 
                    _sourceDataChangesCache.ApplyChanges(SourceData); 
                    SubscribeToSourceDataChanges(); 
                } 
            } 
            _isSourceDataCorrect = newIsSdCorrect; 
        } 
        private bool SetSourceDataChangesCache(SourceDataChangesCache newSdChangesCache) 
        { 
            if (newSdChangesCache == null) 
                return false; 
            if (_sourceDataChangesCache == null) 
            { 
                _sourceDataChangesCache = newSdChangesCache; 
                return true; 
            } 
            if (_sourceDataChangesCache.Equals(newSdChangesCache)) 
                return false; 
            _sourceDataChangesCache = newSdChangesCache; 
            return true; 
        } 
        private bool SetSourceDataFileDescriptor(SourceDataFileDescriptor newSdFileDescriptor) 
        { 
            var oldIsNull = (_sourceDataFileDescriptor == null); 
            if (newSdFileDescriptor == null) 
            { 
                if (oldIsNull) 
                    return false; 
                _sourceDataFileDescriptor = null; 
                return true; 
            } 
            var stateFilePath = Path.Combine( 
                _fileSystemManager.GetDataDirectoryPath(FileType.State), 
                Path.GetFileName(newSdFileDescriptor.FilePath)); 
            _sourceDataFileDescriptor = new SourceDataFileDescriptor( 
                stateFilePath, 
                newSdFileDescriptor.FileSize, 
                newSdFileDescriptor.Uik, 
                _syncManager.LocalScannerSerialNumber); 
            var stateFile = new FileInfo(stateFilePath); 
            if (!stateFile.Exists) 
            { 
                CopySourceDataFile(newSdFileDescriptor, stateFilePath); 
                return true; 
            } 
            if (_sourceDataFileDescriptor.IsPointToFile(stateFilePath, stateFile.Length)) 
                return oldIsNull; 
            CopySourceDataFile(newSdFileDescriptor, stateFilePath); 
            return true; 
        } 
        private void CopySourceDataFile(SourceDataFileDescriptor sdFileDescriptor, string targetFilePath) 
        { 
            if (string.CompareOrdinal( 
                sdFileDescriptor.ScannerSerialNumner, 
                _syncManager.LocalScannerSerialNumber) == 0) 
            { 
                File.Copy(sdFileDescriptor.FilePath, targetFilePath, true); 
                SystemHelper.SyncFileSystem(); 
                return; 
            } 
            var content = _syncManager.RemoteScanner.GetFileContent(sdFileDescriptor.FilePath); 
            File.WriteAllBytes(targetFilePath, content); 
            SystemHelper.SyncFileSystem(); 
        } 
        private void SubscribeToSourceDataChanges() 
        { 
            foreach (var line in SourceData.Elections.SelectMany(election => election.Protocol.Lines)) 
                line.ValueChangedHandler = ProtocolLineValueChanged; 
            foreach (var candidate in SourceData.Elections.SelectMany(election => election.Candidates)) 
                candidate.DisablingChangedHandler = CandidateDisablingChanged; 
        } 
        private void UnsubscribeFromSourceDataChanges() 
        { 
            foreach (var line in SourceData.Elections.SelectMany(election => election.Protocol.Lines)) 
                line.ValueChangedHandler = null; 
            foreach (var candidate in SourceData.Elections.SelectMany(election => election.Candidates)) 
                candidate.DisablingChangedHandler = CandidateDisablingChanged; 
        } 
        private void ProtocolLineValueChanged(object sender, EventArgs e) 
        { 
            _sourceDataChangesCache.StoreLineValue((Line)sender); 
            RaiseStateChanged(); 
        } 
        private void CandidateDisablingChanged(object sender, EventArgs e) 
        { 
            _sourceDataChangesCache.StoreCandidateDisabling((Candidate)sender); 
        } 
        #endregion 
        #endregion 
        #endregion 
        #region StateSubsystem overrides 
        private const int SOURCE_DATA_FILE_DESCRIPTOR_STATEINDEX = 0; 
        private const int SOURCE_DATA_CHANGES_CACHE_STATEINDEX = 1; 
        private const int IS_SOURCE_DATA_CORRECT_STATEINDEX = 2; 
        private const int CURRENT_VOTING_MODE_STATEINDEX = 3; 
        public override object GetState() 
        { 
            return new object[] 
                       { 
                           _sourceDataFileDescriptor, 
                           _sourceDataChangesCache, 
                           _isSourceDataCorrect, 
                           _currentVotingMode, 
                       }; 
        } 
        public override void RestoreState(object state) 
        { 
            var arr = (object[]) state; 
            SetSourceDataInternal( 
                null, 
                (SourceDataFileDescriptor) arr[SOURCE_DATA_FILE_DESCRIPTOR_STATEINDEX], 
                (SourceDataChangesCache) arr[SOURCE_DATA_CHANGES_CACHE_STATEINDEX], 
                (bool) arr[IS_SOURCE_DATA_CORRECT_STATEINDEX]); 
            _currentVotingMode = (VotingMode) arr[CURRENT_VOTING_MODE_STATEINDEX]; 
        } 
        public override SubsystemStateAcceptanceResult AcceptNewState(object newState) 
        { 
            try 
            { 
                if (newState == null) 
                { 
                    Logger.LogVerbose(Message.Election_NewStateRejectedBecauseIsNull); 
                    return SubsystemStateAcceptanceResult.Rejected; 
                } 
                var arr = (object[]) newState; 
                var newSdFileDescriptor = (SourceDataFileDescriptor) arr[SOURCE_DATA_FILE_DESCRIPTOR_STATEINDEX]; 
                if (newSdFileDescriptor == null && _sourceDataFileDescriptor != null) 
                { 
                    Logger.LogVerbose(Message.Election_NewStateRejectedBecauseNewSourceDataIsNull); 
                    return SubsystemStateAcceptanceResult.Rejected; 
                } 
                var newSdChangesCache = (SourceDataChangesCache)arr[SOURCE_DATA_CHANGES_CACHE_STATEINDEX]; 
                var newIsSdCorrect = _isSourceDataCorrect || (bool) arr[IS_SOURCE_DATA_CORRECT_STATEINDEX]; 
                SetSourceDataInternal(null, newSdFileDescriptor, newSdChangesCache, newIsSdCorrect); 
                Logger.LogVerbose(Message.Election_NewStateAccepted); 
                return SubsystemStateAcceptanceResult.Accepted; 
            } 
            catch (Exception ex) 
            { 
                const string MSG = "Ошибка принятия нового состояния менеджера выборов"; 
                Logger.LogError(Message.Election_NewStateAссeptError, ex); 
                throw new Exception(MSG, ex); 
            } 
        } 
        protected override void ResetStateInternal() 
        { 
            SetSourceDataInternal(null, null, new SourceDataChangesCache(), false); 
            _currentVotingMode = VotingMode.None; 
        } 
        #endregion 
    } 
}

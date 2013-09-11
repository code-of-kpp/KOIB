using System; 
using System.Threading; 
using Croc.Core; 
namespace Croc.Bpc.Voting 
{ 
    public interface IElectionManager : IStateSubsystem, IQuietMode 
    { 
        #region Общие св-ва 
        DateTime LocalTimeNow { get; } 
        VotingMode CurrentVotingMode { get; set; } 
        event EventHandler<VotingModeChangedEventArgs> VotingModeChanged; 
        bool NeedExecuteCheckExpressions { get; } 
        bool СanRestoreCandidateCanseledInSd { get; } 
        #endregion 
        #region Получение информации по исходным данным 
        SourceData SourceData { get; } 
        bool IsSourceDataCorrect { get; set; } 
        bool HasSourceDataChanged { get; } 
        ElectionDayСomming IsElectionDay(); 
        ElectionDayСomming IsElectionDay(SourceData sourceData); 
        #endregion 
        #region Загрузка исходных данных 
        bool HasSourceData(); 
        string[] GetSourceDataSearchPaths(); 
        bool FindSourceDataFile(WaitHandle stopSearchingEvent, out SourceDataFileDescriptor sdFileDescriptor); 
        bool LoadSourceDataFromFile(SourceDataFileDescriptor sdFileDescriptor, bool needVerify, out SourceData sd); 
        bool SetSourceData(SourceData sourceData, SourceDataFileDescriptor sourceDataFileDescriptor); 
        #endregion 
        string FindDirPathToSaveVotingResultProtocol(bool needSourceDataForSaveResults); 
    } 
}

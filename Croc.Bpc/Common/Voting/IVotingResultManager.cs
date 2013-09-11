using System; 
using Croc.Core; 
namespace Croc.Bpc.Voting 
{ 
    public interface IVotingResultManager : IStateSubsystem 
    { 
        bool PackResultsEnabled { get; } 
        bool AddBadBlankToCounterValue { get; } 
        #region Результаты голосования 
        VotingResults VotingResults { get; } 
        VotingResult LastVotingResult { get; } 
        void ResetLastVotingResult(); 
        void SetLastVotingResult(VotingResult votingResult); 
        void AddVotingResult( 
            VotingResult votingResult, 
            VotingMode votingMode, 
            int scannerSerialNumber); 
        #endregion 
        #region Сохранение результатов голосования 
        void GeneratePreliminaryVotingResultProtocol(); 
        void GenerateVotingResultProtocol(Election election); 
        bool FindFilePathToSaveVotingResultProtocol(); 
        bool SaveVotingResultProtocol(); 
        #endregion 
    } 
}

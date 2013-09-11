using System; 
using Croc.Core; 
namespace Croc.Bpc.Workflow 
{ 
    public interface IWorkflowManager : IStateSubsystem, IQuietMode 
    { 
        #region Запуск потока работ 
        event EventHandler WorkflowTerminated; 
        void StartWorkflow(bool quietStart); 
        #endregion 
        #region Управление выполнением 
        bool GoToStateActivity(); 
        void GoToActivity(string activityName); 
        void SyncState(); 
        #endregion 
        #region Счетчики ошибок 
        int IncreaseErrorCounter(string errorId); 
        void ResetErrorCounters(); 
        void ResetErrorCounter(string errorId); 
        #endregion 
    } 
}

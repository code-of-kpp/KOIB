using System; 
using Croc.Bpc.Scanner; 
using Croc.Core; 
namespace Croc.Bpc.Synchronization 
{ 
    public interface ISynchronizationManager :  
        ISubsystem,  
        IScannerInteractionChannel, 
        IScannersInfo 
    { 
        #region Устанавка соединения с удаленным сканером 
        void OpenIncomingInteractionChannel(string localSerialNumber, string localIpAddress); 
        event EventHandler RemoteScannerConnected; 
        event EventHandler RemoteScannerDisconnected; 
        #endregion 
        #region Работа с удаленным сканером 
        IScannerInteractionChannel RemoteScanner { get; } 
        bool IsRemoteScannerConnected { get; } 
        #endregion 
        #region Обмен данными 
        object GetDataTransmittedFromRemoteScanner(string name, IWaitController waitCtrl); 
        event EventHandler RemoteScannerWaitForInitialization; 
        event EventHandler RemoteScannerExitFromMenu; 
        #endregion 
        #region Роль сканера 
        new ScannerRole ScannerRole { get; set; } 
        #endregion 
        #region Работа с состоянием 
        bool LoadState(); 
        bool SynchronizationEnabled 
        { 
            get; 
            set; 
        } 
        void StartStateSynchronization(bool enableSync); 
        bool WaitForSynchronizationFinished(IWaitController waitCtrl); 


        #endregion 
        #region Сброс ПО 
        void ResetUik(ResetSoftReason reason); 
        #endregion 
    } 
}

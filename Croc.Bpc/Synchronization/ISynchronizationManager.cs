using System; 

using Croc.Core; 

using Croc.Bpc.Common.Interfaces; 

 

 

namespace Croc.Bpc.Synchronization 

{ 

    /// <summary> 

    /// Интерфейс подсистемы синхронизации 

    /// </summary> 

    public interface ISynchronizationManager :  

        ISubsystem,  

        IScannerInteractionChannel, 

        IScannersInfo 

    { 

        #region Устанавка соединения с удаленным сканером 

 

 

        /// <summary> 

        /// Открыть канал для подключения удаленных сканеров 

        /// </summary> 

        /// <param name="localSerialNumber">Серийный номер локального сканера</param> 

        /// <param name="localIPAddress">IP-адрес локального сканера</param> 

        void OpenIncomingInteractionChannel(string localSerialNumber, string localIPAddress); 

        /// <summary> 

        /// Событие "Соединение с удаленным сканером установлено" 

        /// </summary> 

        event EventHandler RemoteScannerConnected; 

        /// <summary> 

        /// Событие "Связь с удаленным сканером потеряна" 

        /// </summary> 

        event EventHandler RemoteScannerDisconnected; 

 

 

        #endregion 

 

 

        #region Работа с удаленным сканером 

 

 

        /// <summary> 

        /// Интерфейс доступа к удаленному сканеру 

        /// </summary> 

        IScannerInteractionChannel RemoteScanner { get; } 

 

 

        /// <summary> 

        /// Удаленный сканер подключен? 

        /// </summary> 

        bool IsRemoteScannerConnected { get; } 


 
 

        #endregion 

 

 

        #region Обмен данными 

 

 

        /// <summary> 

        /// Получить данные, которые были переданы с удаленного сканера. 

        /// Если данных нет, то ожидается их поступление 

        /// </summary> 

        /// <param name="name">имя данных</param> 

        /// <returns>данные</returns> 

        object GetDataTransmittedFromRemoteScanner(string name, IWaitController waitCtrl); 

 

 

        #endregion 

 

 

        #region Роль сканера 

 

 

        /// <summary> 

        /// Роль данного сканера 

        /// </summary> 

        ScannerRole ScannerRole { get; set; } 

 

 

        #endregion 

 

 

        #region Работа с состоянием 

 

 

        /// <summary> 

        /// Загрузить состояние с диска из файла состояния 

        /// </summary> 

        void LoadState(); 

 

 

        /// <summary> 

        /// Включение/выключение синхронизации состояния с удаленным сканером 

        /// </summary> 

        bool SynchronizationEnabled 

        { 

            get; 

            set; 

        } 

 


 
        /// <summary> 

        /// Начать синхронизацию состояния с удаленным сканером 

        /// </summary> 

        /// <param name="enableSync">нужно ли включить синхронизацию, если она выключена. 

        /// Если значение равно 

        /// true - синхронизация будет включена и выполнена 

        /// false - если будет выполнена, только если она уже включена</param> 

        void StartStateSynchronization(bool enableSync); 

 

 

        /// <summary> 

        /// Ожидает, когда завершится очередная синхронизация с удаленным сканером 

        /// </summary> 

        /// <returns></returns> 

        bool WaitForSynchronizationFinished(IWaitController waitCtrl); 

 

 

        #endregion 

    } 

}



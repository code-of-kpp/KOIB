using System; 

using System.Collections.Generic; 

using System.Collections.Specialized; 

using Croc.Bpc.Printing.Reports; 

using Croc.Core.Extensions; 

 

 

namespace Croc.Bpc.Synchronization 

{ 

    /// <summary> 

    /// Объект, который используется для обращения к нему по ремоутингу с удаленного сканера 

    /// </summary> 

    public class RemoteScannerConnector : MarshalByRefObject, IScannerInteractionChannel 

    { 

        /// <summary> 

        /// Канал для взаимодействия с локальным сканером 

        /// </summary> 

        private IScannerInteractionChannel _localScannerChannel; 

 

 

        public delegate IScannerInteractionChannel GetChannelToLocalScannerDelegate(); 

        /// <summary> 

        /// Событие "Получить канал для взаимодействия с локальным сканером" 

        /// </summary> 

        public static event GetChannelToLocalScannerDelegate GetChannelToLocalScannerEvent; 

 

 

        public delegate bool IsRemoteConnectionAllowDelegate(string serialNumber, string ipAddress); 

        /// <summary> 

        /// Событие "Разрешено ли удаленное подключение?" 

        /// </summary> 

        public static event IsRemoteConnectionAllowDelegate IsRemoteConnectionAllowEvent; 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <remarks>вызывается инфраструкторой .net remoting при обращении удаленного сканера</remarks> 

        public RemoteScannerConnector() 

        { 

            _localScannerChannel = GetChannelToLocalScanner(); 

            if (_localScannerChannel == null) 

                throw new Exception("Ошибка получения канала доступа к локальному объекту"); 

        } 

 

 

        /// <summary> 

        /// Получить канал доступа к локальному сканеру 

        /// </summary> 

        /// <returns></returns> 


        private IScannerInteractionChannel GetChannelToLocalScanner() 

        { 

            var handler = GetChannelToLocalScannerEvent; 

            if (handler != null) 

                return handler(); 

 

 

            return null; 

        } 

 

 

        /// <summary> 

        /// Разрешено ли удаленное подключение? 

        /// </summary> 

        /// <param name="serialNumber"></param> 

        /// <param name="ipAddress"></param> 

        /// <returns></returns> 

        public bool IsRemoteConnectionAllow(string serialNumber, string ipAddress) 

        { 

            var handler = IsRemoteConnectionAllowEvent; 

            if (handler != null) 

                return handler(serialNumber, ipAddress); 

 

 

            return false; 

        } 

 

 

        #region IScannerInteractionChannel Members 

 

 

        #region Система 

 

 

        public Version ApplicationVersion 

        { 

            get 

            { 

                return _localScannerChannel.ApplicationVersion; 

            } 

        } 

 

 

        public void Ping() 

        { 

            _localScannerChannel.Ping(); 

        } 

 

 

        public void SetSystemTime(DateTime utcDateTime) 


        { 

            _localScannerChannel.SetSystemTime(utcDateTime); 

        } 

 

 

        #endregion 

 

 

        #region Исходные данные 

 

 

        public bool IsElectionDayNow 

        { 

            get 

            { 

                return _localScannerChannel.IsElectionDayNow; 

            } 

        } 

 

 

        public Guid SourceDataId 

        { 

            get 

            { 

                return _localScannerChannel.SourceDataId; 

            } 

        } 

 

 

        #endregion 

 

 

        #region Роль сканера 

 

 

        public event EventHandler ScannerRoleChanged; 

 

 

        /// <summary> 

        /// Возбудить событие "Роль сканера изменилась" 

        /// </summary> 

        public void RaiseRemoteScannerRoleChanged() 

        { 

            _localScannerChannel.RaiseRemoteScannerRoleChanged(); 

        } 

 

 

        /// <summary> 

        /// Роль данного сканера 

        /// </summary> 


        public ScannerRole ScannerRole 

        { 

            get 

            { 

                return _localScannerChannel.ScannerRole; 

            } 

        } 

 

 

        /// <summary> 

        /// Ожидает, когда роль сканера будет определена 

        /// </summary> 

        /// <returns>роль, которую принял сканер</returns> 

        public ScannerRole WaitForScannerRoleDefined() 

        { 

            return _localScannerChannel.WaitForScannerRoleDefined(); 

        } 

 

 

        #endregion 

 

 

        #region Передача данных 

 

 

        public void PutData(string name, object data) 

        { 

            _localScannerChannel.PutData(name, data); 

        } 

 

 

        #endregion 

 

 

        #region Состояние 

 

 

        public bool IsStateInitial 

        { 

            get 

            { 

                return _localScannerChannel.IsStateInitial; 

            } 

        } 

 

 

        public void ResetState() 

        { 

            _localScannerChannel.ResetState(); 

        } 


 
 

        public void NeedSynchronizeState(List<StateItem> newStateItems) 

        { 

            _localScannerChannel.NeedSynchronizeState(newStateItems); 

        } 

 

 

        public void StateSynchronizationFinished(SynchronizationResult syncResult) 

        { 

            _localScannerChannel.StateSynchronizationFinished(syncResult); 

        } 

 

 

        #endregion 

 

 

		#region Печать 

 

 

		/// <summary> 

		/// Подключен ли принтер к удаленному сканеру сканеру 

		/// </summary> 

		/// <returns>true - подключен/false - нет</returns> 

		public bool FindRemotePrinter() 

		{ 

			return _localScannerChannel.FindRemotePrinter(); 

		} 

 

 

		/// <summary> 

		/// Распечатать отчет на удаленном принтере 

		/// </summary> 

		/// <returns>успех печати</returns> 

		public bool RemotePrintReport(ReportType reportType, ListDictionary reportParameters) 

		{ 

			return _localScannerChannel.RemotePrintReport(reportType, reportParameters); 

		} 

 

 

		#endregion 

 

 

		#region Сброс ПО 

 

 

		/// <summary> 

		/// Сброс По 

		/// </summary> 

		public void ResetSoft() 


		{ 

			_localScannerChannel.ResetSoft(); 

		} 

 

 

		#endregion 

 

 

        #endregion 

    } 

}



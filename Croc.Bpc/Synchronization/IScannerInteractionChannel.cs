using System; 

using System.Collections.Generic; 

using System.Collections.Specialized; 

using System.Linq; 

using System.Text; 

using Croc.Bpc.Printing.Reports; 

using Croc.Core; 

 

 

namespace Croc.Bpc.Synchronization 

{ 

    /// <summary> 

    /// Интерфейс канала для взаимодействия со сканером 

    /// </summary> 

    public interface IScannerInteractionChannel 

    { 

        #region Система 

 

 

        /// <summary> 

        /// Версия приложения 

        /// </summary> 

        Version ApplicationVersion { get; } 

 

 

        /// <summary> 

        /// Проверка связи 

        /// </summary> 

        void Ping(); 

 

 

        /// <summary> 

        /// Устанавливает текущую дату и время на сканере 

        /// </summary> 

        /// <param name="utcDateTime"></param> 

        void SetSystemTime(DateTime utcDateTime); 

 

 

        #endregion 

 

 

        #region Исходные данные 

 

 

        /// <summary> 

        /// Сейчас день выборов? 

        /// </summary> 

        bool IsElectionDayNow { get; } 

 

 


        /// <summary> 

        /// Идентификатор исходных данных 

        /// </summary> 

        Guid SourceDataId { get; } 

 

 

        #endregion 

 

 

        #region Роль сканера 

 

 

        /// <summary> 

        /// Роль сканера изменилась 

        /// </summary> 

        /// <remarks>имеется ввиду роль данного сканера</remarks> 

        event EventHandler ScannerRoleChanged; 

 

 

        /// <summary> 

        /// Возбудить событие "Роль сканера изменилась" на удаленном сканере 

        /// </summary> 

        void RaiseRemoteScannerRoleChanged(); 

 

 

        /// <summary> 

        /// Роль данного сканера 

        /// </summary> 

        ScannerRole ScannerRole { get; } 

 

 

        /// <summary> 

        /// Ожидает, когда роль сканера будет определена 

        /// </summary> 

        /// <returns>роль, которую принял сканер</returns> 

        ScannerRole WaitForScannerRoleDefined(); 

 

 

        #endregion 

 

 

        #region Передача данных 

 

 

        /// <summary> 

        /// Положить данные в таблицу данных, которые передал удаленный сканер 

        /// </summary> 

        /// <param name="name">имя данных</param> 

        /// <param name="data">данные</param> 

        void PutData(string name, object data); 


 
 

        #endregion 

 

 

        #region Состояние 

 

 

        /// <summary> 

        /// Текущее состояние - начальное? 

        /// </summary> 

        bool IsStateInitial { get; } 

 

 

        /// <summary> 

        /// Архивирует текущее состояние и сбрасываеи его в начальное 

        /// </summary> 

        void ResetState(); 

 

 

        /// <summary> 

        /// Нужно синхронизировать состояния 

        /// </summary> 

        /// <param name="newStateItems">элементы состояния, которые были изменены и  

        /// по которым требуется синхронизация</param> 

        void NeedSynchronizeState(List<StateItem> newStateItems); 

 

 

        /// <summary> 

        /// Синхронизация состояния завершена 

        /// </summary> 

        /// <param name="syncResult">результат синхронизации</param> 

        /// <remarks>этот метод вызывает удаленный сканер с целью сообщить, 

        /// что синхронизация состояния завершена</remarks> 

        void StateSynchronizationFinished(SynchronizationResult syncResult); 

 

 

        #endregion 

 

 

		#region Печать 

 

 

		/// <summary> 

		/// Подключен ли принтер к удаленному сканеру сканеру 

		/// </summary> 

		/// <returns>true - подключен/false - нет</returns> 

		bool FindRemotePrinter(); 

 

 


		/// <summary> 

		/// Распечатать отчет на удаленном принтере 

		/// </summary> 

		/// <returns></returns> 

		bool RemotePrintReport(ReportType reportType, ListDictionary reportParameters); 

 

 

		#endregion 

 

 

		#region Сброс ПО 

 

 

		/// <summary> 

		/// Сброс По 

		/// </summary> 

		void ResetSoft(); 

 

 

		#endregion 

	} 

}



using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Core.Diagnostics; 

using Croc.Core; 

 

 

namespace Croc.Bpc.Scanner 

{ 

	/// <summary> 

	/// Фильтр призванный добавлять в лог ИД бюллетеня 

	/// </summary> 

	public class BulletinIDLogFilter : IEventFilter 

	{ 

		/// <summary> 

		/// Менеджер сканера 

		/// </summary> 

		private IScannerManager _scannerManager; 

 

 

		private static object s_scannerManagerSync = new object(); 

 

 

		public const string BULLETIN_ID_PROPERTY = "BulletinID"; 

 

 

		/// <summary> 

		/// Возвращает ссылку на Менеджер сканера 

		/// </summary> 

		private IScannerManager GetScannerManager() 

		{ 

			if (_scannerManager == null) 

				lock (s_scannerManagerSync) 

				{ 

					if (_scannerManager == null) 

						_scannerManager = CoreApplication.Instance.GetSubsystem<IScannerManager>(); 

				} 

 

 

			return _scannerManager; 

		} 

 

 

		#region IEventWriterFilter Members 

 

 

		/// <summary> 

		/// Добавляет в событие ИД бюллетеня 

		/// </summary> 


		/// <param name="loggerEvent">Событие логера</param> 

		/// <returns>всегда true</returns> 

		public bool Accepted(LoggerEvent logEvent) 

		{ 

			// получим менеджера сканера 

			var scannerManager = GetScannerManager(); 

 

 

			// присвоим событию ИД сессии обработки бюллетеня 

			if(scannerManager != null && scannerManager.SheetProcessingSession != null 

				&& !scannerManager.SheetProcessingSession.Closed) 

				logEvent.Properties[BULLETIN_ID_PROPERTY] = scannerManager.SheetProcessingSession.Id; 

 

 

			return true; 

		} 

 

 

		#endregion 

 

 

		#region IInitializedType Members 

 

 

		/// <summary> 

		/// Инициализация фильтра 

		/// </summary> 

		/// <param name="props"></param> 

		public void Init(System.Configuration.NameValueConfigurationCollection props) 

		{ 

			// ничего не делаем 

		} 

 

 

		#endregion 

 

 

	} 

}



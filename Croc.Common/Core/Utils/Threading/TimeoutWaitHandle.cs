using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Threading; 

 

 

namespace Croc.Core.Utils.Threading 

{ 

	public class TimeoutWaitHandle : EventWaitHandleEx 

	{ 

		/// <summary> 

		/// Время ожидания до вызова события 

		/// </summary> 

		private int _timeout; 

 

 

		/// <summary> 

		/// Тред в котором запускаем ожидание события 

		/// </summary> 

		private Thread _eventThread = null; 

 

 

		/// <summary> 

		/// Признак, что объект освобожден 

		/// </summary> 

		private bool _disposed = false; 

 

 

		/// <summary> 

		/// Конструктор 

		/// </summary> 

		/// <param name="timeout">Время, через которое запускать событие</param> 

		public TimeoutWaitHandle(int timeout) 

			: base(false, false) 

		{ 

			_timeout = timeout; 

		} 

 

 

		/// <summary> 

		/// Запуск события 

		/// </summary> 

		new public void Reset() 

		{ 

			// если тред уже есть 

			if (_eventThread != null) 

				_eventThread.Abort(); 

 

 


			// запустим поток ожидания таймаута 

			_eventThread = new Thread(WaitingForTimeout); 

			_eventThread.IsBackground = true; 

			_eventThread.Start(); 

		} 

 

 

		/// <summary> 

		/// Ожидание таймаута и выставление состояния дескриптора ожидания события в "Включен" 

		/// </summary> 

		private void WaitingForTimeout() 

		{ 

			// ждем сколько указано в таймауте 

			Thread.Sleep(_timeout); 

 

 

			// вызовем Set у базового класса 

			if(!_disposed) 

				Set(); 

		} 

 

 

		/// <summary> 

		/// Освобождение ресурсов 

		/// </summary> 

		/// <param name="explicitDisposing"></param> 

		protected override void Dispose(bool explicitDisposing) 

		{ 

			// пометим объект как dispose 

			_disposed = true; 

 

 

			base.Dispose(explicitDisposing); 

		} 

	} 

}



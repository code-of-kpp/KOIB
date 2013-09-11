using System; 

using System.Diagnostics; 

using Croc.Core.Configuration; 

using Croc.Core.Diagnostics; 

using Croc.Core.Utils; 

using System.Threading; 

using Croc.Core.Extensions; 

 

 

namespace Croc.Core 

{ 

    /// <summary> 

    /// Базовый класс для подсистем 

    /// </summary> 

    public abstract class Subsystem : ISubsystem 

    { 

        /// <summary> 

        /// Имя подсистемы 

        /// </summary> 

        public string Name 

        { 

            get; 

            set; 

        } 

        /// <summary> 

        /// Приложение, в которое входит подсистема 

        /// </summary> 

        public ICoreApplication Application 

        { 

            get; 

            set; 

        } 

 

 

        /// <summary> 

        /// Инициализация подсистемы 

        /// </summary> 

        /// <param name="config">конфиг-элемент с настройками подсистемы</param> 

        public virtual void Init(SubsystemConfig config) 

        { 

        } 

 

 

        /// <summary> 

        /// Применить новую конфигурацию подсистемы 

        /// </summary> 

        /// <param name="newConfig">новый конфиг-элемент с настройками подсистемы</param> 

        public virtual void ApplyNewConfig(SubsystemConfig newConfig) 

        { 

        } 


 
 

		/// <summary> 

		/// Событие изменения конфигурации подсистемы 

		/// </summary> 

		public event EventHandler<ConfigUpdatedEventArgs> ConfigUpdated; 

 

 

		/// <summary> 

		/// Вызвать событие конфиг изменился 

		/// </summary> 

		/// <param name="e">параметры события</param> 

		protected void RaiseConfigUpdatedEvent(ConfigUpdatedEventArgs e) 

		{ 

			ConfigUpdated.RaiseEvent(this, e); 

		} 

 

 

        #region Логирование 

 

 

        private static readonly object s_loggerLock = new object(); 

        private ILogger _logger; 

 

 

        /// <summary> 

        /// Признак необходимости писать логи в отдельный файл 

        /// </summary> 

        public bool SeparateLog 

        { 

            get; 

            set; 

        } 

 

 

        /// <summary> 

        /// Уровень трассировки 

        /// </summary> 

        public TraceLevel TraceLevel 

        { 

            get; 

            internal set; 

        } 

 

 

        /// <summary> 

        /// Папка, в которой будут создаваться лог-файлы 

        /// </summary> 

        public string LogFileFolder 

        { 


            get; 

            internal set; 

        } 

 

 

        /// <summary> 

        /// Логгер подсистемы 

        /// </summary> 

        public ILogger Logger 

        { 

            get 

            { 

                if (_logger == null) 

                    lock (s_loggerLock) 

                        if (_logger == null) 

                            _logger = Application.CreateLogger(Name, TraceLevel); 

 

 

                return _logger; 

            } 

        } 

 

 

        /// <summary> 

        /// Освобождает логгер подсистемы 

        /// </summary> 

        public void DisposeLogger() 

        { 

            if (_logger != null) 

                lock (s_loggerLock) 

                    if (_logger != null) 

                    { 

                        Disposer.DisposeObject(_logger); 

                        _logger = null; 

                    } 

        } 

 

 

        #endregion 

 

 

        #region Ожидание событий 

        // данные методы ожидания следует использовать в случаях,  

        // когда важно отследить вызов деструктора подсистемы 

 

 

        /// <summary> 

        /// Заснуть на заданное время 

        /// </summary> 

        /// <returns>true - проснулись, false - был вызван деструктор</returns> 


        protected bool Sleep(TimeSpan timeout, IWaitController waitCtrl) 

        { 

            return !(waitCtrl == null ? _disposeEvent.WaitOne(timeout) : waitCtrl.WaitOne(_disposeEvent, timeout)); 

        } 

 

 

        /// <summary> 

        /// Ожидает одно заданное событие 

        /// </summary> 

        /// <param name="ev">событие</param> 

        /// <returns>true - событие наступило, false - вызов деструктора, ожидание прервано</returns> 

        protected bool WaitOne(WaitHandle ev, IWaitController waitCtrl) 

        { 

            int index; 

            return WaitAny(new[] { ev }, out index, waitCtrl); 

        } 

 

 

        /// <summary> 

        /// Ожидает любое из заданных событий 

        /// </summary> 

        /// <param name="events">события</param> 

        /// <param name="occurredEventIndex">индекс наступившего события</param> 

        /// <returns>true - событие наступило, false - вызов деструктора, ожидание прервано</returns> 

        protected bool WaitAny(WaitHandle[] events, out int occurredEventIndex, IWaitController waitCtrl) 

        { 

            var eventsEx = new WaitHandle[events.Length + 1]; 

            var disposeEventIndex = events.Length; 

 

 

            events.CopyTo(eventsEx, 0); 

            eventsEx[disposeEventIndex] = _disposeEvent; 

 

 

            occurredEventIndex = (waitCtrl == null ? WaitHandle.WaitAny(eventsEx) : waitCtrl.WaitAny(eventsEx)); 

 

 

            return occurredEventIndex != disposeEventIndex; 

        } 

 

 

        #endregion 

 

 

        #region Освобождение ресурсов 

 

 

        /// <summary> 

        /// Событие "Вызван деструктор" 

        /// </summary> 


        protected ManualResetEvent _disposeEvent = new ManualResetEvent(false); 

        /// <summary> 

        /// Признак того, что для подсистемы был вызван деструктор 

        /// </summary> 

        protected bool _disposed = false; 

 

 

        /// <summary> 

        /// Выставляет признак начала освобождения ресурсов (_disposeEvent.Set()) 

        /// </summary> 

        public virtual void Dispose() 

        { 

            _disposeEvent.Set(); 

            _disposed = true; 

        } 

 

 

        #endregion 

    } 

}



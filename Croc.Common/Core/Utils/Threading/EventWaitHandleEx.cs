using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Threading; 

 

 

namespace Croc.Core.Utils.Threading 

{ 

    /// <summary> 

    /// Дескриптор ожидания события с расширенной функциональностью 

    /// </summary> 

    public class EventWaitHandleEx : WaitHandle, IDisposable 

    { 

        /// <summary> 

        /// Внутренний дескриптор ожидания 

        /// </summary> 

        private EventWaitHandle _innerEvent; 

        /// <summary> 

        /// Объект безопасности 

        /// </summary> 

        private object _securityObject; 

        /// <summary> 

        /// Признак наличия доступа для выполнения операций 

        /// </summary> 

        private bool _accessGranted = false; 

 

 

        /// <summary> 

        /// Конструктор, который создает Дескриптор ожидания с соотв. типом сброса 

        /// без поддержки проверки уровня доступа (т.е. доступ всегда есть) 

        /// </summary> 

        /// <param name="initialState"></param> 

        /// <param name="manualReset"></param> 

        public EventWaitHandleEx(bool initialState, bool manualReset) 

            : this(initialState, manualReset, null) 

        { 

        } 

 

 

        /// <summary> 

        /// Конструктор, который создает Дескриптор ожидания с соотв. типом сброса и  

        /// с поддержкой проверки уровня доступа 

        /// </summary> 

        /// <param name="initialState"></param> 

        /// <param name="manualReset"></param> 

        /// <param name="securityObject"></param> 

        public EventWaitHandleEx(bool initialState, bool manualReset, object securityObject) 

        { 

            _innerEvent = manualReset  


                ? (EventWaitHandle)new ManualResetEvent(initialState)  

                : (EventWaitHandle)new AutoResetEvent(initialState); 

 

 

            base.SafeWaitHandle = _innerEvent.SafeWaitHandle; 

 

 

            _securityObject = securityObject; 

        } 

 

 

        #region Получение доступа 

 

 

        /// <summary> 

        /// Получить доступ 

        /// </summary> 

        /// <param name="securityObject"></param> 

        /// <returns></returns> 

        public bool GetAccess(object securityObject) 

        { 

            // доступ разрешен, если объект безопасности не задан или задан и совпадает с переданным в параметре 

            _accessGranted = (_securityObject == null || _securityObject.Equals(securityObject)); 

            return _accessGranted; 

        } 

 

 

        /// <summary> 

        /// Проверяет наличие доступа 

        /// </summary> 

        /// <returns></returns> 

        private bool CheckAccess() 

        { 

            // если задан объект безопасности и доступа нет 

            if (_securityObject != null && !_accessGranted) 

                return false; 

 

 

            // сбросим признак наличия доступа, т.к. он одноразовый 

            _accessGranted = false; 

            return true; 

        } 

 

 

        #endregion 

 

 

        #region Вкл/Выкл 

 

 


        /// <summary> 

        /// Сбросить состояние дескриптора ожидания события в "Выключен" 

        /// </summary> 

        /// <returns></returns> 

        public bool Reset() 

        { 

            // если доступа нет 

            if (!CheckAccess()) 

                return false; 

 

 

            return _innerEvent.Reset(); 

        } 

 

 

        /// <summary> 

        /// Выставить состояние дескриптора ожидания события в "Включен" 

        /// </summary> 

        /// <returns></returns> 

        public bool Set() 

        { 

            // если доступа нет 

            if (!CheckAccess()) 

                return false; 

 

 

            // FIX: тут может происходить ошибка из-за того, что _innerEvent.Close() уже был ранее вызван. 

            // это иногда происходит при выключении приложения 

            try 

            { 

                return _innerEvent.Set(); 

            } 

            catch 

            { 

                return false; 

            } 

        } 

 

 

        #endregion 

 

 

        #region Ожидание 

 

 

        public override bool WaitOne() 

        { 

            return _innerEvent.WaitOne(); 

        } 

 


 
        public override bool WaitOne(TimeSpan timeout) 

        { 

            return _innerEvent.WaitOne(timeout, false); 

        } 

 

 

        public override bool WaitOne(TimeSpan timeout, bool exitContext) 

        { 

            return _innerEvent.WaitOne(timeout, exitContext); 

        } 

 

 

        public override bool WaitOne(int millisecondsTimeout) 

        { 

            return _innerEvent.WaitOne(millisecondsTimeout); 

        } 

 

 

        public override bool WaitOne(int millisecondsTimeout, bool exitContext) 

        { 

            return _innerEvent.WaitOne(millisecondsTimeout, exitContext); 

        } 

 

 

        #endregion 

 

 

        #region Освобождение ресурсов 

 

 

        public override void Close() 

        { 

            Dispose(); 

        } 

 

 

        ~EventWaitHandleEx() 

        { 

            Dispose(false); 

        }         

 

 

        public void Dispose() 

        { 

            Dispose(true); 

            GC.SuppressFinalize(this); 

        } 

 

 


        protected override void Dispose(bool explicitDisposing) 

        { 

            _innerEvent.Close(); 

        } 

 

 

        #endregion 

    } 

}



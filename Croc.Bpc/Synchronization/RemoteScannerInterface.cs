using System; 

using System.Collections.Generic; 

using System.Collections.Specialized; 

using System.Diagnostics; 

using System.Net.Sockets; 

using System.Runtime.Remoting; 

using System.Threading; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Bpc.Printing.Reports; 

using Croc.Core.Diagnostics; 

using Croc.Core.Extensions; 

using Croc.Bpc.Synchronization.Config; 

 

 

namespace Croc.Bpc.Synchronization 

{ 

    /// <summary> 

    /// Обертка для безопасного обращения к удаленному сканеру 

    /// </summary> 

    internal class RemoteScannerInterface : IScannerInteractionChannel 

    { 

        /// <summary> 

        /// Логгер 

        /// </summary> 

        private ILogger _logger; 

        /// <summary> 

        /// Канал для доступа к удаленному сканеру 

        /// </summary> 

        private volatile IScannerInteractionChannel _interactionChannel; 

        /// <summary> 

        /// Признак того, что интерфейс работает, т.е. соединение в порядке 

        /// </summary> 

        private volatile bool _alive = false; 

        /// <summary> 

        /// Признак того, что интерфейс работает, т.е. соединение в порядке 

        /// </summary> 

        public bool Alive 

        { 

            get 

            { 

                return _alive; 

            } 

            private set 

            { 

                _alive = value; 

 

 

                // если интерфейс умер 

                if (!value) 

                { 


                    // то почистим подписки на события, которые выставляет интерфейс 

                    ScannerRoleChanged.ClearInvocationList(); 

                } 

            } 

        } 

        /// <summary> 

        /// Параметры вызова метода по умолчанию 

        /// </summary> 

        private CallPropertiesConfig _commonCallProperties; 

        /// <summary> 

        /// Параметры вызова метода NeedSynchronizeState 

        /// </summary> 

        private CallPropertiesConfig _synchronizationCallProperties; 

        /// <summary> 

        /// Параметры вызова метода c бесконечным таймаутом (без таймаута) 

        /// </summary> 

        private CallPropertiesConfig _infiniteTimeoutCallProperties; 

        /// <summary> 

        /// Параметры вызова метода RemotePrintReport 

        /// </summary> 

        private CallPropertiesConfig _printingCallProperties; 

 

 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

        /// <param name="logger"></param> 

        public RemoteScannerInterface( 

            ILogger logger,  

            CallPropertiesConfig commonCallProperties, 

            CallPropertiesConfig synchronizationCallProperties, 

            CallPropertiesConfig printingCallProperties) 

        { 

            CodeContract.Requires(logger != null); 

            CodeContract.Requires(commonCallProperties != null); 

            CodeContract.Requires(synchronizationCallProperties != null); 

            CodeContract.Requires(printingCallProperties != null); 

 

 

            _logger = logger; 

 

 

            _commonCallProperties = commonCallProperties; 

            _synchronizationCallProperties = synchronizationCallProperties; 

            _printingCallProperties = printingCallProperties; 

            _infiniteTimeoutCallProperties = new CallPropertiesConfig() 

            { 

                Timeout = Timeout.Infinite, 

                MaxTryCount = commonCallProperties.MaxTryCount, 

                RetryDelay = commonCallProperties.RetryDelay 


            }; 

        } 

 

 

        /// <summary> 

        /// Установить канал для обращения к удаленному сканеру 

        /// </summary> 

        /// <param name="interactionChannel"></param> 

        public void SetInteractionChannel(IScannerInteractionChannel interactionChannel) 

        { 

            CodeContract.Requires(interactionChannel != null); 

 

 

            _interactionChannel = interactionChannel; 

            Alive = true; 

        } 

 

 

        #region IScannerInteractionChannel Members 

 

 

        #region Система 

 

 

        /// <summary> 

        /// Неопределенная версия приложения 

        /// </summary> 

        private static Version UNDEFINED_APPLICATION_VERSION = new Version(0, 0, 0, 0); 

 

 

        /// <summary> 

        /// Версия приложения 

        /// </summary> 

        public Version ApplicationVersion 

        { 

            get 

            { 

                return SafeCall<Version>( 

                    () => { return _interactionChannel.ApplicationVersion; }, 

                    _commonCallProperties, 

                    UNDEFINED_APPLICATION_VERSION); 

            } 

        } 

 

 

        /// <summary> 

        /// Проверка связи 

        /// </summary> 

        public void Ping() 

        { 


            SafeCall<object>( 

                () => { _interactionChannel.Ping(); return null; }, 

                _commonCallProperties, 

                null); 

        } 

 

 

        /// <summary> 

        /// Устанавливает текущую дату и время на сканере 

        /// </summary> 

        /// <param name="utcDateTime"></param> 

        public void SetSystemTime(DateTime utcDateTime) 

        { 

            SafeCall<object>( 

                () => { _interactionChannel.SetSystemTime(utcDateTime); return null; }, 

                _commonCallProperties, 

                null); 

        } 

 

 

        #endregion 

 

 

        #region Исходные данные 

 

 

        /// <summary> 

        /// Сейчас день выборов? 

        /// </summary> 

        public bool IsElectionDayNow 

        { 

            get 

            { 

                return SafeCall<bool>( 

                    () => { return _interactionChannel.IsElectionDayNow; }, 

                    _commonCallProperties, 

                    true); 

            } 

        } 

 

 

        /// <summary> 

        /// Идентификатор исходных данных 

        /// </summary> 

        public Guid SourceDataId 

        { 

            get 

            { 

                return SafeCall<Guid>( 

                    () => { return _interactionChannel.SourceDataId; }, 


                    _commonCallProperties, 

                    Guid.Empty); 

            } 

        } 

 

 

        #endregion 

 

 

        #region Роль сканера 

 

 

        /// <summary> 

        /// Роль сканера изменилась 

        /// </summary> 

        public event EventHandler ScannerRoleChanged; 

 

 

        /// <summary> 

        /// Возбудить событие "Роль сканера изменилась" 

        /// </summary> 

        internal void RaiseScannerRoleChanged() 

        { 

            ScannerRoleChanged.RaiseEvent(this); 

        } 

 

 

        /// <summary> 

        /// Возбудить событие "Роль сканера изменилась" на удаленном сканере 

        /// </summary> 

        public void RaiseRemoteScannerRoleChanged() 

        { 

            SafeCall<object>( 

                () => { _interactionChannel.RaiseRemoteScannerRoleChanged(); return null; }, 

                _commonCallProperties, 

                null); 

        } 

 

 

        /// <summary> 

        /// Роль данного сканера 

        /// </summary> 

        public ScannerRole ScannerRole 

        { 

            get 

            { 

                return SafeCall<ScannerRole>( 

                    () => { return _interactionChannel.ScannerRole; }, 

                    _commonCallProperties, 

                    ScannerRole.Undefined); 


            } 

        } 

 

 

        /// <summary> 

        /// Ожидает, когда роль сканера будет определена 

        /// </summary> 

        /// <returns>роль, которую принял сканер</returns> 

        public ScannerRole WaitForScannerRoleDefined() 

        { 

            return SafeCall<ScannerRole>( 

                    () => { return _interactionChannel.WaitForScannerRoleDefined(); }, 

                    _infiniteTimeoutCallProperties, 

                    ScannerRole.Undefined); 

        } 

 

 

        #endregion 

 

 

        #region Передача данных 

 

 

        /// <summary> 

        /// Положить данные в таблицу данных, принятых с удаленного сканера 

        /// </summary> 

        /// <param name="name"></param> 

        /// <param name="data"></param> 

        public void PutData(string name, object data) 

        { 

            SafeCall<object>( 

                () => { _interactionChannel.PutData(name, data); return null; }, 

                _commonCallProperties, 

                null); 

        } 

 

 

        #endregion 

 

 

        #region Состояние 

 

 

        /// <summary> 

        /// Текущее состояние - начальное? 

        /// </summary> 

        /// <remarks>начальное - т.е. оно еще не было ниоткуда загружено или  

        /// получено с удаленного сканера или оно было сброшено в начальное состояние</remarks> 

        public bool IsStateInitial 

        { 


            get 

            { 

                return SafeCall<bool>( 

                    () => { return _interactionChannel.IsStateInitial; }, 

                    _commonCallProperties,  

                    true); 

            } 

        } 

 

 

        /// <summary> 

        /// Архивирует текущее состояние и сбрасываеи его в начальное 

        /// </summary> 

        public void ResetState() 

        { 

            SafeCall<object>( 

                () => { _interactionChannel.ResetState(); return null; }, 

                _commonCallProperties,  

                null); 

        } 

 

 

        /// <summary> 

        /// Нужно синхронизировать состояния 

        /// </summary> 

        /// <param name="newStateItems">элементы состояния, которые были изменены и  

        /// по которым требуется синхронизация</param> 

        public void NeedSynchronizeState(List<StateItem> newStateItems) 

        { 

            SafeCall<object>( 

                () => { _interactionChannel.NeedSynchronizeState(newStateItems); return null; }, 

                _synchronizationCallProperties,  

                null); 

        } 

 

 

        /// <summary> 

        /// Синхронизация состояния завершена 

        /// </summary> 

        /// <param name="syncResult">результат синхронизации</param> 

        /// <remarks>этот метод вызывает удаленный сканер с целью сообщить, 

        /// что синхронизация состояния завершена</remarks> 

        public void StateSynchronizationFinished(SynchronizationResult syncResult) 

        { 

            SafeCall<object>( 

                () => { _interactionChannel.StateSynchronizationFinished(syncResult); return null; }, 

                _commonCallProperties,  

                null); 

        } 

 


 
        #endregion 

 

 

		#region Печать 

 

 

		/// <summary> 

		/// Подключен ли принтер к удаленному сканеру сканеру 

		/// </summary> 

		/// <returns>true - подключен/false - нет</returns> 

		public bool FindRemotePrinter() 

		{ 

			return SafeCall<bool>( 

				() => { return _interactionChannel.FindRemotePrinter(); }, 

                _commonCallProperties, 

				false); 

		} 

 

 

		/// <summary> 

		/// Распечатать отчет на удаленном принтере 

		/// </summary> 

		/// <returns></returns> 

		public bool RemotePrintReport(ReportType reportType, ListDictionary reportParameters) 

		{ 

			return SafeCall<bool>( 

				() => { return _interactionChannel.RemotePrintReport(reportType, reportParameters); }, 

                _printingCallProperties, 

				false); 

		} 

 

 

		#endregion 

 

 

		#region Сброс ПО 

 

 

		/// <summary> 

		/// Сброс По 

		/// </summary> 

		public void ResetSoft() 

		{ 

			SafeCall<object>( 

				() => { _interactionChannel.ResetSoft(); return null; }, 

				_synchronizationCallProperties, 

				null); 

		} 

 


 
		#endregion 

 

 

		#endregion 

 

 

		#region Безопасный вызов методов канала 

 

 

        /// <summary> 

        /// Событие "Связь с удаленным сканером потеряна" 

        /// </summary> 

        public event EventHandler Disconnected; 

        /// <summary> 

        /// Возбудить событие "Связь с удаленным сканером потеряна" 

        /// </summary> 

        private void RaiseDisconnected(Exception ex) 

        { 

            _logger.LogInfo(Message.SyncDisconnected, ex.Message); 

            Alive = false; 

 

 

            try 

            { 

                var handler = Disconnected; 

                if (handler != null) 

                    handler(this, EventArgs.Empty); 

            } 

            catch (Exception exc) 

            { 

                _logger.LogException(Message.Exception, exc, 

                    "Ошибка при обработке события потери связи с удаленным сканером"); 

            } 

        } 

 

 

        /// <summary> 

        /// Безопасный вызов метода, который возвращает значение 

        /// </summary> 

        /// <typeparam name="T"></typeparam> 

        /// <param name="method">делегат метода</param> 

        /// <param name="timeout">максимально допустимое время для выполнения метода.  

        /// Если метод не завершил выполнение в течение этого времени, то выполнится обработка, 

        /// как при потери связи с удаленным сканером</param> 

        /// <param name="returnOnError"> 

        /// значение, которое будет возвращено при возникновении ошибки при вызове метода</param> 

        /// <returns></returns> 

        private T SafeCall<T>(Func<T> method, CallPropertiesConfig callProps, T returnOnError) 

        { 


            if (!Alive) 

                return returnOnError; 

 

 

            int tryCount = 0; 

            while (true) 

            { 

                try 

                { 

                    IAsyncResult ar = method.BeginInvoke(null, null); 

 

 

                    if (!ar.AsyncWaitHandle.WaitOne(callProps.Timeout)) 

                    { 

						var methodInfo = (new StackTrace()).GetFrame(1).GetMethod(); 

                        var ex = new TimeoutException("Не дождались завершение выполнения метода: " + methodInfo.Name); 

 

 

                        RaiseDisconnected(ex); 

                        return returnOnError; 

                    } 

 

 

                    return method.EndInvoke(ar); 

                } 

                catch (Exception ex) 

                { 

                    // если  

                    if (// это сетевая ошибка 

                        (ex is SocketException || ex is RemotingException) && 

                        // И кол-во попыток еще не превысило максимальное 

                        ++tryCount < callProps.MaxTryCount) 

                    { 

                        // подождем в надежде, что за время ожидания работа сети восстановится 

                        Thread.Sleep(callProps.RetryDelay); 

                        continue; 

                    } 

 

 

                    RaiseDisconnected(ex); 

                    return returnOnError; 

                } 

            } 

        } 

 

 

        #endregion 

    } 

}



using System; 

using Croc.Bpc.Election.Voting; 

using System.Threading; 

 

 

namespace Croc.Bpc.Scanner 

{ 

    /// <summary> 

    /// Сессия обработки листа 

    /// </summary> 

    public class SheetProcessingSession 

    { 

        /// <summary> 

        /// Идентификатор сессии 

        /// </summary> 

        private Guid _id; 

		/// <summary> 

        /// Разрешено ли принятие листа 

        /// </summary> 

        public bool ReceivingAllowed = true; 

        /// <summary> 

        /// Результат голосования 

        /// </summary> 

        public VotingResult VotingResult 

        { 

            get; 

            internal set; 

        } 

        /// <summary> 

        /// Ошибка обработки листа 

        /// </summary> 

        public SheetProcessingError Error 

        { 

            get; 

            internal set; 

        } 

 

 

		/// <summary> 

		/// Результат выполнения сброса листа в данной сессии 

		/// </summary> 

		public DropResult DropResult 

		{ 

			get; 

			internal set; 

		} 

 

 

        /// <summary> 

        /// Закрыта ли данная сессия 


        /// </summary> 

        private volatile bool _closed = false; 

        /// <summary> 

        /// Закрыта ли данная сессия 

        /// </summary> 

        public bool Closed 

        { 

            get 

            { 

                return _closed; 

            } 

        } 

        /// <summary> 

        /// Событие "Сессия закрылась" 

        /// </summary> 

        private ManualResetEvent _closedEvent = new ManualResetEvent(false); 

		/// <summary> 

		/// Идентификатор сессии 

		/// </summary> 

		public Guid Id 

		{ 

			get { return _id; } 

		} 

 

 

        /// <summary> 

        /// Создает закрытую пустую сессию 

        /// </summary> 

        /// <remarks>используется для начальной инициализации</remarks> 

        internal static SheetProcessingSession ClosedEmptySession 

        { 

            get 

            { 

                var session = new SheetProcessingSession(Guid.Empty); 

                session.Close(); 

                return session; 

            } 

        } 

 

 

        public SheetProcessingSession() 

            : this(Guid.NewGuid()) 

        { 

        } 

 

 

        private SheetProcessingSession(Guid id) 

        { 

            _id = id; 

			// по умолчанию не знаем результата 


			DropResult = DropResult.Timeout; 

        } 

 

 

        /// <summary> 

        /// Закрыть сессию 

        /// </summary> 

        internal void Close() 

        { 

            _closed = true; 

            _closedEvent.Set(); 

        } 

 

 

        /// <summary> 

        /// Ожидать, когда сессия закроется 

        /// </summary> 

        public void WaitForClose() 

        { 

            if (_closed) 

                return; 

 

 

            _closedEvent.WaitOne(); 

        } 

    } 

}



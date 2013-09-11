using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using Croc.Bpc.Common.Diagnostics; 

using Croc.Workflow.ComponentModel; 

using Croc.Core.Diagnostics; 

using Croc.Bpc.Printing.Reports; 

using System.Collections.Specialized; 

 

 

namespace Croc.Bpc.Workflow.Activities 

{ 

    /// <summary> 

    /// Печать отчетов 

    /// </summary> 

    [Serializable] 

    public class PrintingActivity : BpcCompositeActivity 

    { 

        /// <summary> 

        /// Тип отчета 

        /// </summary> 

        public ReportType ReportType 

        { 

            get; 

            set; 

        } 

        /// <summary> 

        /// Словарь параметров, используемых при формировании документа [Название параметра; значение параметра] 

        /// </summary> 

        public ListDictionary ReportParameters 

        { 

            get; 

            set; 

        } 

        /// <summary> 

        /// Безусловная печать 

        /// </summary> 

        public bool ImplicitPrint 

        { 

            get; 

            set; 

        } 

        /// <summary> 

        /// Нужно ли печатать копии 

        /// </summary> 

        public bool NeedPrintCopies 

        { 

            get; 

            set; 


        } 

        /// <summary> 

        /// Имя звук. файла для воспроизведения названия отчета 

        /// </summary> 

        public string ReportTypeSound 

        { 

            get 

            { 

				switch (ReportType) 

				{ 

					case ReportType.SourceData: 

						return "print_sd"; 

					case ReportType.TestResults: 

						return "print_test_report"; 

					case ReportType.FailedControlRelations: 

						return "print_failed_cr"; 

                    case ReportType.PreliminaryElectionProtocol: 

                        return "print_preliminary_election_protocol"; 

					case ReportType.ElectionProtocol: 

						return "print_election_protocol"; 

					case ReportType.ResultsProtocol: 

						return "print_result_protocol"; 

 

 

					default: 

						throw new Exception("Неизвестный тип отчета"); 

				} 

            } 

        } 

 

 

        /// <summary> 

        /// Название отчета для отображения на индикаторе 

        /// </summary> 

        public string ReportTypeName 

        { 

            get 

            { 

                switch (ReportType) 

                { 

                    case ReportType.SourceData:  

                        return "Исходные данные"; 

                    case ReportType.TestResults: 

                        return "Протокол тестирования"; 

                    case ReportType.FailedControlRelations: 

                        return "Невыполненные КС"; 

                    case ReportType.PreliminaryElectionProtocol: 

                        return "Предварительный протокол по выборам"; 

                    case ReportType.ElectionProtocol: 

                        return "Протокол по выборам"; 


                    case ReportType.ResultsProtocol: 

                        return "Итоговый протокол"; 

 

 

                    default: 

                        throw new Exception("Неизвестный тип отчета"); 

                } 

            } 

        } 

 

 

		/// <summary> 

		/// Признак, что принтер подключен к удаленному сканеру 

		/// </summary> 

		private bool _isPrinterRemote = false; 

 

 

		/// <summary> 

		/// Инициализация 

		/// </summary> 

		protected override void  Initialize(WorkflowExecutionContext context) 

		{ 

 			base.Initialize(context); 

			// проставим значения по умолчанию 

			ReportParameters = new ListDictionary(); 

			NeedPrintCopies = true; 

		}  

 

 

		/// <summary> 

		/// Проверка обязательности печати без запроса пользователя 

		/// </summary> 

		/// <param name="context"></param> 

		/// <param name="parameters"></param> 

		/// <returns></returns> 

		public NextActivityKey CheckNeedToImplicitPrint(WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

		{ 

			return ImplicitPrint ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

		} 

 

 

		/// <summary> 

		/// Проверка обязательности печати без запроса пользователя 

		/// </summary> 

		/// <param name="context"></param> 

		/// <param name="parameters"></param> 

		/// <returns></returns> 

		public NextActivityKey CheckNeedPrintCopies(WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

		{ 

			return NeedPrintCopies ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 


		} 

 

 

		/// <summary> 

        /// Поиск принтера на текущем сканере 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 

        public NextActivityKey FindPrinter( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

            // сбросим признак, что принтер на подчиненном 

		    _isPrinterRemote = false; 

			// если принтер подключен к данному сканеру 

            return _printingManager.FindPrinter() ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

 

 

		/// <summary> 

		/// Поиск принтера на удаленном сканере 

		/// </summary> 

		/// <param name="context"></param> 

		/// <param name="parameters"></param> 

		/// <returns></returns> 

		public NextActivityKey FindRemotePrinter( 

			WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

		{ 

            // сбросим признак, что принтер на подчиненном 

            _isPrinterRemote = false; 

 

 

            // если принтер подключен к удаленному сканеру 

			if (_syncManager.RemoteScanner.FindRemotePrinter()) 

			{ 

				_isPrinterRemote = true; 

				return BpcNextActivityKeys.Yes; 

			} 

 

 

			return BpcNextActivityKeys.No; 

		} 

 

 

        /// <summary> 

        /// Печать отчета 

        /// </summary> 

        /// <param name="context"></param> 

        /// <param name="parameters"></param> 

        /// <returns></returns> 


        public NextActivityKey PrintReport( 

            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 

        { 

			// выводим на индикатор "Подготовка печати..." 

			_scannerManager.SetIndicator("Подготовка печати..."); 

 

 

            // если принтер на удаленном сканере 

            if (_isPrinterRemote) 

                return _syncManager.RemoteScanner.RemotePrintReport(ReportType, ReportParameters) 

                           ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

 

 

            // если принтер на данном сканере 

			return _printingManager.PrintReport(ReportType, ReportParameters) ? BpcNextActivityKeys.Yes : BpcNextActivityKeys.No; 

        } 

    } 

}



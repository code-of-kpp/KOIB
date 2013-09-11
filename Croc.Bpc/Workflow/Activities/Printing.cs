using System; 
using System.Collections.Specialized; 
using Croc.Bpc.Printing; 
using Croc.Workflow.ComponentModel; 
namespace Croc.Bpc.Workflow.Activities 
{ 
    [Serializable] 
    public class PrintingActivity : BpcCompositeActivity 
    { 
        private bool _needPrintCopies; 
        public int CopiesNumber 
        { 
            get; set; 
        } 
        public ReportType ReportType 
        { 
            get; 
            set; 
        } 
        public ListDictionary ReportParameters 
        { 
            get; 
            set; 
        } 
        public bool ImplicitPrint 
        { 
            get; 
            set; 
        } 
        public bool NeedPrintCopies 
        { 
            get 
            { 
                return _needPrintCopies && !NeedCopiesCountNumber; 
            } 
            set 
            { 
                _needPrintCopies = value; 
            } 
        } 
        public bool NeedCopiesCountNumber 
        { 
            get; 
            set; 
        } 
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
        public bool NotGenerateNewReport 
        { 
            get; 
            set; 
        } 
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
        private bool _isPrinterRemote; 
        private PrinterJob _printerJob; 
        protected override void Initialize(WorkflowExecutionContext context) 
        { 
            base.Initialize(context); 
            ReportParameters = new ListDictionary(); 
            NeedPrintCopies = true; 
            NeedCopiesCountNumber = false; 
            ImplicitPrint = false; 
            NotGenerateNewReport = false; 
            CopiesNumber = 1; 
        } 
        public NextActivityKey CheckGenerateNewReport 
            (WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            if (_printerJob != null && (_printerJob.ReportType != ReportType || !NotGenerateNewReport)) 
                _printerJob = null; 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey CheckNeedToImplicitPrint 
            (WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return ImplicitPrint 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey CheckNeedPrintCopies 
            (WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return NeedPrintCopies 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey CheckNeedCopiesNumber 
            (WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            return NeedCopiesCountNumber 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey AsseptCopiesNumberValue 
            (WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            var copies = int.Parse(CommonActivity.LastReadedValue); 
            CopiesNumber = (copies == 0 ? 1 : copies); 
            return context.DefaultNextActivityKey; 
        } 
        public NextActivityKey FindPrinter( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _isPrinterRemote = false; 
            return _printingManager.FindPrinter() 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey FindRemotePrinter( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _isPrinterRemote = _syncManager.RemoteScanner.FindPrinter(); 
            return _isPrinterRemote 
                       ? BpcNextActivityKeys.Yes 
                       : BpcNextActivityKeys.No; 
        } 
        public NextActivityKey CreateReport( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _scannerManager.SetIndicator("Подготовка печати..."); 
            if (_printerJob == null) 
            { 
                _printerJob = _isPrinterRemote 
                                  ? _syncManager.RemoteScanner.CreateReport(ReportType, ReportParameters, CopiesNumber) 
                                  : _printingManager.CreateReport(ReportType, ReportParameters, CopiesNumber); 
            } 
            return _printerJob == null 
                       ? BpcNextActivityKeys.No     // если отчет так и не сформировался то ошибка 
                       : BpcNextActivityKeys.Yes; 
        } 
        public NextActivityKey PrintReport( 
            WorkflowExecutionContext context, ActivityParameterDictionary parameters) 
        { 
            _scannerManager.SetIndicator(parameters.GetParamValue<string>("Text")); 
            _printerJob.Copies = CopiesNumber; 
            var printResult = _isPrinterRemote 
                                  ? _syncManager.RemoteScanner.PrintReport(_printerJob) 
                                  : _printingManager.PrintReport(_printerJob); 
            CopiesNumber = 1; 
            return !printResult 
                       ? BpcNextActivityKeys.No 
                       : BpcNextActivityKeys.Yes; 
        } 
    } 
}
